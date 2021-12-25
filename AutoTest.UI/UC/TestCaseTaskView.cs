using AutoTest.Domain.Entity;
using AutoTest.UI.SubForm;
using AutoTest.UI.WebTask;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace AutoTest.UI.UC
{
    public partial class TestCaseTaskView : TabPage
    {
        private static TestLogin currentTestLogin = null;
        public event Action<IWebTask> OnTaskStart;

        public TestCaseTaskView()
        {
            InitializeComponent();
        }

        public TestCaseTaskView Init(List<TestSource> testSources, List<TestSite> testSites, List<TestPage> testPages, List<TestTask> testCases, List<int> testCasesChoose)
        {
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
            if (new ConfirmDlg("询问", "执行测试吗？").ShowDialog() == DialogResult.OK)
            {
                var testPanel = (TestPanel)Util.TryAddToMainTab(this, $"执行测试", () =>
                {
                    var panel = new TestPanel("执行测试");
                    panel.Load();

                    return panel;
                }, typeof(TestPanel));

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

                foreach(var tk in tasks)
                {
                    tk.ResultNotify += r =>
                    {
                        UCTestCaseSelector1.SetTestResult(r);
                    };
                }

                LJC.FrameWorkV3.Comm.TaskHelper.SetInterval(1000, () =>
                {
                    var runTaskList = tasks.Select(task => new RunTestTask(task.GetTaskName(), false, task.TestSite, task.TestLogin, task.TestPage, task.TestCase, task.TestEnv, task.TestEnvParams, task.GlobalTestScripts, task.SiteTestScripts, task.ResultNotify));
                    this.BeginInvoke(new Action(() => testPanel.RunTest(runTaskList)));
                    return true;
                }, runintime: false);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
