using AutoTest.UI.WebBrowser;
using AutoTest.UI.WebBrowser.EventListener;
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
        }

        private bool _eventListener_OnProcess(WebEvent evt)
        {
            webEvents.Add(evt);
            return true;
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            stop = false;
            webEvents.Clear();
            var url = TBUrl.Text;
            if(url.StartsWith("http://",StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                _browser.Load(url);
            }
            
        }

        private void BtnEnd_Click(object sender, EventArgs e)
        {
            if (stop)
            {

            }
            else
            {
                stop = true;
            }
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

            DGVResult.DataSource = list.Select(p => new
            {
                网址 = p.SourceUrl
            }).ToList();
        }
    }
}
