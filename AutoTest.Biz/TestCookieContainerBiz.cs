using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Biz
{
    public static class TestCookieContainerBiz
    {
        public static TestCookieContainer GetTestCookieContainer(int siteId, int? envId, int loginId)
        {
            if (envId == null)
            {
                envId = 0;
            }
            var container = BigEntityTableRemotingEngine.Find<TestCookieContainer>(nameof(TestCookieContainer), TestCookieContainer.IX,
                                new object[] { siteId, envId, loginId }).FirstOrDefault();

            return container;
        }

        public static List<TestCookie> GetCookies(int siteId,int? envId,int loginId)
        {
            if (envId == null)
            {
                envId = 0;
            }
            var cookieContainer = BigEntityTableRemotingEngine.Find<TestCookieContainer>(nameof(TestCookieContainer), TestCookieContainer.IX,
                             new object[] { siteId, envId, loginId }).FirstOrDefault();

            if (cookieContainer == null || cookieContainer.Expires < DateTime.Now)
            {
                return null;
            }

            return cookieContainer.TestCookies;
        }

        public static void SetExpired(int siteId, int? envId, int loginId)
        {
            if (envId == null)
            {
                envId = 0;
            }
            var cookieContainer = BigEntityTableRemotingEngine.Find<TestCookieContainer>(nameof(TestCookieContainer), TestCookieContainer.IX,
                             new object[] { siteId, envId, loginId }).FirstOrDefault();

            if (cookieContainer == null || cookieContainer.Expires < DateTime.Now)
            {
                return;
            }

            cookieContainer.Expires = DateTime.Now.AddYears(-1);

            BigEntityTableRemotingEngine.Update(nameof(TestCookieContainer), cookieContainer);
        }

        public static bool Upsert(TestCookieContainer cookieContainer)
        {
            return BigEntityTableRemotingEngine.Upsert(nameof(TestCookieContainer), cookieContainer);
        }
    }
}