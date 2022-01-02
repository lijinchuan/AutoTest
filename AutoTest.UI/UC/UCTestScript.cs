using System.Windows.Forms;
using AutoTest.Domain.Entity;
using AutoTest.Domain;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using LJC.FrameWorkV3.LogManager;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using AutoTest.Util;
using System.Drawing;

namespace AutoTest.UI.UC
{
    public partial class UCTestScript : TabPage, ISaveAble
    {
        private TestScript _testScript;
        private List<TestScript> _testScripts;

        public UCTestScript()
        {
            InitializeComponent();
        }

        public UCTestScript(TestScript testScript,List<TestScript> testScripts)
        {
            InitializeComponent();

            this._testScript = testScript;
            _testScripts = testScripts;

            if (_testScripts != null && _testScripts.Count > 0)
            {
                var keyWords = _testScripts.Select(p => new ScriptKeyWord
                {
                     Desc=p.Desc,
                     HighColor=Color.Red,
                     KeyWord=p.ScriptName
                }).ToList();
                this.TBBody.Init(keyWords);
            }

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
                BigEntityTableRemotingEngine.Insert(nameof(TestScript), _testScript);
                Util.SendMsg(this, "添加脚本成功");
            }
            else
            {
                BigEntityTableRemotingEngine.Update(nameof(TestScript), _testScript);
                EventBus.NotifyTestThingChangeAction?.Invoke(_testScript);
                Util.SendMsg(this, "更新脚本成功");
            }

            LogHelper.Instance.Info($"保存testScript：{JsonConvert.SerializeObject(_testScript)}");
        }
    }
}
