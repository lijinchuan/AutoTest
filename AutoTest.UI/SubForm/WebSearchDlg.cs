using AutoTest.UI.WebBrowser;
using AutoTest.UI.WebBrowser.EventListener;
using LJC.FrameWorkV3.Comm;
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
    public partial class WebSearchDlg : Form
    {
        GrabChromiumWebBrowser _browser = null;
        EventListener _eventListener = null;

        bool stop = false;

        private List<WebEvent> webEvents = new List<WebEvent>();

        int status = 0;

        public WebSearchDlg()
        {
            InitializeComponent();
        }

        private void WebSearchDlg_Load(object sender, EventArgs e)
        {
            _browser = new GrabChromiumWebBrowser("Grab", "about:_blank");
            _eventListener = new GrabEventListener();
            _eventListener.OnProcess += _eventListener_OnProcess;
            _browser.AddEventListener(_eventListener);
            _browser.Dock = DockStyle.Fill;
            PanelLeft.Controls.Add(_browser);

            DGVResult.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DGVResult.BorderStyle = BorderStyle.None;
            DGVResult.GridColor = Color.LightBlue;
            DGVResult.RowHeadersVisible = false;
            DGVResult.BackgroundColor = Color.White;

            PBBtn.Image = Resources.Resource1.play_green;

            DGVResult.ContextMenuStrip = new ContextMenuStrip();
            DGVResult.ContextMenuStrip.Items.Add("复制URL");
            DGVResult.ContextMenuStrip.Items.Add("复制内容");
            DGVResult.ContextMenuStrip.Items.Add("查看文本");
            DGVResult.ContextMenuStrip.Items.Add("搜索结果");
            DGVResult.ContextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;
            DGVResult.CellDoubleClick += DGVResult_CellDoubleClick;
        }

        private void ShowContent()
        {
            var selrow = DGVResult.CurrentRow;
            if (selrow == null)
            {
                return;
            }
            var url = selrow.Cells["网址"].Value.ToString();
            var txt = selrow.Cells["文本"].Value.ToString();
            TextBoxWin textBoxWin = new TextBoxWin(url, txt);
            textBoxWin.Show();
            textBoxWin.BringToFront();

        }

        private void DGVResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ShowContent();
        }

        private void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var selrow = DGVResult.CurrentRow;
            if (selrow == null)
            {
                return;
            }
            switch (e.ClickedItem.Text)
            {
                case "复制URL":
                    {
                        Clipboard.SetText(selrow.Cells["网址"].Value.ToString());
                        MessageBox.Show("已复制到剪贴板");
                        break;
                    }
                case "复制内容":
                    {
                        Clipboard.SetText(selrow.Cells["文本"].Value.ToString());
                        MessageBox.Show("已复制到剪贴板");
                        break;
                    }
                case "搜索结果":
                    {
                        var txt = selrow.Cells["搜索结果"].Value.ToString();
                        TextBoxWin textBoxWin = new TextBoxWin("搜索结果", txt);
                        textBoxWin.Show();
                        textBoxWin.BringToFront();
                        break;
                    }
                case "查看文本":
                    {
                        ShowContent();
                        break;
                    }
            }
        }

        private bool _eventListener_OnProcess(WebEvent evt)
        {
            if (!stop)
            {
                webEvents.Add(evt);
            }
            return true;
        }

        private void PanelLeft_Paint(object sender, PaintEventArgs e)
        {

        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            var searchContext = TBSearchContent.Text;
            if (string.IsNullOrWhiteSpace(searchContext))
            {
                return;
            }
            var list = new List<WebEvent>();
            foreach (var evt in webEvents.ToList())
            {
                if (evt.Content != null)
                {
                    if (evt.Content.IndexOf(searchContext) > -1)
                    {
                        list.Add(evt);
                    }
                }
            }

            DGVResult.DataBindingComplete += DGVResult_DataBindingComplete;
            DGVResult.DataSource = list.Select(p => {
                var sr = StringHelper.SubSearch(p.Content, searchContext);
                return new
                {
                    网址 = p.SourceUrl,
                    文本 = p.Content,
                    mime = p.DataType,
                    出现数量=sr.SubSearchResultItems.Count,
                    搜索结果 =string.Join(Environment.NewLine, sr.SubSearchResultItems.Select(q =>
                    {
                        var w = p.Content.Substring(Math.Max(0, q.StartPos - 50), Math.Min(q.Len + 100, p.Content.Length - Math.Max(0, q.StartPos - 50)));
                        return w;
                    }))
                };
            }).ToList();
           
        }

        private void DGVResult_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DGVResult.Columns["文本"].Visible = false;
            DGVResult.Columns["搜索结果"].Visible = false;
        }

        private void PBBtn_Click(object sender, EventArgs e)
        {
            if (status == 0)
            {
                stop = false;
                webEvents.Clear();
                var url = TBUrl.Text;
                status = 1;
                PBBtn.Image = Resources.Resource1.record_green;
                if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    _browser.Load(url);
                }
            }
            else if (status == 1)
            {
                status = 2;
                stop = true;
                PBBtn.Image = Resources.Resource1.stop;
            }
            else if (status == 2)
            {
                status = 0;
                stop = true;
                
                PBBtn.Image = Resources.Resource1.play_green;
            }
        }
    }
}
