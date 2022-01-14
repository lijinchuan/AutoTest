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
    public partial class CompareWebDlg : SubBaseDlg
    {
        private ChromiumWebBrowser leftChromiumWebBrowser = null;
        private ChromiumWebBrowser rightChromiumWebBrowser = null;
        private List<string> TempFiles = new List<string>();
        private string leftHtml = null, rightHtml = null;

        public CompareWebDlg()
        {
            InitializeComponent();
        }

        public CompareWebDlg(string title,string leftHtml,string rightHtml)
        {
            InitializeComponent();
            this.leftHtml = leftHtml;
            this.rightHtml = rightHtml;
            Text = title;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            leftChromiumWebBrowser = new ChromiumWebBrowser("about:_blank");

            PanelLeft.Controls.Add(leftChromiumWebBrowser);

            if (!string.IsNullOrWhiteSpace(leftHtml))
            {
                _ = leftChromiumWebBrowser.WaitForInitialLoadAsync().Result;
                LoadHtml(leftChromiumWebBrowser, leftHtml);
            }

            rightChromiumWebBrowser = new ChromiumWebBrowser("about:_blank");

            PanelRight.Controls.Add(rightChromiumWebBrowser);

            if (!string.IsNullOrWhiteSpace(rightHtml))
            {
                _ = rightChromiumWebBrowser.WaitForInitialLoadAsync().Result;
                LoadHtml(rightChromiumWebBrowser, rightHtml);
            }
        }

        public void LoadUrl(ChromiumWebBrowser browser,string url)
        {
            browser.LoadUrl(url);
        }

        public void LoadHtml(ChromiumWebBrowser browser, string html)
        {
            var md5 = HashEncrypt.MD5_JS(html);
            var file = "CefSharp\\" + md5 + ".html";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, html, Encoding.UTF8);

                TempFiles.Add(file);
            }
            LoadUrl(browser,CommFun.GetRuningPath() + file);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                leftChromiumWebBrowser.Dispose();
            }
            catch
            {

            }

            try
            {
                rightChromiumWebBrowser.Dispose();
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
