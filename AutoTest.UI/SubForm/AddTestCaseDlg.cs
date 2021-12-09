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
        public AddTestCaseDlg()
        {
            InitializeComponent();
        }

        public AddTestCaseDlg(int pageId,int caseId)
        {
            InitializeComponent();
            _pageId = pageId;
            _caseId = caseId;
            
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
                var testCase = BigEntityTableEngine.LocalEngine.Find<TestCase>(nameof(TestCase), _caseId);
                if (testCase == null)
                {
                    MessageBox.Show("case不存在");
                    this.Close();
                    return;
                }
                TBName.Text = testCase.CaseName;
                NUDOrder.Value = testCase.Order;
                TBCode.Text = testCase.TestCode;
                TBValidCode.Text = testCase.ValidCode;
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
                ValidCode = TBValidCode.Text
            };
            if (_caseId == 0)
            {

                BigEntityTableEngine.LocalEngine.Insert(nameof(TestCase), TestCase);
            }
            else
            {
                BigEntityTableEngine.LocalEngine.Update<TestCase>(nameof(TestCase), TestCase);
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
