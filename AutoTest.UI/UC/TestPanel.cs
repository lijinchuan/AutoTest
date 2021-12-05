using AutoTest.UI.WebBrowser;
using AutoTest.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.UC
{
    public partial class TestPanel : TabPage//UserControl
    {
        private string _name = string.Empty;
        private DefaultChromiumWebBrowser webView = null;
        public TestPanel()
        {
            InitializeComponent();
        }

        public TestPanel(string name)
        {
            InitializeComponent();
            _name = name;
        }

        private void LoadWebBrowser()
        {
            if (webView == null)
            {
                webView = new DefaultChromiumWebBrowser(_name, "about:blank");
                webView.Dock = DockStyle.Fill;
                PannelLeft.Controls.Add(webView);
                tbMsg.Text = "";
                webView.OnMsgPublished += (msg =>
                {
                    _ = BeginInvoke(new Action(() =>
                    {
                        if (tbMsg.Text.Length > 1024 * 100)
                        {
                            tbMsg.Text = tbMsg.Text.Substring(tbMsg.Text.Length - 10240, 10240);
                        }
                        if (msg == Consts.CMDCLEARMSG)
                        {
                            tbMsg.ResetText();
                        }
                        else
                        {
                            tbMsg.AppendText(msg + Environment.NewLine);
                        }
                    }));
                });
            }
        }

        public void Load()
        {
            LoadWebBrowser();
        }

        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);

        //    LoadWebBrowser();
        //}
    }
}
