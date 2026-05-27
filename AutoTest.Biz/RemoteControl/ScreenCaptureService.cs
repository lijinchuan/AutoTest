using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace AutoTest.Biz.RemoteControl
{
    /// <summary>
    /// 当前进程主窗口截图服务（单例）
    /// </summary>
    public sealed class ScreenCaptureService
    {
        private static readonly ScreenCaptureService _instance = new ScreenCaptureService();
        public static ScreenCaptureService Instance => _instance;

        private Rectangle? _windowRelativeRegion;

        private ScreenCaptureService() { }

        public sealed class CaptureFrameMetrics
        {
            public long CaptureMs { get; set; }
            public long OverlayMs { get; set; }
            public long EncodeMs { get; set; }
            public long TotalMs { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int ByteSize { get; set; }
            public DateTime CapturedAtUtc { get; set; }
        }

        public sealed class CaptureFrameResult
        {
            public byte[] Bytes { get; set; }
            public CaptureFrameMetrics Metrics { get; set; }
        }

        /// <summary>
        /// 当前捕获区域（屏幕坐标）
        /// </summary>
        public Rectangle CaptureRegion { get; private set; } = Rectangle.Empty;

        /// <summary>
        /// 当前捕获使用的主窗口句柄
        /// </summary>
        public IntPtr CurrentWindowHandle { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// JPEG压缩质量 1-100
        /// </summary>
        public long JpegQuality { get; set; } = 70;

        /// <summary>
        /// CEF截图覆盖层提供器（参数为屏幕坐标捕获区域）
        /// </summary>
        public Func<Rectangle, Bitmap> CefOverlayCaptureProvider { get; set; }

        /// <summary>
        /// 切换到后一个Tab命令（由UI层注入）
        /// </summary>
        public Func<bool> SwitchToNextTabCommand { get; set; }

        /// <summary>
        /// 设定截图区域（相对主窗口坐标）
        /// </summary>
        public void SetRegion(int x, int y, int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("宽度和高度必须大于0");
            _windowRelativeRegion = new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// 截取主窗口并返回JPEG字节
        /// </summary>
        public byte[] CaptureJpegBytes()
        {
            return CaptureJpegFrame().Bytes;
        }

        public bool SwitchToNextTab()
        {
            var cmd = SwitchToNextTabCommand;
            if (cmd == null)
            {
                return false;
            }

            try
            {
                return cmd();
            }
            catch
            {
                return false;
            }
        }

        public CaptureFrameResult CaptureJpegFrame()
        {
            var totalSw = Stopwatch.StartNew();
            var captureSw = Stopwatch.StartNew();
            var hwnd = CurrentWindowHandle;
            if (hwnd == IntPtr.Zero || !IsWindow(hwnd))
            {
                hwnd = Process.GetCurrentProcess().MainWindowHandle;
                if (hwnd == IntPtr.Zero)
                    throw new InvalidOperationException("未找到主窗口句柄");
                CurrentWindowHandle = hwnd;
            }

            RECT windowRect;
            if (!GetWindowRect(hwnd, out windowRect))
                throw new InvalidOperationException("获取主窗口坐标失败");

            var fullRect = new Rectangle(windowRect.Left, windowRect.Top, Math.Max(1, windowRect.Right - windowRect.Left), Math.Max(1, windowRect.Bottom - windowRect.Top));
            var localRect = _windowRelativeRegion ?? new Rectangle(0, 0, fullRect.Width, fullRect.Height);
            var localClipped = Rectangle.Intersect(new Rectangle(0, 0, fullRect.Width, fullRect.Height), localRect);
            if (localClipped.Width <= 0 || localClipped.Height <= 0)
                throw new InvalidOperationException("截图区域超出主窗口范围");

            CaptureRegion = new Rectangle(fullRect.Left + localClipped.X, fullRect.Top + localClipped.Y, localClipped.Width, localClipped.Height);

            using (var fullBmp = new Bitmap(fullRect.Width, fullRect.Height, PixelFormat.Format32bppArgb))
            {
                var captured = TryCaptureWindow(hwnd, fullBmp);
                if (!captured)
                    throw new InvalidOperationException("抓取主窗口失败");
                captureSw.Stop();

                using (var cropBmp = fullBmp.Clone(localClipped, PixelFormat.Format32bppArgb))
                {
                    var overlaySw = Stopwatch.StartNew();
                    ComposeCefOverlay(cropBmp, CaptureRegion);
                    overlaySw.Stop();

                    var encodeSw = Stopwatch.StartNew();
                    var encoder = GetJpegEncoder();
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, JpegQuality);

                    using (var ms = new MemoryStream())
                    {
                        cropBmp.Save(ms, encoder, encoderParams);
                        encodeSw.Stop();
                        totalSw.Stop();

                        var bytes = ms.ToArray();
                        var metrics = new CaptureFrameMetrics
                        {
                            CaptureMs = captureSw.ElapsedMilliseconds,
                            OverlayMs = overlaySw.ElapsedMilliseconds,
                            EncodeMs = encodeSw.ElapsedMilliseconds,
                            TotalMs = totalSw.ElapsedMilliseconds,
                            Width = cropBmp.Width,
                            Height = cropBmp.Height,
                            ByteSize = bytes.Length,
                            CapturedAtUtc = DateTime.UtcNow
                        };

                        return new CaptureFrameResult
                        {
                            Bytes = bytes,
                            Metrics = metrics
                        };
                    }
                }
            }
        }

        private void ComposeCefOverlay(Bitmap baseBmp, Rectangle captureRegion)
        {
            var provider = CefOverlayCaptureProvider;
            if (provider == null)
                return;

            try
            {
                using (var overlay = provider(captureRegion))
                {
                    if (overlay == null)
                        return;

                    using (var g = Graphics.FromImage(baseBmp))
                    {
                        g.DrawImage(overlay, 0, 0, baseBmp.Width, baseBmp.Height);
                    }
                }
            }
            catch
            {
            }
        }

        private static ImageCodecInfo GetJpegEncoder()
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.MimeType == "image/jpeg")
                    return codec;
            }
            throw new InvalidOperationException("未找到JPEG编码器");
        }

        private const uint PW_RENDERFULLCONTENT = 0x00000002;
        private const int SRCCOPY = 0x00CC0020;

        private static bool TryCaptureWindow(IntPtr hwnd, Bitmap target)
        {
            if (TryPrintWindow(hwnd, target, PW_RENDERFULLCONTENT) && !IsNearlyBlack(target))
                return true;

            if (TryPrintWindow(hwnd, target, 0) && !IsNearlyBlack(target))
                return true;

            if (TryCopyFromScreen(hwnd, target) && !IsNearlyBlack(target))
                return true;

            return TryBitBltWindow(hwnd, target);
        }

        private static bool TryPrintWindow(IntPtr hwnd, Bitmap target, uint flags)
        {
            using (var g = Graphics.FromImage(target))
            {
                var hdc = g.GetHdc();
                try
                {
                    return PrintWindow(hwnd, hdc, flags);
                }
                finally
                {
                    g.ReleaseHdc(hdc);
                }
            }
        }

        private static bool TryCopyFromScreen(IntPtr hwnd, Bitmap target)
        {
            try
            {
                RECT rect;
                if (!GetWindowRect(hwnd, out rect))
                    return false;

                using (var g = Graphics.FromImage(target))
                {
                    g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(target.Width, target.Height), CopyPixelOperation.SourceCopy);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool TryBitBltWindow(IntPtr hwnd, Bitmap target)
        {
            var src = GetWindowDC(hwnd);
            if (src == IntPtr.Zero)
                return false;

            using (var g = Graphics.FromImage(target))
            {
                var dst = g.GetHdc();
                try
                {
                    return BitBlt(dst, 0, 0, target.Width, target.Height, src, 0, 0, SRCCOPY);
                }
                finally
                {
                    g.ReleaseHdc(dst);
                    ReleaseDC(hwnd, src);
                }
            }
        }

        private static bool IsNearlyBlack(Bitmap bmp)
        {
            int sampleX = Math.Max(1, bmp.Width / 8);
            int sampleY = Math.Max(1, bmp.Height / 8);
            int total = 0;
            int black = 0;

            for (int y = 0; y < bmp.Height; y += sampleY)
            {
                for (int x = 0; x < bmp.Width; x += sampleX)
                {
                    total++;
                    var c = bmp.GetPixel(x, y);
                    if (c.R < 12 && c.G < 12 && c.B < 12)
                        black++;
                }
            }

            return total > 0 && (black * 100 / total) >= 95;
        }

        private static void ComposeOwnedPopupOverlays(IntPtr rootHwnd, Bitmap baseBmp, Rectangle rootRect)
        {
            try
            {
                uint rootProcessId;
                GetWindowThreadProcessId(rootHwnd, out rootProcessId);

                using (var g = Graphics.FromImage(baseBmp))
                {
                    EnumWindows((candidate, lParam) =>
                    {
                        if (candidate == IntPtr.Zero || candidate == rootHwnd)
                            return true;
                        if (!IsWindowVisible(candidate))
                            return true;
                        if (!IsPopupCandidate(candidate, rootHwnd, rootProcessId))
                            return true;

                        RECT popupRectRaw;
                        if (!GetWindowRect(candidate, out popupRectRaw))
                            return true;

                        var popupRect = Rectangle.FromLTRB(popupRectRaw.Left, popupRectRaw.Top, popupRectRaw.Right, popupRectRaw.Bottom);
                        if (popupRect.Width <= 0 || popupRect.Height <= 0)
                            return true;

                        var overlap = Rectangle.Intersect(rootRect, popupRect);
                        if (overlap.Width <= 0 || overlap.Height <= 0)
                            return true;

                        var dstRect = new Rectangle(overlap.Left - rootRect.Left, overlap.Top - rootRect.Top, overlap.Width, overlap.Height);
                        var srcRect = new Rectangle(overlap.Left - popupRect.Left, overlap.Top - popupRect.Top, overlap.Width, overlap.Height);

                        using (var popupBmp = new Bitmap(popupRect.Width, popupRect.Height, PixelFormat.Format32bppArgb))
                        {
                            if (TryCaptureWindow(candidate, popupBmp))
                            {
                                g.DrawImage(popupBmp, dstRect, srcRect, GraphicsUnit.Pixel);
                            }
                            else
                            {
                                g.CopyFromScreen(overlap.Left, overlap.Top, dstRect.Left, dstRect.Top, overlap.Size, CopyPixelOperation.SourceCopy);
                            }
                        }

                        return true;
                    }, IntPtr.Zero);
                }
            }
            catch
            {
            }
        }

        private static bool IsPopupCandidate(IntPtr candidate, IntPtr rootHwnd, uint rootProcessId)
        {
            uint candidateProcessId;
            GetWindowThreadProcessId(candidate, out candidateProcessId);
            if (candidateProcessId != rootProcessId)
                return false;

            if (IsOwnedBy(candidate, rootHwnd))
                return true;

            if (IsMenuWindow(candidate))
                return true;

            return HasPopupStyle(candidate);
        }

        private static bool HasPopupStyle(IntPtr hwnd)
        {
            var style = GetWindowStyle(hwnd);
            return (style & WS_POPUP) != 0 && (style & WS_CHILD) == 0;
        }

        private static long GetWindowStyle(IntPtr hwnd)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hwnd, GWL_STYLE).ToInt64();

            return GetWindowLong32(hwnd, GWL_STYLE);
        }

        private static bool IsMenuWindow(IntPtr hwnd)
        {
            var sb = new System.Text.StringBuilder(64);
            var len = GetClassName(hwnd, sb, sb.Capacity);
            if (len <= 0)
                return false;

            return string.Equals(sb.ToString(), "#32768", StringComparison.Ordinal);
        }

        private static bool IsOwnedBy(IntPtr hwnd, IntPtr rootHwnd)
        {
            var owner = GetWindow(hwnd, GW_OWNER);
            while (owner != IntPtr.Zero)
            {
                if (owner == rootHwnd)
                    return true;
                owner = GetWindow(owner, GW_OWNER);
            }

            return false;
        }

        private const uint GW_OWNER = 4;
        private const int GWL_STYLE = -16;
        private const long WS_CHILD = 0x40000000L;
        private const long WS_POPUP = unchecked((int)0x80000000);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
