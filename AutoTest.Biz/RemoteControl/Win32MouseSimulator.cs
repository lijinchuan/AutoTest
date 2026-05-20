using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AutoTest.Biz.RemoteControl
{
    /// <summary>
    /// 通过Win32 API模拟系统真实鼠标操作
    /// </summary>
    public static class Win32MouseSimulator
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll")]
        private static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int MK_LBUTTON = 0x0001;

        public static void MoveTo(int x, int y)
        {
            SetCursorPos(x, y);
        }

        public static void LeftDown(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, IntPtr.Zero);
        }

        public static void LeftUp(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, IntPtr.Zero);
        }

        public static void MoveToOnWindow(IntPtr hwnd, int screenX, int screenY)
        {
            if (!TryGetTargetWindow(hwnd, screenX, screenY, out var targetHwnd, out var clientPt))
            {
                MoveTo(screenX, screenY);
                return;
            }

            PostMessage(targetHwnd, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(clientPt.X, clientPt.Y));
        }

        public static void LeftDownOnWindow(IntPtr hwnd, int screenX, int screenY)
        {
            if (!TryGetTargetWindow(hwnd, screenX, screenY, out var targetHwnd, out var clientPt))
            {
                LeftDown(screenX, screenY);
                return;
            }

            var lParam = MakeLParam(clientPt.X, clientPt.Y);
            PostMessage(targetHwnd, WM_MOUSEMOVE, IntPtr.Zero, lParam);
            PostMessage(targetHwnd, WM_LBUTTONDOWN, new IntPtr(MK_LBUTTON), lParam);
        }

        public static void LeftUpOnWindow(IntPtr hwnd, int screenX, int screenY)
        {
            if (!TryGetTargetWindow(hwnd, screenX, screenY, out var targetHwnd, out var clientPt))
            {
                LeftUp(screenX, screenY);
                return;
            }

            var lParam = MakeLParam(clientPt.X, clientPt.Y);
            PostMessage(targetHwnd, WM_MOUSEMOVE, IntPtr.Zero, lParam);
            PostMessage(targetHwnd, WM_LBUTTONUP, IntPtr.Zero, lParam);
        }

        /// <summary>
        /// 将图像归一化坐标 (0~1) 映射到屏幕实际坐标
        /// </summary>
        public static Point NormalizedToScreen(double nx, double ny, Rectangle region)
        {
            int x = region.X + (int)Math.Round(nx * region.Width);
            int y = region.Y + (int)Math.Round(ny * region.Height);
            return new Point(
                Math.Max(region.X, Math.Min(region.Right - 1, x)),
                Math.Max(region.Y, Math.Min(region.Bottom - 1, y)));
        }

        private static bool TryGetTargetWindow(IntPtr hwnd, int screenX, int screenY, out IntPtr targetHwnd, out POINT clientPt)
        {
            targetHwnd = IntPtr.Zero;
            clientPt = new POINT { X = screenX, Y = screenY };
            if (hwnd == IntPtr.Zero)
                return false;

            var point = new POINT { X = screenX, Y = screenY };
            var child = WindowFromPoint(point);
            if (child == IntPtr.Zero || (child != hwnd && !IsChild(hwnd, child)))
                child = hwnd;

            var cp = new POINT { X = screenX, Y = screenY };
            if (!ScreenToClient(child, ref cp))
                return false;

            targetHwnd = child;
            clientPt = cp;
            return true;
        }

        private static IntPtr MakeLParam(int x, int y)
        {
            return new IntPtr((y << 16) | (x & 0xFFFF));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
