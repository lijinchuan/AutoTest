﻿using AutoMapper;
using AutoTest.Domain.Entity;
using AutoTest.Domain.Model;
using AutoTest.UI.EventListener;
using AutoTest.UI.WebBrowser;
using AutoTest.Util;
using CefSharp;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.UI.WebTask
{
    public class RunTestTask : WebTask
    {
        private const int TestTimeOut = 60000;

        private readonly IEventListener eventListener = null;
        private readonly RequestAutoResetEvent pageRequestAutoResetEvent = null;
        private readonly IMapper mapper = null;

        private TestSite _testSite;
        private TestPage _testPage;
        private TestLogin _testLogin;
        private TestCase _testCase;
        private TestEnv _testEnv;
        private List<TestEnvParam> _testEnvParams;
        private bool _readyFlag = false;
        private TestCaseData _testCaseData;
        private List<WebEvent> webEvents = new List<WebEvent>();
        private List<TestScript> _globScripts;
        private List<TestScript> _siteScripts;
        private TestResult _testResult;

        public RunTestTask(string taskname, bool useProxy, TestSite testSite, TestLogin testLogin,
            TestPage testPage, TestCase testCase, TestEnv testEnv, List<TestEnvParam> testEnvParams,
            List<TestScript> globScripts, List<TestScript> siteScripts)
            : base(taskname, Util.ReplaceEvnParams(string.IsNullOrWhiteSpace(testCase.Url)?testPage.Url:testCase.Url, testEnvParams),
                  useProxy, false)
        {

            eventListener = new TestEventListener();
            eventListener.OnProcess += ProcessWebEvent;

            pageRequestAutoResetEvent = new RequestAutoResetEvent(true);

            mapper = AutofacBuilder.GetFromFac<IMapper>();

            _testSite = testSite;
            _testPage = testPage;
            _testCase = testCase;
            _testEnv = testEnv;
            _testEnvParams = testEnvParams;
            _testLogin = testLogin;
            _testCaseData = BigEntityTableEngine.LocalEngine.Find<TestCaseData>(nameof(TestCaseData), nameof(TestCaseData.TestCaseId), new object[] { _testCase.Id }).FirstOrDefault();
            _globScripts = globScripts;
            _siteScripts = siteScripts;

            _testResult = new TestResult
            {
                EnvId=_testEnv.Id,
                TestCaseId=_testCase.Id,
                Success=false,
                TestStartDate=DateTime.Now,
                TestEndDate=DateTime.Now,
                IsTimeOut=false
            };
        }

        public override void DocumentCompletedHandler(IBrowser browser, IFrame frame, List<Cookie> cookies)
        {
            //if (browser.MainFrame.Url.IndexOf(this.GetStartPageUrl(), StringComparison.OrdinalIgnoreCase) > -1)
            {
                if (!_readyFlag)
                {
                    _readyFlag = true;
                    FireTaskReady();
                }
            }
        }

        public override IEnumerable<Cookie> GetCookieList()
        {
            return new List<Cookie>();
        }

        public override IEventListener GetEventListener()
        {
            return eventListener;
        }

        public override string GetSite()
        {
            return GetTaskName();
        }

        private void RegistTestScript(IBrowser browser, IFrame frame, TestScript testScript)
        {
            if (!testScript.Enable || string.IsNullOrWhiteSpace(testScript.Body))
            {
                return;
            }

            if (Regex.Match(testScript.Body.TrimStart(), "^(https?:|//)", RegexOptions.IgnoreCase).Success)
            {
                webBrowserTool.RegisterRomoteScript(browser, frame, testScript.Body.TrimStart());
            }
            else
            {
                webBrowserTool.ExecuteScript(browser, frame, testScript.Body);
            }
        }

        private void PrepareTest(IBrowser browser, IFrame frame,object userData)
        {
            if (!string.IsNullOrWhiteSpace(_testCase.TestCode) || !string.IsNullOrWhiteSpace(_testCase.ValidCode))
            {
                //注入JQUERY
                webBrowserTool.AddJqueryLib(browser, frame);

                //注入工具包
                if (_globScripts != null && _globScripts.Count > 0)
                {
                    foreach(var s in _globScripts.OrderBy(p => p.Order))
                    {
                        RegistTestScript(browser, frame, s);
                    }
                }

                if (_siteScripts != null && _siteScripts.Count > 0)
                {
                    foreach (var s in _siteScripts.OrderBy(p => p.Order))
                    {
                        RegistTestScript(browser, frame, s);
                    }
                }

                //注入变量
                SetVar(browser, frame, userData);
            }
        }


        /// <summary>
        /// 获取用户变量数据
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        private dynamic GetUserVarData(IBrowser browser, IFrame frame)
        {
            var code = $"if(typeof {WebVar.VarName}!='undefined'&&{WebVar.VarName}.{nameof(WebVar.Bag)}) return {WebVar.VarName}.{nameof(WebVar.Bag)}";

            return webBrowserTool.ExecutePromiseScript(browser, frame, code) as dynamic;
        }

        private void SetVar(IBrowser browser, IFrame frame, object userData)
        {
            var webRequestDatas = webEvents.Select(p => new WebRequestData
            {
                Code = p.StatusCode,
                ResponseContent = p.Content,
                Url = p.SourceUrl
            }).ToList();

            if (userData == null)
            {
                userData = new object();
            }

            var code = $"var {WebVar.VarName}={WebVar.VarName}||{{}};\n";

            code += $"{WebVar.VarName}.{nameof(WebVar.Bag)}={WebVar.VarName}.{nameof(WebVar.Bag)}||{Newtonsoft.Json.JsonConvert.SerializeObject(userData)}\n";

            code += $"{WebVar.VarName}.{nameof(WebVar.WebRequestDatas)}={Newtonsoft.Json.JsonConvert.SerializeObject(webRequestDatas)}\n";

            var ret = webBrowserTool.ExecuteScript(browser, frame, code);
        }

        private async Task<int> RunTestCode(IBrowser browser, IFrame frame)
        {

            dynamic bag = null;
            int sleepMills = 1000;
            if (!string.IsNullOrWhiteSpace(_testCase.TestCode))
            {
                var tryCount = 0;

                while (true)
                {
                    try
                    {
                        PrepareTest(browser, frame, bag);
                        var ret = webBrowserTool.ExecutePromiseScript(browser, frame, Util.ReplaceEvnParams(_testCase.TestCode, _testEnvParams));
                        if (object.Equals(ret, false))
                        {
                            if (tryCount++ >= TestTimeOut / sleepMills)
                            {
                                PublishMsg("超时");
                                return await Task.FromResult(0);
                            }
                            Thread.Sleep(sleepMills);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        PublishMsg(ex.Message);
                        if (tryCount++ > TestTimeOut / sleepMills)
                        {
                            PublishMsg("超时");
                            return await Task.FromResult(0);
                        }
                        Thread.Sleep(sleepMills);
                    }
                    finally
                    {
                        bag = GetUserVarData(browser, frame);
                    }
                }

            }

            return await Task.FromResult(1);

        }

        private async Task<int> RunValidCode(IBrowser browser, IFrame frame)
        {
            dynamic bag = null;
            int validResult = 0;
            int sleepMills = 1000;
            try
            {
                if (!string.IsNullOrWhiteSpace(_testCase.ValidCode))
                {
                    var tryCount = 0;

                    while (true)
                    {
                        try
                        {
                            PrepareTest(browser, frame, bag);
                            var ret = webBrowserTool.TryExecuteScript(browser, frame, Util.ReplaceEvnParams(_testCase.ValidCode, _testEnvParams));
                            if (ret == null)
                            {
                                if (tryCount++ >= TestTimeOut / sleepMills)
                                {
                                    _testResult.IsTimeOut = true;
                                    _testResult.FailMsg = "检查结果一直为NULL";
                                    return await Task.FromResult(0);
                                }
                                Thread.Sleep(sleepMills);
                            }
                            else
                            {
                                validResult = object.Equals(ret, true) ? 1 : 0;
                                _testResult.Success = object.Equals(ret, true);
                                _testResult.FailMsg = "检查结果返回true/false";
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (tryCount++ >= TestTimeOut / sleepMills)
                            {
                                _testResult.IsTimeOut = true;
                                _testResult.FailMsg = "检查出错：" + ex.Message;
                                return await Task.FromResult(0);
                            }
                            Thread.Sleep(sleepMills);
                        }
                        finally
                        {
                            bag = GetUserVarData(browser, frame);
                        }
                    }
                }
                else
                {
                    _testResult.Success = true;
                    _testResult.FailMsg = "没有验证脚本，默认为成功";
                }
            }
            catch (Exception ex)
            {
                _testResult.Success = false;
                _testResult.FailMsg = ex.Message;
                PublishMsg(ex.Message);
                throw ex;
            }
            finally
            {
                if (bag != null)
                {
                    _testResult.ResultContent = Newtonsoft.Json.JsonConvert.SerializeObject(bag);
                }
                _testResult.TestEndDate = DateTime.Now;
                BigEntityTableEngine.LocalEngine.Insert(nameof(TestResult), _testResult);
            }
            return await Task.FromResult(validResult);
        }

        private bool Check(IBrowser browser, IFrame frame)
        {
            var flag = true;
            if (!string.IsNullOrWhiteSpace(_testSite.CheckLoginCode))
            {
                var isLogin = webBrowserTool.TryExecuteScript(browser, frame, _testSite.CheckLoginCode);
                if (!object.Equals(isLogin, true))
                {
                    flag = false;
                    var loginTask = new RunTestLoginTask(_testSite.Name + "登陆", UseProxy, _testSite, _testLogin, _testEnv, _testEnvParams);
                    loginTask.SetNext(new RunTestTask(GetTaskName(), UseProxy, _testSite, _testLogin, _testPage, _testCase, _testEnv, _testEnvParams,_globScripts,_siteScripts));
                    this.SetNext(loginTask);
                }
            }
            return flag;
        }

        protected override async Task<int> ExecuteInner(IBrowser browser, IFrame frame)
        {
            if (!Check(browser, frame))
            {
                return await Task.FromResult(0);
            }

            var ret = 0;
            try
            {
                ret = await RunTestCode(browser, frame);
                if (ret == 1)
                {
                    ret = await RunValidCode(browser, frame);
                    if (ret == 1)
                    {
                        PublishMsg($"{_testCase.CaseName}测试成功");
                    }
                    else
                    {
                        PublishMsg($"{_testCase.CaseName}测试失败");
                    }
                }
            }
            catch (Exception ex)
            {
                PublishMsg($"{_testCase.CaseName}出错:{ex.Message}");
            }
            return await Task.FromResult(ret);
        }

        /// <summary>
        /// 监听底层数据方法
        /// </summary>
        /// <param name="webEvent"></param>
        /// <returns></returns>
        private bool ProcessWebEvent(WebEvent webEvent)
        {
            webEvents.Add(webEvent);
            return false;
        }

        public override string GetStartPageUrl()
        {
            var url = base.GetStartPageUrl();
            var paramlist2 =_testCaseData.Params?.Where(p => p.Checked);
            if (paramlist2?.Count() > 0)
            {
                if (url.IndexOf("?") == -1)
                {
                    url += "?";
                }

                url += string.Join("&", paramlist2.Select(p => $"{System.Net.WebUtility.UrlEncode(Util.ReplaceEvnParams(p.Name, _testEnvParams))}={System.Net.WebUtility.UrlEncode(Util.ReplaceEvnParams(p.Value, _testEnvParams))}"));
            }
            return url;
        }
    }
}
