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
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll")]
        private static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr ChildWindowFromPointEx(IntPtr hWndParent, POINT pt, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int MK_LBUTTON = 0x0001;
        private const int MK_RBUTTON = 0x0002;

        private const uint CWP_SKIPINVISIBLE = 0x0001;
        private const uint CWP_SKIPDISABLED = 0x0002;

        private static volatile bool _leftDown;
        private static volatile bool _rightDown;

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

        public static void RightDown(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_RIGHTDOWN, x, y, 0, IntPtr.Zero);
        }

        public static void RightUp(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_RIGHTUP, x, y, 0, IntPtr.Zero);
        }

        public static void MoveToOnWindow(IntPtr hwnd, int screenX, int screenY)
        {
            if (HasVisibleSystemMenuWindow())
            {
                MoveTo(screenX, screenY);
                return;
            }

            if (TryGetTargetWindow(hwnd, screenX, screenY, out var targetHwnd, out var clientPt))
            {
                var mk = (_leftDown ? MK_LBUTTON : 0) | (_rightDown ? MK_RBUTTON : 0);
                PostMessage(targetHwnd, WM_MOUSEMOVE, new IntPtr(mk), MakeLParam(clientPt.X, clientPt.Y));
                return;
            }

            MoveTo(screenX, screenY);
        }

        public static void LeftDownOnWindow(IntPtr hwnd, int screenX, int screenY)
        {
            _rightDown = false;
            _leftDown = true;
            if (hwnd != IntPtr.Zero)
            {
                SetForegroundWindow(hwnd);
            }

            if (HasVisibleSystemMenuWindow())
            {
                LeftDown(screenX, screenY);
                return;
            }

            if (!TryGetTargetWindow(hwnd, screenX, screenY, out var targetHwnd, out var clientPt))
            {
                LeftDown(screenX, screenY);
                return;
            }

            var lParam = MakeLParam(clientPt.X, clientPt.Y);
            SendMessage(targetHwnd, WM_MOUSEMOVE, new IntPtr(MK_LBUTTON), lParam);
            SendMessage(targetHwnd, WM_LBUTTONDOWN, new IntPtr(MK_LBUTTON), lParam);
        }

        public static void LeftUpOnWindow(IntPtr hwnd, int screenX, int screenY)
        {
            _leftDown = false;
            if (HasVisibleSystemMenuWindow())
            {
                LeftUp(screenX, screenY);
                return;
            }

            if (!TryGetTargetWindow(hwnd, screenX, screenY, out var targetHwnd, out var clientPt))
            {
                LeftUp(screenX, screenY);
                return;
            }

            var lParam = MakeLParam(clientPt.X, clientPt.Y);
            SendMessage(targetHwnd, WM_MOUSEMOVE, new IntPtr(MK_LBUTTON), lParam);
            SendMessage(targetHwnd, WM_LBUTTONUP, IntPtr.Zero, lParam);
        }

        public static void RightDownOnWindow(IntPtr hwnd, int screenX, int screenY)
        {
            _leftDown = false;
            _rightDown = true;
            if (hwnd != IntPtr.Zero)
            {
                SetForegroundWindow(hwnd);
            }

            if (!TryGetTargetWindow(hwnd, screenX, screenY, out var targetHwnd, out var clientPt))
            {
                RightDown(screenX, screenY);
                return;
            }

            var lParam = MakeLParam(clientPt.X, clientPt.Y);
            SendMessage(targetHwnd, WM_MOUSEMOVE, new IntPtr(MK_RBUTTON), lParam);
            SendMessage(targetHwnd, WM_RBUTTONDOWN, new IntPtr(MK_RBUTTON), lParam);
        }

        public static void RightUpOnWindow(IntPtr hwnd, int screenX, int screenY)
        {
            _rightDown = false;
            if (!TryGetTargetWindow(hwnd, screenX, screenY, out var targetHwnd, out var clientPt))
            {
                RightUp(screenX, screenY);
                return;
            }

            var lParam = MakeLParam(clientPt.X, clientPt.Y);
            SendMessage(targetHwnd, WM_MOUSEMOVE, new IntPtr(MK_RBUTTON), lParam);
            SendMessage(targetHwnd, WM_RBUTTONUP, IntPtr.Zero, lParam);
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

            var child = FindDeepestChildWindow(hwnd, screenX, screenY);
            if (child == IntPtr.Zero)
            {
                var point = new POINT { X = screenX, Y = screenY };
                child = WindowFromPoint(point);
                if (child == IntPtr.Zero || (child != hwnd && !IsChild(hwnd, child)))
                    child = hwnd;
            }

            var cp = new POINT { X = screenX, Y = screenY };
            if (!ScreenToClient(child, ref cp))
                return false;

            targetHwnd = child;
            clientPt = cp;
            return true;
        }

        private static IntPtr FindDeepestChildWindow(IntPtr rootHwnd, int screenX, int screenY)
        {
            var current = rootHwnd;
            while (current != IntPtr.Zero)
            {
                var pt = new POINT { X = screenX, Y = screenY };
                if (!ScreenToClient(current, ref pt))
                    break;

                var next = ChildWindowFromPointEx(current, pt, CWP_SKIPINVISIBLE | CWP_SKIPDISABLED);
                if (next == IntPtr.Zero || next == current)
                    break;

                current = next;
            }

            return current;
        }

        private static bool HasVisibleSystemMenuWindow()
        {
            var found = false;
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                var sb = new System.Text.StringBuilder(64);
                if (GetClassName(hWnd, sb, sb.Capacity) <= 0)
                    return true;

                if (string.Equals(sb.ToString(), "#32768", StringComparison.Ordinal))
                {
                    found = true;
                    return false;
                }

                return true;
            }, IntPtr.Zero);

            return found;
        }

        private static IntPtr MakeLParam(int x, int y)
        {
            return new IntPtr((y << 16) | (x & 0xFFFF));
        }

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
