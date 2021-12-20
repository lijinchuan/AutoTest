using AutoTest.Domain.Model;
using AutoTest.UI.WebTask;
using CefSharp;
using CefSharp.WinForms;
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

        private static string userFileDir = "userData\\";

        public event Action<string> OnPublishMsg;

        static CSObj()
        {
            if (!Directory.Exists(userFileDir))
            {
                Directory.CreateDirectory(userFileDir);
            }

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
            var path = userFileDir + fileName;
            if (File.Exists(path) &&!replace)
            {
                return false;
            }

            File.WriteAllText(path, content, Encoding.UTF8);
            return true;
        }

        public string ReadFile(string fileName)
        {
            var path = userFileDir + fileName;
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            else
            {
                return string.Empty;
            }
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

        public void Dispose()
        {
            this.OnPublishMsg = null;
        }
    }
}
