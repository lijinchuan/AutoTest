using CefSharp.Web;
using CefSharp.WinForms;
using LJC.FrameWorkV3.Comm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.SubForm
{
    public partial class PopWebDlg : SubBaseDlg
    {
        private ChromiumWebBrowser chromiumWebBrowser = null;

        private string initHtml = null;

        private List<string> TempFiles = new List<string>();

        public PopWebDlg()
        {
            InitializeComponent();
        }

        public PopWebDlg(string title,string html)
        {
            InitializeComponent();
            initHtml = html;
            Text = title;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            chromiumWebBrowser = new ChromiumWebBrowser("about:_blank");

            this.Controls.Add(chromiumWebBrowser);

            if (!string.IsNullOrWhiteSpace(initHtml))
            {
                _ = chromiumWebBrowser.WaitForInitialLoadAsync().Result;
                LoadHtml(initHtml);
            }

        }

        public void LoadUrl(string url)
        {
            chromiumWebBrowser.LoadUrl(url);
        }

        public void LoadHtml(string html)
        {
            var md5 = HashEncrypt.MD5_JS(html);
            var file = "CefSharp\\" + md5 + ".html";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, html, Encoding.UTF8);

                TempFiles.Add(file);
            }
            LoadUrl(CommFun.GetRuningPath() + file);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                chromiumWebBrowser.Dispose();
            }
            catch
            {

            }
            foreach (var f in TempFiles)
            {
                try
                {
                    File.Delete(f);
                }
                catch
                {

                }
            }
            base.OnClosing(e);
        }
    }
}
