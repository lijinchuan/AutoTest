using AutoTest.Domain.Entity;
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
    public partial class AddEnvDlg : Form
    {
        private int _siteId;

        public AddEnvDlg()
        {
            InitializeComponent();
        }

        public AddEnvDlg(int siteId)
        {
            InitializeComponent();

            _siteId = siteId;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (_siteId == 0)
            {
                return;
            }

            var envname = TBName.Text.Trim();
            if (string.IsNullOrWhiteSpace(envname))
            {
                MessageBox.Show("名称不能为空");
                return;
            }

            var isexists = BigEntityTableRemotingEngine.Find<TestEnv>(nameof(TestEnv), nameof(TestEnv.SiteId), new object[] { _siteId }).Any(p => p.EnvName.Equals(envname, StringComparison.OrdinalIgnoreCase));
            if (isexists)
            {
                MessageBox.Show("环境名称不能重复");
                return;
            }

            BigEntityTableRemotingEngine.Insert(nameof(TestEnv), new TestEnv
            {
                EnvDesc = TBDesc.Text.Trim(),
                EnvName = envname,
                SiteId = _siteId
            });

            this.DialogResult = DialogResult.OK;
        }
    }
}
