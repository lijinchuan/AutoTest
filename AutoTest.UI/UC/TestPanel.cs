using AutoTest.Domain.Entity;
using AutoTest.UI.WebBrowser;
using AutoTest.UI.WebTask;
using AutoTest.Util;
using CefSharp.Enums;
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

        public bool AutoCloseWhenCompleted { get; set; }

        private CancellationTokenSource _autoCloseCts = null;

        public TestPanel()
        {
            InitializeComponent();
        }

        public TestPanel(string name)
        {
            InitializeComponent();
            _name = name;
        }

        public List<IWebTask> GetTaskList()
        {
            if (webView == null)
            {
                return new List<IWebTask>();
            }
            return webView.GetTaskList();
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
                            //ThreadPool.GetMaxThreads(out int work, out int completionPortNum);
                            ThreadPool.GetMinThreads(out int minWork, out int minCompletionPortNum);
                            ThreadPool.GetMaxThreads(out int maxWork, out int maxCompletionPortNum);
                            ThreadPool.GetAvailableThreads(out int aWork, out int aCompletionPortNum);
                            tbMsg.AppendText(msg + ("MaxThreads(" + (minWork - maxWork + aWork) + "," + (minCompletionPortNum - maxCompletionPortNum + aCompletionPortNum) + ")") + Environment.NewLine);
                        }
                    }));
                });
                webView.OnTaskStart += WebView_OnTaskStart;
                webView.OnAllTasksCompleted += WebView_OnAllTasksCompleted;
            }
        }

        private void WebView_OnAllTasksCompleted()
        {
            if (!AutoCloseWhenCompleted || IsDisposed)
            {
                return;
            }

            var cts = new CancellationTokenSource();
            var old = Interlocked.Exchange(ref _autoCloseCts, cts);
            old?.Cancel();

            var token = cts.Token;
            Task.Delay(TimeSpan.FromMinutes(1), token).ContinueWith(t =>
            {
                if (t.IsCanceled || IsDisposed)
                {
                    return;
                }

                void closeTab()
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    if (webView != null && webView.IsRunningJob())
                    {
                        return;
                    }

                    if (Parent is TabControl tabControl && tabControl.TabPages.Contains(this))
                    {
                        tabControl.TabPages.Remove(this);
                        Dispose();
                    }
                }

                if (Parent != null && Parent.InvokeRequired)
                {
                    Parent.Invoke((Action)closeTab);
                }
                else
                {
                    closeTab();
                }
            }, TaskScheduler.Default);
        }

        private void WebView_OnTaskStart(IWebTask task)
        {
            var old = Interlocked.Exchange(ref _autoCloseCts, null);
            old?.Cancel();

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

        public bool SetCookie(string url,List<TestCookie> cookies)
        {
            foreach (var cookie in cookies)
            {
                webView.SetCookie(url, new CefSharp.Cookie
                {
                    Domain = cookie.Domain,
                    HttpOnly = cookie.HttpOnly,
                    Name = cookie.Name,
                    Expires = cookie.Expires,
                    Path = cookie.Path,
                    Priority = (CookiePriority)cookie.Priority,
                    SameSite = (CookieSameSite)cookie.SameSite,
                    Secure = cookie.Secure,
                    Value = cookie.Value
                });
            }

            return true;
        }

        public bool Reset()
        {
            if (!webView.IsRunningJob())
            {
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
