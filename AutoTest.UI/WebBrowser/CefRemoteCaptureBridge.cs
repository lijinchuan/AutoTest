using AutoTest.Biz.RemoteControl;
using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.WebBrowser
{
    public static class CefRemoteCaptureBridge
    {
        private sealed class BrowserCaptureState
        {
            public readonly object SyncRoot = new object();
            public Bitmap Frame;
            public int LastCaptureTick;
            public int LastInvalidateTick;
            public int Capturing;
        }

        private static readonly object locker = new object();
        private static readonly Dictionary<ChromiumWebBrowser, BrowserCaptureState> browserStates = new Dictionary<ChromiumWebBrowser, BrowserCaptureState>();
        private static bool initialized = false;
        private const int CaptureRefreshIntervalMs = 80;
        private const int CaptureTimeoutMs = 1200;
        private const int MaxFrameAgeMs = 500;
        private const int InvalidateMinIntervalMs = 350;

        public static void Register(ChromiumWebBrowser browser)
        {
            if (browser == null)
                return;

            EnsureInitialized();

            lock (locker)
            {
                if (browserStates.ContainsKey(browser))
                    return;

                browserStates[browser] = new BrowserCaptureState();
            }

            browser.Disposed += Browser_Disposed;
        }

        private static void Browser_Disposed(object sender, EventArgs e)
        {
            if (!(sender is ChromiumWebBrowser browser))
                return;

            browser.Disposed -= Browser_Disposed;

            BrowserCaptureState state = null;
            lock (locker)
            {
                if (browserStates.TryGetValue(browser, out state))
                {
                    browserStates.Remove(browser);
                }
            }

            DisposeState(state);
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
            List<KeyValuePair<ChromiumWebBrowser, BrowserCaptureState>> snapshot;
            lock (locker)
            {
                snapshot = browserStates.Where(p => p.Key != null && !p.Key.IsDisposed).ToList();
            }

            if (snapshot.Count == 0 || captureRegion.Width <= 0 || captureRegion.Height <= 0)
                return null;

            var overlay = new Bitmap(captureRegion.Width, captureRegion.Height, PixelFormat.Format32bppArgb);
            var hasDraw = false;

            using (var g = Graphics.FromImage(overlay))
            {
                g.Clear(Color.Transparent);

                foreach (var item in snapshot)
                {
                    DrawSingleBrowser(item.Key, item.Value, captureRegion, g, ref hasDraw);
                }
            }

            if (!hasDraw)
            {
                overlay.Dispose();
                return null;
            }

            return overlay;
        }

        private static void DrawSingleBrowser(ChromiumWebBrowser browser, BrowserCaptureState state, Rectangle captureRegion, Graphics g, ref bool hasDraw)
        {
            Rectangle browserRect = Rectangle.Empty;
            if (!TryInvoke(browser, () =>
            {
                if (!browser.IsHandleCreated || browser.Width <= 0 || browser.Height <= 0 || !browser.Visible || !browser.IsBrowserInitialized)
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

            TryRefreshFrameAsync(browser, state);

            lock (state.SyncRoot)
            {
                if (state.Frame == null)
                    return;

                var age = unchecked(Environment.TickCount - state.LastCaptureTick);
                if (age > MaxFrameAgeMs)
                {
                    state.Frame.Dispose();
                    state.Frame = null;
                    return;
                }

                var srcRect = new Rectangle(hit.X - browserRect.X, hit.Y - browserRect.Y, hit.Width, hit.Height);
                srcRect = Rectangle.Intersect(srcRect, new Rectangle(0, 0, state.Frame.Width, state.Frame.Height));
                if (srcRect.Width <= 0 || srcRect.Height <= 0)
                    return;

                var destRect = new Rectangle(hit.X - captureRegion.X, hit.Y - captureRegion.Y, srcRect.Width, srcRect.Height);
                g.DrawImage(state.Frame, destRect, srcRect, GraphicsUnit.Pixel);
                hasDraw = true;
            }
        }

        private static void TryRefreshFrameAsync(ChromiumWebBrowser browser, BrowserCaptureState state)
        {
            var now = Environment.TickCount;
            lock (state.SyncRoot)
            {
                if (state.Frame != null && unchecked(now - state.LastCaptureTick) < CaptureRefreshIntervalMs)
                    return;
            }

            if (Interlocked.CompareExchange(ref state.Capturing, 1, 0) != 0)
                return;

            Task.Run(async () =>
            {
                Bitmap newFrame = null;
                try
                {
                    if (browser.IsDisposed)
                        return;

                    var needInvalidate = false;
                    lock (state.SyncRoot)
                    {
                        var frameAge = state.Frame == null ? int.MaxValue : unchecked(now - state.LastCaptureTick);
                        var invalidateAge = unchecked(now - state.LastInvalidateTick);
                        needInvalidate = state.Frame == null || (frameAge >= CaptureRefreshIntervalMs && invalidateAge >= InvalidateMinIntervalMs);
                    }

                    if (needInvalidate)
                    {
                        if (TryInvoke(browser, () =>
                        {
                            if (browser.IsDisposed || !browser.IsBrowserInitialized)
                                return false;

                            var host = browser.GetBrowserHost();
                            if (host == null)
                                return false;

                            host.Invalidate(PaintElementType.View);
                            return true;
                        }))
                        {
                            lock (state.SyncRoot)
                            {
                                state.LastInvalidateTick = Environment.TickCount;
                            }
                        }
                    }

                    var captureTask = browser.CaptureScreenshotAsync();
                    var completed = await Task.WhenAny(captureTask, Task.Delay(CaptureTimeoutMs)).ConfigureAwait(false);
                    if (completed != captureTask)
                        return;

                    var bytes = captureTask.Result;
                    if (bytes == null || bytes.Length == 0)
                        return;

                    using (var ms = new MemoryStream(bytes))
                    using (var bmp = new Bitmap(ms))
                    {
                        newFrame = new Bitmap(bmp);
                    }

                    Bitmap oldFrame = null;
                    lock (state.SyncRoot)
                    {
                        oldFrame = state.Frame;
                        state.Frame = newFrame;
                        state.LastCaptureTick = Environment.TickCount;
                    }

                    newFrame = null;
                    if (oldFrame != null)
                        oldFrame.Dispose();
                }
                catch
                {
                }
                finally
                {
                    if (newFrame != null)
                        newFrame.Dispose();

                    Interlocked.Exchange(ref state.Capturing, 0);
                }
            });
        }

        private static void DisposeState(BrowserCaptureState state)
        {
            if (state == null)
                return;

            lock (state.SyncRoot)
            {
                if (state.Frame != null)
                {
                    state.Frame.Dispose();
                    state.Frame = null;
                }
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
