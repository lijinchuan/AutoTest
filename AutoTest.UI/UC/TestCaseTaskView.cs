using AutoTest.Biz;
using AutoTest.Domain;
using AutoTest.Domain.Entity;
using AutoTest.UI.SubForm;
using AutoTest.UI.WebTask;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.UC
{
    public partial class TestCaseTaskView : TabPage, IRecoverAble
    {
        private static TestLogin currentTestLogin = null;
        public event Action<IWebTask> OnTaskStart;
        private TestTaskBag _testTaskBag = null;
        private List<TestSource> _testSources = null;
        private List<TestSite> _testSites = null;
        private List<TestPage> _testPages = null;
        private List<TestTask> _testCases = null;
        private List<int> _testCasesChoose = null;
        private Dictionary<int, TestResult> _testResults = new Dictionary<int, TestResult>();

        public TestCaseTaskView()
        {
            InitializeComponent();
            BtnCancel.Enabled = false;
            UCTestCaseSelector1.ReTestCaseAction += ReTestCase;
        }

        public TestCaseTaskView Init(TestTaskBag testTaskBag,List<TestSource> testSources, List<TestSite> testSites, List<TestPage> testPages, List<TestTask> testCases, List<int> testCasesChoose)
        {
            _testTaskBag = testTaskBag;
            _testSources = testSources;
            _testSites = testSites;
            _testPages = testPages;
            _testCases = testCases;
            _testCasesChoose = testCasesChoose;
            UCTestCaseSelector1.Init(testSources, testSites, testPages, testCases, testCasesChoose, _testResults);
            
            return this;
        }

        public List<TestTask> GetSelecteCase()
        {
            return UCTestCaseSelector1.GetSelecteCase();
        }

        private void RunTest(List<TestTask> tasks, bool resetResult)
        {
            if (!tasks.Any())
            {
                return;
            }
            if (new ConfirmDlg("询问", "执行测试吗？").ShowDialog() == DialogResult.OK)
            {
                if (resetResult)
                {
                    UCTestCaseSelector1.Reset();
                }

                // 桌面测试：创建日志+结果面板，不需要浏览器
                var logTab = new LogViewTab();
                logTab.Dock = DockStyle.Fill;

                TabPage tabPage = new TabPage("执行测试");
                tabPage.Controls.Add(logTab);

                Util.TryAddToMainTab(this, $"执行测试", () => tabPage, null);

                if (_testTaskBag != null)
                {
                    tasks = TestTaskBagBiz.Order(tasks, this._testTaskBag);
                }

                foreach (var tk in tasks)
                {
                    tk.ResultNotify += r =>
                    {
                        _testResults[r.TestCaseId] = r;
                        UCTestCaseSelector1.SetTestResult(r);
                    };
                }

                this.BtnOk.Enabled = false;
                BtnRefrash.Enabled = false;
                BtnCancel.Enabled = true;

                BtnCancel.Click += BtnCancel_Click;

                LJC.FrameWorkV3.Comm.TaskHelper.SetInterval(1000, () =>
                {
                    foreach (var task in tasks)
                    {
                        var runner = new DesktopTestRunner(
                            task.TestCase, task.TestEnv, task.TestEnvParams, task.ResultNotify);
                        Task.Factory.StartNew(async () => await runner.RunAsync());
                    }

                    BeginInvoke(new Action(() => { BtnOk.Enabled = true; BtnCancel.Enabled = false; BtnRefrash.Enabled = true; BtnCancel.Click -= BtnCancel_Click; Util.SelectedTab(this, this); }));

                    return true;
                }, runintime: false);


            }

            void BtnCancel_Click(object ss, EventArgs ee)
            {
                if (new ConfirmDlg("停止任务提示", "要停止任务吗？", timeOutOk: false).ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var tasks = GetSelecteCase();
            if (tasks.Count() > _testResults.Count() && _testResults.Count() > 0)
            {
                if (new ConfirmDlg("未完成测试询问", "继续测试吗?是-继续，否-全部重新测试").ShowDialog() == DialogResult.OK)
                {
                    RunTest(tasks.Where(p => !_testResults.ContainsKey(p.TestCase.Id)).ToList(), false);
                    return;
                }
            }
            _testResults.Clear();
            RunTest(tasks, true);
        }

        public object[] GetRecoverData()
        {
            _testCasesChoose = GetSelecteCase().Select(p => p.TestCase.Id).ToList();
            return new object[] { _testSources, _testSites, _testPages, _testCases, _testCasesChoose, Text, _testResults , _testTaskBag};
        }

        public IRecoverAble Recover(object[] recoverData)
        {
            Text = (string)recoverData[5];
            _testSources = (List<TestSource>)recoverData[0];
            _testSites = (List<TestSite>)recoverData[1];
            _testPages = (List<TestPage>)recoverData[2];
            _testCases = (List<TestTask>)recoverData[3];
            _testCasesChoose = (List<int>)recoverData[4];
            _testResults = (Dictionary<int, TestResult>)recoverData[6];
            if (recoverData.Length > 7)
            {
                _testTaskBag = (TestTaskBag)recoverData[7];
            }
            UCTestCaseSelector1.Init(_testSources, _testSites, _testPages, _testCases, _testCasesChoose, _testResults);
            return this;
        }

        private void BtnRefrash_Click(object sender, EventArgs e)
        {
            try
            {
                var sources = AutoTest.Data.DataStoreSwitcher.Current.FindBatch<TestSource>(nameof(TestSource), _testSources.Select(p => (object)p.Id));
                var sites = AutoTest.Data.DataStoreSwitcher.Current.FindBatch<TestSite>(nameof(TestSite), _testSites.Select(p => (object)p.Id));
                var pages = AutoTest.Data.DataStoreSwitcher.Current.FindBatch<TestPage>(nameof(TestPage), _testPages.Select(p => (object)p.Id));
                var cases = AutoTest.Data.DataStoreSwitcher.Current.FindBatch<TestCase>(nameof(TestCase), _testCases.Select(p => (object)p.TestCase.Id));
                var testLogins = AutoTest.Data.DataStoreSwitcher.Current.FindBatch<TestLogin>(nameof(TestLogin), _testCases.Where(p => p.TestLogin != null).Select(p => (object)p.TestLogin.Id));

                var scriptsDic = new Dictionary<object, List<TestScript>>();
                var loginDic = new Dictionary<object, TestLogin>();
                Dictionary<int, (TestEnv env, List<TestEnvParam> envParams, bool hasEvn)> ep = new Dictionary<int, (TestEnv env, List<TestEnvParam> envParams, bool hasEvn)>();
                foreach (var item in _testCases.Where(p => p.TestEnv != null).Select(p => p.TestEnv.Id).Distinct())
                {
                    var env = AutoTest.Data.DataStoreSwitcher.Current.Find<TestEnv>(nameof(TestEnv), item);
                    var envParams = AutoTest.Data.DataStoreSwitcher.Current.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_EnvId", new object[] { env.SiteId, env.Id });
                    ep.Add(env.Id, (env, envParams.ToList(), true));
                }

                foreach (var c in _testCases)
                {
                    c.TestSource = sources.FirstOrDefault(p => p.Id == c.TestSource.Id);
                    c.TestSite = sites.FirstOrDefault(p => p.Id == c.TestSite.Id);
                    c.TestPage = pages.FirstOrDefault(p => p.Id == c.TestPage.Id);
                    c.TestCase = AutoTest.Data.DataStoreSwitcher.Current.Find<TestCase>(nameof(TestCase), c.TestCase.Id);

                    c.TestLogin = c.TestLogin == null ? null : testLogins.FirstOrDefault(p => p.Id == c.TestLogin.Id);

                    var key = "globalScripts_" + c.TestSource.Id;
                    List<TestScript> globalScripts = null;
                    if (!scriptsDic.TryGetValue(key, out globalScripts))
                    {
                        globalScripts = AutoTest.Data.DataStoreSwitcher.Current.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == c.TestSource.Id && s.SiteId == 0).ToList();
                        scriptsDic.Add(key, globalScripts);
                    }

                    key = "siteScripts_" + c.TestSource.Id + "_" + c.TestSite.Id;
                    List<TestScript> siteScripts = null;
                    if (!scriptsDic.TryGetValue(key, out siteScripts))
                    {
                        siteScripts = AutoTest.Data.DataStoreSwitcher.Current.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == c.TestSource.Id && s.SiteId == c.TestSite.Id).ToList();
                        scriptsDic.Add(key, siteScripts);
                    }

                    c.GlobalTestScripts = globalScripts;
                    c.SiteTestScripts = siteScripts;

                    c.TestEnv = c.TestEnv == null ? null : ep.FirstOrDefault(p => p.Key == c.TestEnv.Id).Value.env;
                    c.TestEnvParams = c.TestEnv == null ? null : ep.FirstOrDefault(p => p.Key == c.TestEnv.Id).Value.envParams;
                }
                _testCasesChoose = GetSelecteCase().Select(p => p.TestCase.Id).ToList();
                UCTestCaseSelector1.Init(_testSources, _testSites, _testPages, _testCases, _testCasesChoose, _testResults);

                Util.SendMsg(this, "刷新成功");

            }
            catch(Exception ex)
            {
                Util.SendMsg(this, ex.Message);
            }

        }

        private void ReTestCase(int caseid)
        {
            var tasks = _testCases.Where(p => p.TestCase.Id == caseid).ToList();
            this.BeginInvoke(new Action(() => RunTest(tasks, false)));
        }
    }
}

