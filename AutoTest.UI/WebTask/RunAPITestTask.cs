using AutoTest.Domain.Entity;
using AutoTest.UI.EventListener;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebTask
{
    public class RunAPITestTask : WebTask
    {
        public RunAPITestTask(string taskname, bool useProxy, TestSite testSite, TestLogin testLogin,
            TestPage testPage, TestCase testCase, TestEnv testEnv, List<TestEnvParam> testEnvParams,
            List<TestScript> globScripts, List<TestScript> siteScripts, Action<TestResult> notify)
            : base(taskname, Util.ReplaceEvnParams(string.IsNullOrWhiteSpace(testCase.Url)?testPage.Url:testCase.Url, testEnvParams),
                  useProxy, false)
        {

        }

        public override void DocumentCompletedHandler(IBrowser browser, IFrame frame)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Cookie> GetCookieList()
        {
            throw new NotImplementedException();
        }

        public override IEventListener GetEventListener()
        {
            throw new NotImplementedException();
        }

        public override string GetSite()
        {
            throw new NotImplementedException();
        }

        protected override Task<int> ExecuteInner(IBrowser browser, IFrame frame, ICookieManager cookieManager)
        {
            throw new NotImplementedException();
        }

        public override IRequest GetTestRequest(IRequest request)
        {
            base.GetTestRequest(request);

            return request;
        }
    }
}
