using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;

namespace AutoTest.UI.UC
{
    public partial class UCTestCaseSetting : UserControl
    {
        private int _testCaseId = 0;
        private Domain.Entity.TestCaseSetting _testCaseSetting = null;
        private TestCaseSettingObj _testCaseSettingObj = null;
        public UCTestCaseSetting()
        {
            InitializeComponent();
            this.AutoScroll = true;
            BtnSave.Visible = false;
            Init();
        }

        public UCTestCaseSetting(int testCaseId)
        {
            InitializeComponent();
            this.AutoScroll = true;
            _testCaseId = testCaseId;
            Init();
        }

        public bool NoPrxoy()
        {
            return _testCaseSettingObj?.NoPrxoy == true;
        }

        public bool SaveResp()
        {
            return _testCaseSettingObj?.SaveResp==true;
        }

        public int TimeOut()
        {
            return _testCaseSettingObj?.TimeOut ?? 0;
        }

        public int PNumber()
        {
            return _testCaseSettingObj?.PSendNumber ?? 1;
        }

        public void ResetPNumber()
        {
            if (_testCaseSettingObj != null)
                _testCaseSettingObj.PSendNumber = 1;
            NPNumber.Value = 1;

            BtnSave_Click(null, null);
        }

        public bool CreateSSLTLSSecureChannel()
        {
            return _testCaseSettingObj?.Create_SSL_TLS_secure_channel ?? false;
        }

        private void Init()
        {
            if (_testCaseId > 0)
            {
                var setting = BigEntityTableEngine.LocalEngine.Find<TestCaseSetting>(nameof(TestCaseSetting), nameof(TestCaseSetting.TestCaseId), new object[] { _testCaseId }).FirstOrDefault();
                _testCaseSetting = setting ?? new Domain.Entity.TestCaseSetting() { TestCaseId = this._testCaseId };

                if (!string.IsNullOrWhiteSpace(_testCaseSetting.SettingJson))
                {
                    _testCaseSettingObj = Newtonsoft.Json.JsonConvert.DeserializeObject<TestCaseSettingObj>(_testCaseSetting.SettingJson);
                }
                else
                {
                    _testCaseSettingObj = new TestCaseSettingObj();
                }

                this.TBTimeOut.Text = _testCaseSettingObj.TimeOut.ToString();
                this.CBNoproxy.Checked = _testCaseSettingObj.NoPrxoy;
                this.CBSaveResp.Checked = _testCaseSettingObj.SaveResp;
                this.NPNumber.Value = _testCaseSettingObj.PSendNumber;
                this.CBCreakSSLChannel.Checked = _testCaseSettingObj.Create_SSL_TLS_secure_channel;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
        }


        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var newapiUrlSettingObj = new TestCaseSettingObj
                {
                    NoPrxoy = CBNoproxy.Checked,
                    SaveResp = CBSaveResp.Checked,
                    TimeOut = int.Parse(TBTimeOut.Text),
                    PSendNumber = (int)NPNumber.Value,
                    Create_SSL_TLS_secure_channel = CBCreakSSLChannel.Checked
                };
                _testCaseSetting.SettingJson = Newtonsoft.Json.JsonConvert.SerializeObject(newapiUrlSettingObj);
                if (_testCaseSetting.Id == 0)
                {
                    BigEntityTableEngine.LocalEngine.Insert(nameof(Domain.Entity.TestCaseSetting), _testCaseSetting);
                }
                else
                {
                    BigEntityTableEngine.LocalEngine.Update(nameof(Domain.Entity.TestCaseSetting), _testCaseSetting);
                }

                _testCaseSettingObj = newapiUrlSettingObj;

            }
            catch (Exception ex)
            {
                _testCaseSetting.SettingJson = Newtonsoft.Json.JsonConvert.SerializeObject(_testCaseSettingObj);
                Util.SendMsg(this, ex.Message);
            }

        }
    }
}
