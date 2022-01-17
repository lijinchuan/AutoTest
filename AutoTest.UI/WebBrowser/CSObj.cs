using AutoTest.Domain.Entity;
using AutoTest.Domain.Model;
using AutoTest.UI.WebTask;
using CefSharp;
using CefSharp.WinForms;
using LJC.FrameWorkV3.Comm;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.WebBrowser
{
    public class CSObj:IDisposable
    {
        private SynchronizationContext context;
        private ChromiumWebBrowser browser;
        private IWebTask currentWebTask;

        public event Action<string> OnPublishMsg;

        static CSObj()
        {

        }

        public void SetWebTask(IWebTask webTask)
        {
            currentWebTask = webTask;
        }

        public CSObj(SynchronizationContext context, ChromiumWebBrowser browser)
        {
            this.context = context;
            this.browser = browser;
        }
        public void ShowMsg(string msg)
        {
            MessageBox.Show(msg);
        }

        public void Click(int x,int y)
        {
            MouseDown(x, y);
            Thread.Sleep(2);
            MouseUp(x, y);
        }

        public void MouseDown(int x, int y)
        {
            var host = browser.GetBrowser().GetHost();

            var mouseDown = new MouseEvent(x, y, CefEventFlags.LeftMouseButton);
            host.SendMouseClickEvent(mouseDown, MouseButtonType.Left, false, 1);
        }

        public void MouseUp(int x, int y)
        {
            var host = browser.GetBrowser().GetHost();

            var mouseUp = new MouseEvent(x, y, CefEventFlags.LeftMouseButton);
            host.SendMouseClickEvent(mouseUp, MouseButtonType.Left, true, 1);
        }

        public void MouseMove(int x, int y)
        {
            var host = browser.GetBrowser().GetHost();

            var mouseMove = new MouseEvent(x, y, CefEventFlags.LeftMouseButton);
            host.SendMouseMoveEvent(mouseMove, false);
        }

        public void Sleep(int ms)
        {
            var task = Task.Delay(ms);
            task.Wait();
        }

        public bool SaveFile(string fileName,bool replace,string content)
        {
            var isExists = BigEntityTableRemotingEngine.Count(nameof(FileDB), nameof(FileDB.FileName), new object[] { fileName }) > 0;
            if (!replace && isExists)
            {
                return false;
            }

            if (!isExists)
            {
                BigEntityTableRemotingEngine.Insert(nameof(FileDB), new FileDB
                {
                    CDate = DateTime.Now,
                    MDate = DateTime.Now,
                    FileName = fileName,
                    FileContent = content
                });
            }
            else
            {
                var file = BigEntityTableRemotingEngine.Find<FileDB>(nameof(FileDB), nameof(FileDB.FileName), new object[] { fileName }).First();
                file.MDate = DateTime.Now;
                file.FileContent = content;

                BigEntityTableRemotingEngine.Update<FileDB>(nameof(FileDB), file);
            }
            

            return true;
        }

        public string ReadFile(string fileName)
        {
            var file = BigEntityTableRemotingEngine.Find<FileDB>(nameof(FileDB), nameof(FileDB.FileName), new object[] { fileName }).FirstOrDefault();

            if (file == null)
            {
                return string.Empty;
            }

            return file.FileContent;
        }

        public string GetLastAlertMsg()
        {
            return (browser.JsDialogHandler as JsDialogHandler)?.LastAlertMsg;
        }

        public void Ehco(string msg)
        {
            this.OnPublishMsg?.Invoke(msg);
        }

        public string GetWebRequestData(string url)
        {
            if (currentWebTask == null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(new List<WebRequestData>());
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(currentWebTask.GetWebRequestData(url));
        }

        public void Reload(bool ignoreCache)
        {
            browser.GetBrowser().Reload(ignoreCache);
        }

        public void LoadUrl(string url)
        {
            browser.GetBrowser().MainFrame.LoadUrl(url);
        }

        public void SetFileValue(string file,string script)
        {
            var path=Path.Combine(CommFun.GetRuningPath() + "Images",file);
            if (!File.Exists(path))
            {
                browser.GetMainFrame().ExecuteJavaScriptAsync($"alert('{path.Replace("\\","\\\\")}不存在')");
                return;
            }
            if (browser.DialogHandler is CommonFileDialogHandler)
            {
                (browser.DialogHandler as CommonFileDialogHandler).GetFiles = () =>
                {
                    return new List<string> { path };
                };
                if (!string.IsNullOrWhiteSpace(script))
                {
                    browser.GetMainFrame().ExecuteJavaScriptAsync(script);
                }
            }
        }

        public void OpenWindow(string title, string html)
        {
            MainFrm.Instance.BeginInvoke(
            new Action(() =>
            {
                var win = new SubForm.PopWebDlg(title, html);
                win.Show();
                win.BringToFront();
            }));

        }

        public void CompareHtml(string title,string oldHtml,string newHtml)
        {
            MainFrm.Instance.BeginInvoke(
            new Action(() =>
            {
                var win = new SubForm.CompareWebDlg($"{title}（左边是旧的HTML，右边是新的HTML）", oldHtml,newHtml);
                win.Show();
                win.BringToFront();
            }));
        }

        public long Inc(string name, long init = 0)
        {
            return Biz.CounterBiz.Inc(name, init);
        }

        public long GetCounterVal(string name)
        {
            return Biz.CounterBiz.GetCounterVal(name);
        }

        public void Dispose()
        {
            this.OnPublishMsg = null;
        }
    }
}
