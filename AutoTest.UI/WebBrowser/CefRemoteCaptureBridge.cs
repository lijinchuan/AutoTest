using AutoTest.Biz.RemoteControl;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.WebBrowser
{
    public static class CefRemoteCaptureBridge
    {
        private static readonly object locker = new object();
        private static readonly HashSet<ChromiumWebBrowser> browsers = new HashSet<ChromiumWebBrowser>();
        private static bool initialized = false;

        public static void Register(ChromiumWebBrowser browser)
        {
            if (browser == null)
                return;

            EnsureInitialized();

            lock (locker)
            {
                if (!browsers.Add(browser))
                    return;
            }

            browser.Disposed += Browser_Disposed;
        }

        private static void Browser_Disposed(object sender, EventArgs e)
        {
            if (!(sender is ChromiumWebBrowser browser))
                return;

            browser.Disposed -= Browser_Disposed;
            lock (locker)
            {
                browsers.Remove(browser);
            }
        }

        private static void EnsureInitialized()
        {
            if (initialized)
                return;

            lock (locker)
            {
                if (initialized)
                    return;

                ScreenCaptureService.Instance.CefOverlayCaptureProvider = CaptureOverlay;
                initialized = true;
            }
        }

        private static Bitmap CaptureOverlay(Rectangle captureRegion)
        {
            List<ChromiumWebBrowser> snapshot;
            lock (locker)
            {
                snapshot = browsers.Where(p => p != null && !p.IsDisposed).ToList();
            }

            if (snapshot.Count == 0 || captureRegion.Width <= 0 || captureRegion.Height <= 0)
                return null;

            var overlay = new Bitmap(captureRegion.Width, captureRegion.Height, PixelFormat.Format32bppArgb);
            var hasDraw = false;

            using (var g = Graphics.FromImage(overlay))
            {
                g.Clear(Color.Transparent);

                foreach (var browser in snapshot)
                {
                    DrawSingleBrowser(browser, captureRegion, g, ref hasDraw);
                }
            }

            if (!hasDraw)
            {
                overlay.Dispose();
                return null;
            }

            return overlay;
        }

        private static void DrawSingleBrowser(ChromiumWebBrowser browser, Rectangle captureRegion, Graphics g, ref bool hasDraw)
        {
            Rectangle browserRect = Rectangle.Empty;
            if (!TryInvoke(browser, () =>
            {
                if (!browser.IsHandleCreated || browser.Width <= 0 || browser.Height <= 0 || !browser.Visible)
                    return false;

                browserRect = browser.RectangleToScreen(new Rectangle(0, 0, browser.Width, browser.Height));
                return true;
            }))
            {
                return;
            }

            if (browserRect.Width <= 0 || browserRect.Height <= 0)
                return;

            var hit = Rectangle.Intersect(browserRect, captureRegion);
            if (hit.Width <= 0 || hit.Height <= 0)
                return;

            Bitmap browserBmp = null;
            try
            {
                var task = browser.CaptureScreenshotAsync();
                if (!task.Wait(3000) || task.IsFaulted || task.Result == null)
                    return;

                using (var ms = new MemoryStream(task.Result))
                {
                    browserBmp = new Bitmap(ms);
                }
            }
            catch
            {
                return;
            }

            if (browserBmp == null)
                return;

            using (browserBmp)
            {
                var srcRect = new Rectangle(hit.X - browserRect.X, hit.Y - browserRect.Y, hit.Width, hit.Height);
                srcRect = Rectangle.Intersect(srcRect, new Rectangle(0, 0, browserBmp.Width, browserBmp.Height));
                if (srcRect.Width <= 0 || srcRect.Height <= 0)
                    return;

                var destRect = new Rectangle(hit.X - captureRegion.X, hit.Y - captureRegion.Y, srcRect.Width, srcRect.Height);
                g.DrawImage(browserBmp, destRect, srcRect, GraphicsUnit.Pixel);
                hasDraw = true;
            }
        }

        private static bool TryInvoke(Control control, Func<bool> action)
        {
            try
            {
                if (control.IsDisposed)
                    return false;

                if (control.InvokeRequired)
                {
                    return (bool)control.Invoke(action);
                }

                return action();
            }
            catch
            {
                return false;
            }
        }
    }
}
