﻿using AutoTest.Domain;
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
            return new object[] {_testSources, _testSites, _testPages, _testCases, _testCasesChoose,Text};
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
    }
}
