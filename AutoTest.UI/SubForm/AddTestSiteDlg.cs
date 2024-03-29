﻿using AutoTest.Domain.Entity;
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
    public partial class AddTestSiteDlg : SubBaseDlg
    {
        private int _sourceId;
        private int _siteId;
        public AddTestSiteDlg()
        {
            InitializeComponent();
        }

        public TestSite TestSite
        {
            get;
            private set;
        }

        public AddTestSiteDlg(int sourceId,int siteId)
        {
            InitializeComponent();

            this._sourceId = sourceId;
            this._siteId = siteId;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_siteId > 0)
            {
                var testSite = BigEntityTableRemotingEngine.Find<TestSite>(nameof(TestSite), _siteId);
                if (testSite == null)
                {
                    MessageBox.Show("site不存在");
                    this.Close();
                    return;
                }

                this.TBName.Text = testSite.Name;
                this.TBCode.Text = testSite.CheckLoginCode;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (_siteId == 0)
            {
                var site = new TestSite
                {
                    Name = TBName.Text,
                    CheckLoginCode = TBCode.Text,
                    SourceId = _sourceId
                };
                BigEntityTableRemotingEngine.Insert(nameof(TestSite), site);

                TestSite = site;
            }
            else
            {
                var site = new TestSite
                {
                    Id = _siteId,
                    Name = TBName.Text,
                    CheckLoginCode = TBCode.Text,
                    SourceId = _sourceId
                };
                BigEntityTableRemotingEngine.Update(nameof(TestSite), site);
                TestSite = site;

                EventBus.NotifyTestThingChangeAction?.Invoke(site);
            }

            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
