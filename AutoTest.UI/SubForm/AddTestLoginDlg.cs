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
    public partial class AddTestLoginDlg : SubBaseDlg
    {
        private int _siteLoginId;
        private int _siteId;

        public AddTestLoginDlg()
        {
            InitializeComponent();
        }

        public AddTestLoginDlg(int siteId)
        {
            InitializeComponent();

            _siteId = siteId;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var testLogin = BigEntityTableEngine.LocalEngine.Find<TestLogin>(nameof(TestLogin), nameof(TestLogin.SiteId), new object[] { _siteId }).FirstOrDefault();
            if (testLogin != null)
            {
                _siteLoginId = testLogin.Id;
                TBLoginCode.Text = testLogin.LoginCode;
                TBValidCode.Text = testLogin.ValidCode;
                CBManual.Checked = testLogin.IsMannual;
                TBUrl.Text = testLogin.Url;
            }
        }

        private void CBManual_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TBUrl.Text))
            {
                MessageBox.Show("地址不能为空");
                return;
            }

            if (_siteLoginId == 0)
            {
                BigEntityTableEngine.LocalEngine.Insert(nameof(TestLogin), new TestLogin
                {
                    IsMannual=CBManual.Checked,
                    LoginCode=TBLoginCode.Text,
                    ValidCode=TBValidCode.Text,
                    SiteId=_siteId,
                    Url=TBUrl.Text
                });
            }
            else
            {
                BigEntityTableEngine.LocalEngine.Update(nameof(TestLogin), new TestLogin
                {
                    Id=_siteLoginId,
                    IsMannual = CBManual.Checked,
                    LoginCode = TBLoginCode.Text,
                    ValidCode = TBValidCode.Text,
                    SiteId = _siteId,
                    Url = TBUrl.Text
                });
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
