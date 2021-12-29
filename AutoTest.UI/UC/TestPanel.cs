using AutoTest.UI.WebBrowser;
using AutoTest.UI.WebTask;
using AutoTest.Util;
using LJC.FrameWorkV3.LogManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.UC
{
    public partial class TestPanel : TabPage//UserControl
    {
        private string _name = string.Empty;
        private DefaultChromiumWebBrowser webView = null;
        public event Action<IWebTask> OnTaskStart;

        public TestPanel()
        {
            InitializeComponent();
        }

        public TestPanel(string name)
        {
            InitializeComponent();
            _name = name;
        }

        public async Task RunTest(IEnumerable<IWebTask> webTasks)
        {
            foreach (var webTask in webTasks)
            {
                this.webView.AddTask(webTask);
            }
            new Action(() =>
            {
                this.webView.RunTask();
            }).BeginInvoke(null, null);
        }

        public bool IsRunning()
        {
            return this.webView.IsRunningJob();
        }

        public async Task RunTest(IWebTask webTask)
        {
            this.webView.AddTask(webTask);
            new Action(() =>
            {
                this.webView.RunTask();
            }).BeginInvoke(null, null);
        }

        public void CancelTasks()
        {
            webView.CancelTasks();
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
                    LogHelper.Instance.Debug(msg);
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
                webView.OnTaskStart += WebView_OnTaskStart;
            }
        }

        private void WebView_OnTaskStart(IWebTask task)
        {
            OnTaskStart?.Invoke(task);
            EventBus.NotifyTestStartAction?.Invoke(task);
        }

        public void Load()
        {
            LoadWebBrowser();
        }

        public bool ClearCookie(string url)
        {
            return webView.ClearCookie(url);
        }

        public bool Reset()
        {
            if (!webView.IsRunningJob())
            {
                this.OnTaskStart = null;
                return true;
            }

            return false;
        }

        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);

        //    LoadWebBrowser();
        //}
    }
}
