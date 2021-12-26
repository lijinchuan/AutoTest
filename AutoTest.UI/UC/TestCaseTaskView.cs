using AutoTest.Domain;
using AutoTest.Domain.Entity;
using AutoTest.UI.SubForm;
using AutoTest.UI.WebTask;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace AutoTest.UI.UC
{
    public partial class TestCaseTaskView : TabPage, IRecoverAble
    {
        private static TestLogin currentTestLogin = null;
        public event Action<IWebTask> OnTaskStart;

        private List<TestSource> _testSources = null;
        private List<TestSite> _testSites = null;
        private List<TestPage> _testPages = null;
        private List<TestTask> _testCases = null;
        private List<int> _testCasesChoose = null;

        public TestCaseTaskView()
        {
            InitializeComponent();
            BtnCancel.Enabled = false;
        }

        public TestCaseTaskView Init(List<TestSource> testSources, List<TestSite> testSites, List<TestPage> testPages, List<TestTask> testCases, List<int> testCasesChoose)
        {
            _testSources = testSources;
            _testSites = testSites;
            _testPages = testPages;
            _testCases = testCases;
            _testCasesChoose = testCasesChoose;
            UCTestCaseSelector1.Init(testSources, testSites, testPages, testCases, testCasesChoose);
            return this;
        }

        public List<TestTask> GetSelecteCase()
        {
            return UCTestCaseSelector1.GetSelecteCase();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var tasks = GetSelecteCase();
            if (!tasks.Any())
            {
                return;
            }
            TestPanel testPanel = null;
            if (new ConfirmDlg("询问", "执行测试吗？").ShowDialog() == DialogResult.OK)
            {
                UCTestCaseSelector1.Reset();
                testPanel = (TestPanel)Util.TryAddToMainTab(this, $"执行测试", () =>
                {
                    var panel = new TestPanel("执行测试");
                    panel.Load();

                    return panel;
                }, typeof(TestPanel));

                if (testPanel.IsRunning())
                {
                    Util.SendMsg(this, "正在执行测试，请稍后再试");
                    return;
                }

                if (!testPanel.Reset())
                {
                    Util.SendMsg(this, "任务未开始，有测试在执行");
                    return;
                }
                testPanel.OnTaskStart += t =>
                {
                    var rt = t as RunTestTask;
                    if (rt != null && rt.TestLogin != null && (currentTestLogin == null || currentTestLogin.Id != rt.TestLogin.Id))
                    {
                        currentTestLogin = rt.TestLogin;
                        testPanel.ClearCookie(rt.TestLogin.Url);
                    }
                    OnTaskStart?.Invoke(t);
                };



                foreach (var tk in tasks)
                {
                    tk.ResultNotify += r =>
                    {
                        UCTestCaseSelector1.SetTestResult(r);
                    };
                }

                this.BtnOk.Enabled = false;
                BtnCancel.Enabled = true;

                BtnCancel.Click += BtnCancel_Click;

                LJC.FrameWorkV3.Comm.TaskHelper.SetInterval(1000, () =>
                {
                    var runTaskList = tasks.Select(task => new RunTestTask(task.GetTaskName(), false, task.TestSite, task.TestLogin, task.TestPage, task.TestCase, task.TestEnv, task.TestEnvParams, task.GlobalTestScripts, task.SiteTestScripts, task.ResultNotify));
                    BeginInvoke(new Action(() => testPanel.RunTest(runTaskList)));
                    LJC.FrameWorkV3.Comm.TaskHelper.SetInterval(1000, () =>
                    {
                        if (testPanel.IsDisposed || !testPanel.IsRunning())
                        {
                            BeginInvoke(new Action(() => { BtnOk.Enabled = true; BtnCancel.Enabled = false; BtnCancel.Click -= BtnCancel_Click; }));
                            return true;
                        }
                        return false;
                    }, runintime: false);
                    return true;
                }, runintime: false);
            }

            void BtnCancel_Click(object ss, EventArgs ee)
            {
                if (new ConfirmDlg("停止任务提示", "要停止任务吗？", timeOutOk: false).ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                if (!testPanel.IsDisposed)
                {
                    testPanel.CancelTasks();
                }
            }
        }

        public object[] GetRecoverData()
        {
            _testCasesChoose = GetSelecteCase().Select(p => p.TestCase.Id).ToList();
            return new object[] { _testSources, _testSites, _testPages, _testCases, _testCasesChoose, Text };
        }

        public IRecoverAble Recover(object[] recoverData)
        {
            Text = (string)recoverData[5];
            _testSources = (List<TestSource>)recoverData[0];
            _testSites = (List<TestSite>)recoverData[1];
            _testPages = (List<TestPage>)recoverData[2];
            _testCases = (List<TestTask>)recoverData[3];
            _testCasesChoose = (List<int>)recoverData[4];
            UCTestCaseSelector1.Init(_testSources, _testSites, _testPages, _testCases, _testCasesChoose);
            return this;
        }

        private void BtnRefrash_Click(object sender, EventArgs e)
        {
            try
            {
                var sources = BigEntityTableEngine.LocalEngine.FindBatch<TestSource>(nameof(TestSource), _testSources.Select(p => (object)p.Id));
                var sites = BigEntityTableEngine.LocalEngine.FindBatch<TestSite>(nameof(TestSite), _testSites.Select(p => (object)p.Id));
                var pages = BigEntityTableEngine.LocalEngine.FindBatch<TestPage>(nameof(TestPage), _testPages.Select(p => (object)p.Id));
                var cases = BigEntityTableEngine.LocalEngine.FindBatch<TestCase>(nameof(TestCase), _testCases.Select(p => (object)p.TestCase.Id));
                var testLogins = BigEntityTableEngine.LocalEngine.FindBatch<TestLogin>(nameof(TestLogin), _testCases.Select(p => (object)p.TestLogin.Id));

                var scriptsDic = new Dictionary<object, List<TestScript>>();
                var loginDic = new Dictionary<object, TestLogin>();
                Dictionary<int, (TestEnv env, List<TestEnvParam> envParams, bool hasEvn)> ep = new Dictionary<int, (TestEnv env, List<TestEnvParam> envParams, bool hasEvn)>();
                foreach (var item in _testCases.Where(p => p.TestEnv != null).Select(p => p.TestEnv.Id).Distinct())
                {
                    var env = BigEntityTableEngine.LocalEngine.Find<TestEnv>(nameof(TestEnv), item);
                    var envParams = BigEntityTableEngine.LocalEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_EnvId", new object[] { env.SiteId, env.Id });
                    ep.Add(env.Id, (env, envParams.ToList(), true));
                }

                foreach (var c in _testCases)
                {
                    c.TestSource = sources.FirstOrDefault(p => p.Id == c.TestSource.Id);
                    c.TestSite = sites.FirstOrDefault(p => p.Id == c.TestSite.Id);
                    c.TestPage = pages.FirstOrDefault(p => p.Id == c.TestPage.Id);
                    c.TestCase = BigEntityTableEngine.LocalEngine.Find<TestCase>(nameof(TestCase), c.TestCase.Id);

                    c.TestLogin = testLogins.FirstOrDefault(p => p.Id == c.TestLogin.Id);

                    var key = "globalScripts_" + c.TestSource.Id;
                    List<TestScript> globalScripts = null;
                    if (!scriptsDic.TryGetValue(key, out globalScripts))
                    {
                        globalScripts = BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == c.TestSource.Id && s.SiteId == 0).ToList();
                        scriptsDic.Add(key, globalScripts);
                    }

                    key = "siteScripts_" + c.TestSource.Id + "_" + c.TestSite.Id;
                    List<TestScript> siteScripts = null;
                    if (!scriptsDic.TryGetValue(key, out siteScripts))
                    {
                        siteScripts = BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == c.TestSource.Id && s.SiteId == c.TestSite.Id).ToList();
                        scriptsDic.Add(key, siteScripts);
                    }

                    c.GlobalTestScripts = globalScripts;
                    c.SiteTestScripts = siteScripts;

                    c.TestEnv = c.TestEnv == null ? null : ep.FirstOrDefault(p => p.Key == c.TestEnv.Id).Value.env;
                    c.TestEnvParams = c.TestEnv == null ? null : ep.FirstOrDefault(p => p.Key == c.TestEnv.Id).Value.envParams;
                }
                _testCasesChoose = GetSelecteCase().Select(p => p.TestCase.Id).ToList();
                UCTestCaseSelector1.Init(_testSources, _testSites, _testPages, _testCases, _testCasesChoose);

                Util.SendMsg(this, "刷新成功");

            }
            catch(Exception ex)
            {
                Util.SendMsg(this, ex.Message);
            }

        }

    }
}
