using AutoTest.Domain.Entity;
using AutoTest.UI.Resources;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.UC
{
    public partial class UCRequestInterceptConfigManager : UserControl
    {
        private TestCase _testCase = null;
        private RequestInterceptConfig _currentInterceptConfig = null;
        private string _file = null;

        public UCRequestInterceptConfigManager()
        {
            InitializeComponent();
        }

        public UCRequestInterceptConfigManager(TestCase testCase)
        {
            InitializeComponent();

            _testCase = testCase;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GBEdit.Visible = false;
            GVRequestIntercept.Dock = DockStyle.Fill;
            GVRequestIntercept.MouseClick += GVRequestIntercept_MouseClick;
            this.GVRequestIntercept.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.GVRequestIntercept.ContextMenuStrip = new ContextMenuStrip();
            this.GVRequestIntercept.ContextMenuStrip.Items.Add("删除");
            this.GVRequestIntercept.ContextMenuStrip.Items.Add("禁用");
            this.GVRequestIntercept.ContextMenuStrip.Items.Add("启用");
            this.GVRequestIntercept.ContextMenuStrip.Items.Add("新增");
            GVRequestIntercept.ContextMenuStrip.VisibleChanged += ContextMenuStrip_VisibleChanged;
            this.GVRequestIntercept.ContextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked; ;
            this.GVRequestIntercept.CellDoubleClick += GVRequestIntercept_CellDoubleClick; ;
            this.GVRequestIntercept.BorderStyle = BorderStyle.None;
            this.GVRequestIntercept.GridColor = Color.LightBlue;
            GVRequestIntercept.BackgroundColor = Color.White;

            this.GVRequestIntercept.AllowUserToResizeRows = true;

            GVRequestIntercept.RowHeadersVisible = false;
            GVRequestIntercept.DataBindingComplete += GVRequestIntercept_DataBindingComplete;

            if (_testCase != null)
            {
                var configs = BigEntityTableRemotingEngine.Find<RequestInterceptConfig>(nameof(RequestInterceptConfig),
                     nameof(RequestInterceptConfig.TestCaseId), new object[] { _testCase.Id }).ToList();

                GVRequestIntercept.DataSource = configs;
            }

            var dic = new List<object>
            {
                new
                {
                    Key=0,
                    Value="相同"
                },
                 new
                {
                    Key=1,
                    Value="包含"
                },
                  new
                {
                    Key=2,
                    Value="正则"
                }
            };

            CBTypes.DataSource = dic;
            CBTypes.ValueMember = "Key";
            CBTypes.DisplayMember = "Value";
            CBTypes.SelectedValue = 0;

            CBMimeType.DataSource = MimeResource.GetMimes();
            CBMimeType.ValueMember = "MimeName";
            CBMimeType.DisplayMember = "MimeName";
            CBMimeType.SelectedIndex = 0;
        }

        private void ContextMenuStrip_VisibleChanged(object sender, EventArgs e)
        {
            if (GVRequestIntercept.ContextMenuStrip.Visible)
            {
                var currentRow = GVRequestIntercept.CurrentRow;
                if (currentRow != null)
                {
                    var config = currentRow.DataBoundItem as RequestInterceptConfig;

                    GVRequestIntercept.ContextMenuStrip.Items[1].Enabled = config.Enabled;
                    GVRequestIntercept.ContextMenuStrip.Items[2].Enabled = !config.Enabled;

                }
            }
        }

        private void GVRequestIntercept_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void GVRequestIntercept_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            _currentInterceptConfig = (GVRequestIntercept.DataSource as List<RequestInterceptConfig>)[e.RowIndex];
            TBUrl.Text = _currentInterceptConfig.MatchUrl;
            TBContent.Text = _currentInterceptConfig.Response??string.Empty;
            CBTypes.SelectedValue = _currentInterceptConfig.MatchType;
            CBEnable.Checked = _currentInterceptConfig.Enabled;
            CBMimeType.SelectedValue = _currentInterceptConfig.MimeType;

            GVRequestIntercept.Visible = false;
            GBEdit.Visible = true;
            GBEdit.Dock = DockStyle.Fill;
            GBEdit.Text = "编辑";
        }

        private void GVRequestIntercept_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
        }

        private void BindData()
        {
            var configs = BigEntityTableRemotingEngine.Find<RequestInterceptConfig>(nameof(RequestInterceptConfig),
                    nameof(RequestInterceptConfig.TestCaseId), new object[] { _testCase.Id }).ToList();

            GVRequestIntercept.DataSource = configs;
        }

        private void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "新增":
                    {
                        _currentInterceptConfig = new RequestInterceptConfig()
                        {
                            TestCaseId=_testCase.Id
                        };
                        GBEdit.Text = "新增";
                        TBUrl.Text = string.Empty;
                        CBTypes.SelectedValue = 0;
                        CBEnable.Checked = true;
                        TBContent.Text = string.Empty;
                        GVRequestIntercept.Visible = false;
                        GBEdit.Visible = true;
                        GBEdit.Dock = DockStyle.Fill;
                        _file = null;
                        break;
                    }
                case "启用":
                    {
                        var row=GVRequestIntercept.CurrentRow;
                        if (row != null)
                        {
                            var config = (row.DataBoundItem as RequestInterceptConfig);
                            config.Enabled = true;
                            BigEntityTableRemotingEngine.Update(nameof(RequestInterceptConfig), config);
                            BindData();
                        }
                        break;
                    }
                case "禁用":
                    {
                        var row = GVRequestIntercept.CurrentRow;
                        if (row != null)
                        {
                            var config = (row.DataBoundItem as RequestInterceptConfig);
                            config.Enabled = false;
                            BigEntityTableRemotingEngine.Update(nameof(RequestInterceptConfig), config);
                            BindData();
                        }
                        break;
                    }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_currentInterceptConfig != null)
            {
                if (string.IsNullOrEmpty(TBUrl.Text))
                {
                    MessageBox.Show("地址不能为空");
                    return;
                }

                if ((int)CBTypes.SelectedValue == 2)
                {
                    try
                    {
                        _ = Regex.IsMatch("http://www.abc.com/def/gi", TBUrl.Text, RegexOptions.IgnoreCase);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("正则表达式有误:"+ex.Message);
                        return;
                    }
                }

                _currentInterceptConfig.MatchUrl = TBUrl.Text;
                _currentInterceptConfig.MatchType = (int)CBTypes.SelectedValue;
                _currentInterceptConfig.Enabled = CBEnable.Checked;
                if (_file != null && File.Exists(_file))
                {
                    _currentInterceptConfig.ResponseData = File.ReadAllBytes(_file);
                    _currentInterceptConfig.Response = null;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(TBContent.Text))
                    {
                        _currentInterceptConfig.ResponseData = null;
                        _currentInterceptConfig.Response = TBContent.Text;
                    }
                }
                _currentInterceptConfig.MimeType = (string)CBMimeType.SelectedValue;

                BigEntityTableRemotingEngine.Upsert(nameof(RequestInterceptConfig), _currentInterceptConfig);

                GBEdit.Visible = false;
                _currentInterceptConfig = null;
                

                GVRequestIntercept.Visible = true;
                GVRequestIntercept.Dock = DockStyle.Fill;

                BindData();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            _currentInterceptConfig = null;
            _file = null;
            GBEdit.Visible = false;
            GVRequestIntercept.Visible = true;
            GVRequestIntercept.Dock = DockStyle.Fill;
        }

        private void BtnChooseFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _file = dlg.FileName;
            }
        }
    }
}
