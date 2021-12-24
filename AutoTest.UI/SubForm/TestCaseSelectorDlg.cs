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
    public partial class TestCaseSelectorDlg : Form
    {
        public TestCaseSelectorDlg()
        {
            InitializeComponent();
        }

        public TestCaseSelectorDlg Init(List<TestSource> testSources, List<TestSite> testSites, List<TestPage> testPages, List<TestCase> testCases)
        {
            UCTestCaseSelector.Init(testSources, testSites, testPages, testCases);
            return this;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
