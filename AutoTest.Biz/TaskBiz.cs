using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Biz
{
    public class TaskBiz
    {
        public TestTask CreateTask(int caseId)
        {
            var testCase = BigEntityTableEngine.LocalEngine.Find<TestCase>(nameof(TestCase), caseId);

            if (testCase != null)
            {
                var page = BigEntityTableEngine.LocalEngine.Find<TestPage>(nameof(TestPage), testCase.PageId);
                var site = BigEntityTableEngine.LocalEngine.Find<TestSite>(nameof(TestSite), page.SiteId);
                var source = BigEntityTableEngine.LocalEngine.Find<TestSource>(nameof(TestSource), site.SourceId);
                var scripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == source.Id).ToList();
                var testLogins = BigEntityTableRemotingEngine.Find<TestLogin>(nameof(TestLogin), nameof(TestLogin.SiteId), new object[] { site.Id });

                var testEnvs = BigEntityTableRemotingEngine.Find<TestEnv>(nameof(TestEnv), nameof(TestEnv.SiteId), new object[] { site.Id });
                var currentEnv = testEnvs.FirstOrDefault(p => p.Used);
                List<TestEnvParam> testEnvParams = null;
                if (currentEnv != null)
                {
                    testEnvParams = BigEntityTableRemotingEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_EnvId", new object[] { site.Id, currentEnv.Id }).ToList();
                }
                var newTask = new TestTask
                {
                    TestSource = source,
                    SiteTestScripts = scripts.Where(p => p.SiteId > 0).ToList(),
                    GlobalTestScripts = scripts.Where(p => p.SiteId == 0).ToList(),
                    TestCase = testCase,
                    TestLogin = testLogins.Count()<=1?testLogins.FirstOrDefault():testLogins.FirstOrDefault(p=>p.Used),
                    TestPage = page,
                    TestSite = site,
                    TestEnv = currentEnv,
                    TestEnvParams = testEnvParams
                };

                return newTask;
            }

            return null;
        }
    }
}
