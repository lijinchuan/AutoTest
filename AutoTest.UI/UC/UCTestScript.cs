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
using AutoTest.Domain;
using LJC.FrameWorkV3.Data.EntityDataBase;

namespace AutoTest.UI.UC
{
    public partial class UCTestScript : TabPage, ISaveAble
    {
        private TestScript _testScript;

        public UCTestScript()
        {
            InitializeComponent();
        }

        public UCTestScript(TestScript testScript)
        {
            InitializeComponent();

            this._testScript = testScript;

            this.TBName.Text = testScript.ScriptName;
            this.NUDNumber.Value = testScript.Order;
            this.TBBody.Text = testScript.Body;
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
        }
    }
}
