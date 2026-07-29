using AutoTest.Domain.Entity;
using AutoTest.UI.WebBrowser.ResourceRequestHandler;
using CefSharp;
using LJC.FrameWorkV3.LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.WebBrowser.RequestHandler
{
    public class GrabRequestHandler : IRequestHandler
    {
        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            if (isProxy)
            {
                var config = AutofacBuilder.GetFromFac<TestConfig>();
                if (config.ProxyConfig != null && config.ProxyConfig.Enabled == true && !string.IsNullOrWhiteSpace(config.ProxyConfig.ProxyUri))
                {
                    callback.Continue(config.ProxyConfig.UserName, config.ProxyConfig.Password);
                }
                return true;
            }
            return false;
        }

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new GrabResourceRequestHandler();
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            return false;
        }

        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            return false;
        }

        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        public void OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath)
        {
            LogHelper.Instance.Error($"Grab浏览器插件崩溃: {pluginPath}");
        }

        public bool OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
        {
            LogHelper.Instance.Error($"Grab浏览器渲染进程终止(无错误码): Status={status}");

            TryRecoverBrowser(chromiumWebBrowser, browser);
        }

        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status, int errorCode, string errorMessage)
        {
            LogHelper.Instance.Error($"Grab浏览器渲染进程终止: Status={status}, ErrorCode={errorCode}, ErrorMsg={errorMessage}");

            TryRecoverBrowser(chromiumWebBrowser, browser);
        }

        private static void TryRecoverBrowser(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            if (chromiumWebBrowser is Control control && !control.IsDisposed)
            {
                control.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (!control.IsDisposed && browser != null && !browser.IsDisposed)
                        {
                            browser.Reload();
                            LogHelper.Instance.Info("Grab浏览器渲染进程恢复成功");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Error("Grab浏览器渲染进程恢复失败", ex);
                    }
                }));
            }
        }

        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return false;
        }
    }
}
