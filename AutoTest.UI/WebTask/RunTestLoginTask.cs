using AutoMapper;
using AutoTest.Domain.Entity;
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
        private readonly IWebBrowserTool webBrowserTool = null;
        private readonly RequestAutoResetEvent pageRequestAutoResetEvent = null;
        private readonly IMapper mapper = null;
        private TestSite _testSite;

        private TestLogin _testLogin;
        private bool _readyFlag;
        private dynamic bag;

        public RunTestLoginTask(string taskname, bool useProxy, TestSite testSite, TestLogin testLogin) : base(taskname, testLogin.Url, useProxy, false)
        {

            eventListener = new TestEventListener();
            eventListener.OnProcess += ProcessWebEvent;
            webBrowserTool = new WebBrowserTool();

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

        protected override async Task<int> ExecuteInner(IBrowser browser, IFrame frame)
        {
            if (!string.IsNullOrWhiteSpace(_testLogin.LoginCode))
            {
                var tryCount = 0;
                dynamic ret;

                while (true)
                {
                    try
                    {
                        string varCode = "";
                        if (bag != null)
                        {
                            varCode = $@"eval('_$$=_$$||{{}};\
                            _$$.bag={Newtonsoft.Json.JsonConvert.SerializeObject(bag)}');";
                        }
                        ret = webBrowserTool.ExecutePromiseScript(browser, frame, varCode + _testLogin.LoginCode) as dynamic;
                        if (ret != null)
                        {
                            bag = ret.bag;
                            if (!ret.ret)
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
                }

            }
            return await Task.FromResult(1);
        }


        /// <summary>
        /// 监听底层数据方法
        /// </summary>
        /// <param name="webEvent"></param>
        /// <returns></returns>
        private bool ProcessWebEvent(WebEvent webEvent)
        {
            return false;
        }
    }
}
