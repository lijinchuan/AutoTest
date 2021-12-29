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
        private TestLogin _testLogin;
        private int _siteLoginId = 0;
        private int _siteId;

        public AddTestLoginDlg()
        {
            InitializeComponent();
        }

        public AddTestLoginDlg(int siteId,TestLogin testLogin)
        {
            InitializeComponent();

            _siteId = siteId;
            _testLogin = testLogin;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_testLogin != null)
            {
                _siteLoginId = _testLogin.Id;
                TBLoginCode.Text = _testLogin.LoginCode;
                TBValidCode.Text = _testLogin.ValidCode;
                CBManual.Checked = _testLogin.IsMannual;
                TBUrl.Text = _testLogin.Url;
                TBAccountName.Text = _testLogin.AccountInfo;
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
            if (string.IsNullOrWhiteSpace(TBAccountName.Text))
            {
                MessageBox.Show("帐号名称不能为空");
                return;
            }

            if (string.IsNullOrEmpty(TBUrl.Text))
            {
                MessageBox.Show("地址不能为空");
                return;
            }

            var testLoginList = BigEntityTableEngine.LocalEngine.Find<TestLogin>(nameof(TestLogin), nameof(TestLogin.SiteId), new object[] { _siteId }).ToList();
            if (testLoginList.Any(t => t.Id != _siteLoginId && t.AccountInfo == TBAccountName.Text.Trim()))
            {
                MessageBox.Show("帐号名称不能重复");
                return;
            }

            if (_siteLoginId == 0)
            {
                var login = new TestLogin
                {
                    AccountInfo = TBAccountName.Text.Trim(),
                    IsMannual = CBManual.Checked,
                    LoginCode = TBLoginCode.Text,
                    ValidCode = TBValidCode.Text,
                    SiteId = _siteId,
                    Url = TBUrl.Text,
                    Used = !testLoginList.Any(p => p.Used)
                };
                BigEntityTableEngine.LocalEngine.Insert(nameof(TestLogin), login);

                EventBus.NotifyTestThingAddAction?.Invoke(login);
            }
            else
            {
                var login = new TestLogin
                {
                    Id = _siteLoginId,
                    AccountInfo = TBAccountName.Text.Trim(),
                    IsMannual = CBManual.Checked,
                    LoginCode = TBLoginCode.Text,
                    ValidCode = TBValidCode.Text,
                    SiteId = _siteId,
                    Url = TBUrl.Text
                };
                BigEntityTableEngine.LocalEngine.Update(nameof(TestLogin), login);

                EventBus.NotifyTestThingChangeAction?.Invoke(login);
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
