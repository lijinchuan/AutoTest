﻿using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace AutoTest.UI.SubForm
{
    public partial class AddTestEnvParamDlg : Form
    {
        private int _siteId = 0, _envparamid = 0;

        private List<TestEnv> apiEnvs = null;
        private List<ParamInfo> apiEnvParams = new List<ParamInfo>();

        public AddTestEnvParamDlg()
        {
            InitializeComponent();
        }

        public AddTestEnvParamDlg(int siteId, int envparamid)
        {
            InitializeComponent();

            _siteId = siteId;
            _envparamid = envparamid;

            apiEnvs = BigEntityTableRemotingEngine.Find<TestEnv>(nameof(TestEnv), nameof(TestEnv.SiteId), new object[] { _siteId }).ToList();
            if (_envparamid == 0)
            {
                apiEnvParams.AddRange(apiEnvs.Select(p => new ParamInfo
                {
                    Name = p.EnvName
                }));
            }
            else
            {
                var apienvparam = BigEntityTableRemotingEngine.Find<TestEnvParam>(nameof(TestEnvParam), _envparamid);
                var envparamlist = BigEntityTableRemotingEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_Name", new object[] { _siteId, apienvparam.Name }).ToList();
                apiEnvParams.AddRange(apiEnvs.Select(p =>
                {
                    return new ParamInfo
                    {
                        Name = p.EnvName,
                        Value = envparamlist.Any(q => q.EnvId == p.Id) ? envparamlist.Find(q => q.EnvId == p.Id).Val : string.Empty,
                        Desc = envparamlist.Any(q => q.EnvId == p.Id) ? envparamlist.Find(q => q.EnvId == p.Id).Id.ToString() : string.Empty
                    };
                }));

                TBName.Text = apienvparam.Name;

            }
            this.DGV.DataSource = apiEnvParams;

            DGV.RowHeadersVisible = false;
            DGV.ColumnHeadersVisible = false;
            DGV.DataBindingComplete += DGV_DataBindingComplete;
            DGV.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void DGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewColumn col in DGV.Columns)
            {
                col.Visible = col.Name == "Name" || col.Name == "Value";
            }

            if (DGV.Rows.Count > 0)
            {
                DGV.Columns["Name"].Width = DGV.Width / 4;
                DGV.Columns["Name"].ReadOnly = true;
                DGV.Height = DGV.Rows.Count * (DGV.Rows[0].Height + 1);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            var envparamname = TBName.Text.Trim();

            var list = apiEnvParams.Select(p => new TestEnvParam
            {
                SiteId = _siteId,
                EnvId = apiEnvs.Find(q => q.EnvName == p.Name).Id,
                Name = envparamname,
                Val = p.Value,
                Id = string.IsNullOrEmpty(p.Desc) ? 0 : int.Parse(p.Desc)
            }).ToList();

            if (list.Any(p => p.Id == 0))
            {
                BigEntityTableRemotingEngine.InsertBatch(nameof(TestEnvParam), list.Where(p => p.Id == 0));
            }
            else
            {
                foreach (var item in list.Where(p => p.Id > 0))
                {
                    BigEntityTableRemotingEngine.Update(nameof(TestEnvParam), item);
                }
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
