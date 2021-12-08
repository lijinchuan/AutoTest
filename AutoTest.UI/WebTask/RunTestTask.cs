using AutoMapper;
using AutoTest.Domain.Entity;
using AutoTest.Domain.Model;
using AutoTest.UI.EventListener;
using AutoTest.UI.WebBrowser;
using AutoTest.Util;
using CefSharp;
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
        private List<WebEvent> webEvents = new List<WebEvent>();

        public RunTestTask(string taskname, bool useProxy, TestSite testSite,TestLogin testLogin,
            TestPage testPage, TestCase testCase,TestEnv testEnv,List<TestEnvParam> testEnvParams) 
            : base(taskname,Util.ReplaceEvnParams(testPage.Url,testEnvParams), useProxy, false)
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
        }

        public override void DocumentCompletedHandler(IBrowser browser, IFrame frame, List<Cookie> cookies)
        {
            if (!_readyFlag)
            {
                _readyFlag = true;
                FireTaskReady();
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

        private void PrepareTest(IBrowser browser, IFrame frame,object userData)
        {
            if (!string.IsNullOrWhiteSpace(_testCase.TestCode) || !string.IsNullOrWhiteSpace(_testCase.ValidCode))
            {
                //注入JQUERY
                webBrowserTool.AddJqueryLib(browser, frame);

                //注入工具包

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
            //

            dynamic bag = null;
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
                            if (tryCount++ > 30)
                            {
                                return await Task.FromResult(0);
                            }
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (tryCount++ > 30)
                        {
                            return await Task.FromResult(0);
                        }
                        Thread.Sleep(1000);
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
            if (!string.IsNullOrWhiteSpace(_testCase.ValidCode))
            {
                var tryCount = 0;

                while (true)
                {
                    try
                    {
                        PrepareTest(browser, frame, bag);
                        var ret = webBrowserTool.ExecutePromiseScript(browser, frame, Util.ReplaceEvnParams(_testCase.ValidCode, _testEnvParams));
                        if (ret == null)
                        {
                            if (tryCount++ > 30)
                            {
                                return await Task.FromResult(0);
                            }
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            validResult = object.Equals(ret, true) ? 1 : 0;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (tryCount++ > 30)
                        {
                            return await Task.FromResult(0);
                        }
                        Thread.Sleep(1000);
                    }
                    finally
                    {
                        bag = GetUserVarData(browser, frame);
                    }
                }

            }
            return await Task.FromResult(validResult);
        }

        private bool Check(IBrowser browser, IFrame frame)
        {
            var flag = true;
            if (!string.IsNullOrWhiteSpace(_testSite.CheckLoginCode))
            {
                var isLogin = webBrowserTool.ExecuteScript(browser, frame, _testSite.CheckLoginCode);
                if (!object.Equals(isLogin, true))
                {
                    flag = false;
                    var loginTask = new RunTestLoginTask(_testSite.Name + "登陆", UseProxy, _testSite, _testLogin, _testEnv, _testEnvParams);
                    loginTask.SetNext(new RunTestTask(GetTaskName(), UseProxy, _testSite, _testLogin, _testPage, _testCase, _testEnv, _testEnvParams));
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
    }
}
