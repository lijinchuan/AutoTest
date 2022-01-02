using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Comm;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.SubForm
{
    public partial class AddTestCaseDlg : Form
    {
        private int _pageId;
        private int _caseId;
        private TestCase _copyCase;
        private List<TestScript> _testScripts;
        public AddTestCaseDlg()
        {
            InitializeComponent();
        }

        public AddTestCaseDlg(int pageId, int caseId, List<TestScript> testScripts,TestCase copyCase=null)
        {
            InitializeComponent();
            _pageId = pageId;
            _caseId = caseId;
            _copyCase = copyCase;
            _testScripts = testScripts;

            if (testScripts != null)
            {
                var keyWords = testScripts.Select(p => new AutoTest.Util.ScriptKeyWord
                {
                    Desc = p.Desc,
                    KeyWord = p.ScriptName,
                    HighColor = Color.Red
                }).ToList();
                TBCode.Init(keyWords);
                TBValidCode.Init(keyWords);
            }
        }

        public TestCase TestCase
        {
            get;
            private set;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_caseId > 0)
            {
                var testCase = BigEntityTableRemotingEngine.Find<TestCase>(nameof(TestCase), _caseId);
                if (testCase == null)
                {
                    MessageBox.Show("case不存在");
                    this.Close();
                    return;
                }
                TBName.Text = testCase.CaseName;
                TBUrl.Text = testCase.Url ?? string.Empty;
                NUDOrder.Value = testCase.Order;
                TBCode.Text = testCase.TestCode;
                TBValidCode.Text = testCase.ValidCode;
            }
            else
            {
                if (_copyCase != null)
                {
                    TBName.Text = _copyCase.CaseName;
                    TBCode.Text = _copyCase.TestCode;
                    TBValidCode.Text = _copyCase.ValidCode;
                    if (!string.IsNullOrWhiteSpace(_copyCase.Url))
                    {
                        TBUrl.Text = _copyCase.Url;
                    }
                }

                var cnt = BigEntityTableRemotingEngine.Count(nameof(TestCase), nameof(TestCase.PageId), new object[] { _pageId });
                NUDOrder.Value = cnt + 1;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBName.Text))
            {
                MessageBox.Show("名称不能为空");
                return;
            }

            TestCase = new TestCase
            {
                Id = _caseId,
                PageId = _pageId,
                CaseName = TBName.Text,
                Order = (int)NUDOrder.Value,
                TestCode = TBCode.Text,
                ValidCode = TBValidCode.Text,
                Url = TBUrl.Text
            };
            if (_caseId == 0)
            {
                var list = BigEntityTableRemotingEngine.Find<TestCase>(nameof(TestCase), nameof(TestCase.PageId), new object[] { _pageId });
                if (list.Any(p => p.CaseName == TestCase.CaseName))
                {
                    new AlertDlg("添加测试用例", $"用例名[{TestCase.CaseName}]不能重复", null).ShowDialog();
                    return;
                }
                BigEntityTableRemotingEngine.Insert(nameof(TestCase), TestCase);
            }
            else
            {
                BigEntityTableRemotingEngine.Update(nameof(TestCase), TestCase);
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
