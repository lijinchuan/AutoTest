using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LJC.FrameWorkV3.Data.EntityDataBase;
using AutoTest.Domain.Entity;
using AutoTest.Biz;
using System.Diagnostics;

namespace AutoTest.UI.UC
{
    public partial class UCTestTaskBagView : TabPage//UserControl//
    {
        private int _siteId = 0;
        private TestTaskBag _testTaskBag = null;
        private UCTestCaseSelector _ucTestCaseSelector = null;
        private UCTestCaseOrder ucTestCaseOrder = null;
        private Action<TestTaskBag> _onUpdateTestBag;

        public UCTestTaskBagView()
        {
            InitializeComponent();
        }

        public UCTestTaskBagView(int siteId,int testBagId, Action<TestTaskBag> onCallBack)
        {
            InitializeComponent();
            CBEvn.DropDownStyle = CBUser.DropDownStyle = ComboBoxStyle.DropDownList;

            _onUpdateTestBag = onCallBack;

            _siteId = siteId;

            var _testSite = BigEntityTableRemotingEngine.Find<TestSite>(nameof(TestSite), _siteId);
            var _testSource = BigEntityTableRemotingEngine.Find<TestSource>(nameof(TestSource), _testSite.SourceId);
            var _testPageList = new List<TestPage>();

            var testPageList = BigEntityTableRemotingEngine.Find<TestPage>(nameof(TestPage), nameof(TestPage.SiteId), new object[] { _testSite.Id }).ToList();
            if (testPageList.Any())
            {
                _testPageList.AddRange(testPageList);
            }

            var _testEnvList = BigEntityTableRemotingEngine.Find<TestEnv>(nameof(TestEnv), nameof(TestEnv.SiteId), new object[] { _testSite.Id }).ToList();
            if (_testEnvList.Any())
            {
                CBEvn.DataSource = _testEnvList;
                CBEvn.DisplayMember = nameof(TestEnv.EnvName);
                CBEvn.ValueMember = nameof(TestEnv.Id);
            }

            var _testLoginList = BigEntityTableRemotingEngine.Find<TestLogin>(nameof(TestLogin), nameof(TestLogin.SiteId), new object[] { _testSite.Id }).ToList();
            if (_testLoginList.Any())
            {
                CBUser.DataSource = _testLoginList;
                CBUser.DisplayMember = nameof(TestLogin.AccountInfo);
                CBUser.ValueMember = nameof(TestLogin.Id);
            }

            var _testCaseList = new List<TestCase>();
            foreach (var page in _testPageList)
            {
                var testCaseList = BigEntityTableRemotingEngine.Find<TestCase>(nameof(TestCase), nameof(TestCase.PageId), new object[] { page.Id }).ToList();
                if (testCaseList.Any())
                {
                    _testCaseList.AddRange(testCaseList);
                }
            }

            var _testTaskList = new List<TestTask>();
            foreach (var testCase in _testCaseList)
            {
                var testPage = _testPageList.Find(p => p.Id == testCase.PageId);
                _testTaskList.Add(new TestTask
                {
                    GlobalTestScripts=new List<TestScript>(),
                    SiteTestScripts=new List<TestScript>(),
                    TestCase=testCase,
                    TestSource=_testSource,
                    TestPage= testPage,
                    TestSite=_testSite
                });
            }

            _ucTestCaseSelector = new UCTestCaseSelector();
            _ucTestCaseSelector.Dock = DockStyle.Fill;
            panel1.Controls.Add(_ucTestCaseSelector);

            if (testBagId > 0)
            {
                _testTaskBag = BigEntityTableRemotingEngine.Find<TestTaskBag>(nameof(TestTaskBag), testBagId);
            }
            else
            {
                _testTaskBag = new TestTaskBag
                {
                    CaseId=_testCaseList.Select(p=>p.Id).ToList(),
                    SiteId=_testSource.Id
                };
            }

            _ucTestCaseSelector.Init(new List<TestSource> { _testSource }, new List<TestSite> { _testSite }, _testPageList, _testTaskList, _testTaskBag.CaseId, new Dictionary<int, TestResult>());

        }

        public void Init()
        {
            if (_testTaskBag?.Id > 0)
            {
                TBName.Text = _testTaskBag.BagName;
                TBCorn.Text = _testTaskBag.Corn;
                CBEvn.SelectedValue = _testTaskBag.TestEnvId;
                CBUser.SelectedValue = _testTaskBag.TestLoginId;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_ucTestCaseSelector == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(TBName.Text))
            {
                Util.SendMsg(this, "测试包名称不能为空");
                return;
            }

            if (!string.IsNullOrWhiteSpace(TBCorn.Text) && CronHelper.GetNextDateTime(TBCorn.Text, DateTime.Now) == null)
            {
                Util.SendMsg(this, "corn表达式错误");
                return;
            }

            var selCaseList = _ucTestCaseSelector.GetSelecteCase();

            _testTaskBag.BagName = TBName.Text;
            _testTaskBag.CaseId = selCaseList.Select(p => p.TestCase.Id).ToList();
            if (CBEvn.SelectedValue != null)
            {
                _testTaskBag.TestEnvId = (int)CBEvn.SelectedValue;
            }
            if (CBUser.SelectedValue != null)
            {
                _testTaskBag.TestLoginId = (int)CBUser.SelectedValue;
            }
            _testTaskBag.Corn = TBCorn.Text.Trim();

            if (_testTaskBag.Id > 0)
            {
                BigEntityTableRemotingEngine.Update(nameof(TestTaskBag), _testTaskBag);
                Util.SendMsg(this, "测试包更新成功");
            }
            else
            {
                BigEntityTableRemotingEngine.Insert(nameof(TestTaskBag), _testTaskBag);
                Util.SendMsg(this, "测试包添加成功");
            }

            _onUpdateTestBag?.Invoke(_testTaskBag);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://cron.qqe2.com/");
        }

        private void BtnOrder_Click(object sender, EventArgs e)
        {
            if (BtnOrder.Text == "排序")
            {
                var selCaseList = _ucTestCaseSelector.GetSelecteCase();
                if (selCaseList.Count < 2)
                {
                    MessageBox.Show("没有CASE可以排序");
                    return;
                }
                _ucTestCaseSelector.Visible = false;
                if (ucTestCaseOrder == null)
                {
                    ucTestCaseOrder = new UCTestCaseOrder();
                    ucTestCaseOrder.Dock = DockStyle.Fill;
                    panel1.Controls.Add(ucTestCaseOrder);
                }
                ucTestCaseOrder.Visible = true;
                ucTestCaseOrder.SetTestTasks(selCaseList);
                BtnOrder.Text = "选择";

            }
            else
            {
                ucTestCaseOrder.Visible = false;
                var orders = ucTestCaseOrder.GetOrders();
                _ucTestCaseSelector.Visible = true;
                BtnOrder.Text = "排序";
            }
        }
    }
}
