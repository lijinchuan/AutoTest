using AutoTest.Domain.Entity;
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
    public partial class TestCaseSelectorDlg : SubBaseDlg
    {
        public TestCaseSelectorDlg()
        {
            InitializeComponent();
        }

        public TestCaseSelectorDlg Init(List<TestSource> testSources, List<TestSite> testSites, List<TestPage> testPages, List<TestTask> testCases)
        {
            UCTestCaseSelector.Init(testSources, testSites, testPages, testCases,testCases.Select(p=>p.TestCase.Id).ToList(),null);
            return this;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var list = UCTestCaseSelector.GetSelecteCase();
            this.DialogResult = DialogResult.OK;
        }
    }
}
