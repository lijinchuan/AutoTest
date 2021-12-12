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
    public partial class RunTestCaseConfirmDlg : SubBaseDlg
    {
        private List<TestTask> _testTaskList = null;

        public RunTestCaseConfirmDlg()
        {
            InitializeComponent();
        }

        public RunTestCaseConfirmDlg(List<TestTask> taskList)
        {
            InitializeComponent();

            _testTaskList = taskList;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.GVTaskList.DataBindingComplete += GVTaskList_DataBindingComplete;
            this.GVTaskList.DataSource = _testTaskList;
        }

        private void GVTaskList_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
