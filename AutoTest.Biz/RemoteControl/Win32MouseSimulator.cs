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

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP   = 0x0004;

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

        /// <summary>
        /// 将图像归一化坐标 (0~1) 映射到屏幕实际坐标
        /// </summary>
        public static Point NormalizedToScreen(double nx, double ny, Rectangle region)
        {
            int x = region.X + (int)Math.Round(nx * region.Width);
            int y = region.Y + (int)Math.Round(ny * region.Height);
            return new Point(
                Math.Max(region.X, Math.Min(region.Right  - 1, x)),
                Math.Max(region.Y, Math.Min(region.Bottom - 1, y)));
        }
    }
}
