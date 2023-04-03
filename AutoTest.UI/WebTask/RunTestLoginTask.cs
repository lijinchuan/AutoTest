using AutoMapper;
using AutoTest.Biz;
using AutoTest.Domain.Entity;
using AutoTest.Domain.Model;
using AutoTest.UI.EventListener;
using AutoTest.UI.WebBrowser;
using AutoTest.Util;
using CefSharp;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.UI.WebTask
{
    public class RunTestLoginTask : WebTask
    {
        private readonly IEventListener eventListener = null;
        private readonly RequestAutoResetEvent pageRequestAutoResetEvent = null;
        private readonly IMapper mapper = null;
        private readonly TestSite _testSite;

        private List<TestScript> _globScripts;
        private List<TestScript> _siteScripts;

        private readonly TestLogin _testLogin;
        private TestEnv _testEnv;
        private readonly List<TestEnvParam> _testEnvParams;
        private readonly dynamic bag;

        public RunTestLoginTask(string taskname, bool useProxy, TestSite testSite,
            TestLogin testLogin, TestEnv testEnv, List<TestEnvParam> testEnvParams, List<TestScript> globScripts, List<TestScript> siteScripts)
            : base(taskname, Util.ReplaceEvnParams(testLogin.Url, testEnvParams), useProxy, false, testEnv, testEnvParams)
        {

            eventListener = new TestEventListener();
            eventListener.OnProcess += ProcessWebEvent;

            pageRequestAutoResetEvent = new RequestAutoResetEvent(true);

            mapper = AutofacBuilder.GetFromFac<IMapper>();

            _testSite = testSite;
            _testLogin = testLogin;
            _testEnv = testEnv;
            _globScripts = globScripts;
            _siteScripts = siteScripts;
        }

        public override void DocumentCompletedHandler(IBrowser browser, IFrame frame)
        {

            FireTaskReady();
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
            return _testSite.Name;
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

            webBrowserTool.ExecuteScript(browser, frame, code);
        }

        private void PrepareTest(IBrowser browser, IFrame frame, object userData)
        {
            if (!string.IsNullOrWhiteSpace(_testLogin.LoginCode) || !string.IsNullOrWhiteSpace(_testLogin.ValidCode))
            {
                //注入JQUERY
                webBrowserTool.AddJqueryLib(browser, frame);

                //注入工具包
                if (_globScripts != null && _globScripts.Count > 0)
                {
                    foreach (var s in _globScripts.OrderBy(p => p.Order))
                    {
                        RegistTestScript(browser, frame, s);
                    }
                }

                PublishDebugMsg("注入全局工具包");


                if (_siteScripts != null && _siteScripts.Count > 0)
                {
                    foreach (var s in _siteScripts.OrderBy(p => p.Order))
                    {
                        RegistTestScript(browser, frame, s);
                    }
                }

                PublishDebugMsg("注入通用工具包");

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

        private async Task<int> RunTestCode(IBrowser browser, IFrame frame)
        {
            dynamic bag = null;
            if (!string.IsNullOrWhiteSpace(_testLogin.LoginCode))
            {
                var tryCount = 0;

                while (true)
                {
                    if (_cancelFlag)
                    {
                        throw new Exception("任务取消");
                    }
                    try
                    {
                        webBrowserTool.WaitLoading(browser, _cancelFlag, false);
                        PrepareTest(browser, frame, bag);
                        var ret = webBrowserTool.ExecutePromiseScript(browser, frame, Util.ReplaceEvnParams(_testLogin.LoginCode, _testEnvParams));
                        if (object.Equals(ret, false))
                        {
                            //if (tryCount++ > 30)
                            //{
                            //    return await Task.FromResult(0);
                            //}
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
            if (!string.IsNullOrWhiteSpace(_testLogin.ValidCode))
            {
                var tryCount = 0;

                while (true)
                {
                    if (_cancelFlag)
                    {
                        throw new Exception("任务取消");
                    }
                    try
                    {
                        webBrowserTool.WaitLoading(browser, _cancelFlag, false);
                        PrepareTest(browser, frame, bag);
                        var ret = webBrowserTool.TryExecuteScript(browser, frame, Util.ReplaceEvnParams(_testLogin.ValidCode, _testEnvParams));
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
                            if (validResult == 0 && _testLogin.IsMannual)
                            {
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                break;
                            }
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

        protected override async Task<int> ExecuteInner(IBrowser browser, IFrame frame,ICookieManager cookieManager)
        {
            var ret = 0;
            try
            {
                ret = await RunTestCode(browser, frame);
                if (ret == 1)
                {
                    ret = await RunValidCode(browser, frame);
                    if (ret == 1)
                    {
                        //保存下COOKIE
                        using (var visiter = new CookieVisitor(cookieManager))
                        {
                            var container = TestCookieContainerBiz.GetTestCookieContainer(_testSite.Id, _testEnv?.Id, _testLogin.Id);
                            if (container == null) {
                                container = new TestCookieContainer()
                                {
                                    Account = _testLogin.Id,
                                    CreateTime = DateTime.Now,
                                    Expires = DateTime.Now.AddYears(1),
                                    Env = _testEnv?.Id ?? 0,
                                    SiteId = _testSite.Id,
                                    TestCookies = new List<TestCookie>()
                                };
                            }
                            else
                            {
                                container.CreateTime = DateTime.Now;
                                container.Expires = DateTime.Now.AddYears(1);
                                container.TestCookies.Clear();
                            }
                            var list = visiter.GetCookies(GetStartPageUrl()).Result;
                            foreach(var li in list)
                            {
                                //li.Creation
                                container.TestCookies.Add(new TestCookie
                                {
                                    Domain=li.Domain,
                                    Expires=li.Expires??DateTime.Now.AddYears(1),
                                    HttpOnly=li.HttpOnly,
                                    Name=li.Name,
                                    Path=li.Path,
                                    Priority=(int)li.Priority,
                                    SameSite=(int)li.SameSite,
                                    Secure=li.Secure,
                                    Value=li.Value
                                });
                            }

                            TestCookieContainerBiz.Upsert(container);
                        }
                        PublishMsg($"登陆成功");
                    }
                    else
                    {
                        PublishMsg($"登陆失败");
                    }
                }
            }
            catch (Exception ex)
            {
                PublishMsg($"登陆出错:{ex.Message}");
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

        public override TestCase GetTestCase()
        {
            return null;
        }
    }
}
