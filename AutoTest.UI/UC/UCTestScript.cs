using System.Windows.Forms;
using AutoTest.Domain.Entity;
using AutoTest.Domain;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using LJC.FrameWorkV3.LogManager;
using Newtonsoft.Json;

namespace AutoTest.UI.UC
{
    public partial class UCTestScript : TabPage, ISaveAble
    {
        private TestScript _testScript;
        private Action _callBack;

        public UCTestScript()
        {
            InitializeComponent();
        }

        public UCTestScript(TestScript testScript,Action callBack)
        {
            InitializeComponent();

            this._testScript = testScript;
            this._callBack = callBack;

            this.TBName.Text = testScript.ScriptName;
            this.NUDNumber.Value = testScript.Order;
            this.TBBody.Text = testScript.Body;
            this.TBDesc.Text = testScript.Desc;
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(TBName.Text))
            {
                Util.SendMsg(this, "脚本名称不能为空");
                return;
            }

            if (string.IsNullOrWhiteSpace(TBBody.Text))
            {
                Util.SendMsg(this, "脚本内容不能为空");
                return;
            }

            _testScript.ScriptName = TBName.Text.Trim();
            _testScript.Order = (int)NUDNumber.Value;
            _testScript.Body = TBBody.Text;
            _testScript.Desc = TBDesc.Text.Trim();

            if (_testScript.Id == 0)
            {
                BigEntityTableEngine.LocalEngine.Insert(nameof(TestScript), _testScript);
                Util.SendMsg(this, "添加脚本成功");
            }
            else
            {
                BigEntityTableEngine.LocalEngine.Update(nameof(TestScript), _testScript);
                Util.SendMsg(this, "更新脚本成功");
            }

            LogHelper.Instance.Info($"保存testScript：{JsonConvert.SerializeObject(_testScript)}");

            _callBack?.Invoke();
        }
    }
}
