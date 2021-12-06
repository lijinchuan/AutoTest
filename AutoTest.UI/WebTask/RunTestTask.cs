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
    public class RunTestTask : WebTask
    {
        private readonly IEventListener eventListener = null;
        private readonly IWebBrowserTool webBrowserTool = null;
        private readonly RequestAutoResetEvent pageRequestAutoResetEvent = null;
        private readonly IMapper mapper = null;
        private static string defaultUrl = "about:blank";

        private TestSite _testSite;
        private TestPage _testPage;
        private TestCase _testCase;

        public RunTestTask(string taskname, bool useProxy, TestSite testSite, TestPage testPage, TestCase testCase) : base(taskname, testPage.Url, useProxy, false)
        {

            eventListener = new TestEventListener();
            eventListener.OnProcess += ProcessWebEvent;
            webBrowserTool = new WebBrowserTool();

            pageRequestAutoResetEvent = new RequestAutoResetEvent(true);

            mapper = AutofacBuilder.GetFromFac<IMapper>();

            _testSite = testSite;
            _testPage = testPage;
            _testCase = testCase;
        }

        public override void DocumentCompletedHandler(IBrowser browser, IFrame frame, List<Cookie> cookies)
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
            return GetTaskName();
        }

        protected override async Task<int> ExecuteInner(IBrowser browser, IFrame frame)
        {
            if (!string.IsNullOrWhiteSpace(_testCase.TestCode))
            {
                var tryCount = 0;
                object ret;
                while (true)
                {
                    try
                    {
                        ret = webBrowserTool.ExecutePromiseScript(browser, frame, _testCase.TestCode);
                        if (object.Equals(ret, false))
                        {
                            if (tryCount > 30)
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
                        if (tryCount > 30)
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
