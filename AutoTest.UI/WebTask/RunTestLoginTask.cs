using AutoMapper;
using AutoTest.Domain.Entity;
using AutoTest.Domain.Model;
using AutoTest.UI.EventListener;
using AutoTest.UI.WebBrowser;
using AutoTest.Util;
using CefSharp;
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
        private TestSite _testSite;

        private TestLogin _testLogin;
        private TestEnv _testEnv;
        private List<TestEnvParam> _testEnvParams;
        private bool _readyFlag;
        private dynamic bag;

        public RunTestLoginTask(string taskname, bool useProxy, TestSite testSite, 
            TestLogin testLogin,TestEnv testEnv, List<TestEnvParam> testEnvParams) 
            : base(taskname,Util.ReplaceEvnParams(testLogin.Url,testEnvParams), useProxy, false)
        {

            eventListener = new TestEventListener();
            eventListener.OnProcess += ProcessWebEvent;

            pageRequestAutoResetEvent = new RequestAutoResetEvent(true);

            mapper = AutofacBuilder.GetFromFac<IMapper>();

            _testSite = testSite;
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

        protected override async Task<int> ExecuteInner(IBrowser browser, IFrame frame)
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
    }
}
