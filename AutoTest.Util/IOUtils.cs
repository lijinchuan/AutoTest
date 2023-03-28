using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.Util
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public int mouseData;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public int type;
        public InputUnion u;
        public static int Size => Marshal.SizeOf(typeof(INPUT));
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
        [FieldOffset(0)]
        public KEYBDINPUT ki;
        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }

    public static class IOUtils
    {
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        //SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        const int MOUSEEVENTF_LEFTDOWN = 0x0002;//表示按下鼠标左键。

        const int MOUSEEVENTF_LEFTUP = 0x0004;//表示释放鼠标左键。

        const int INPUT_MOUSE = 0;//表示输入事件类型为鼠标事件。

        // 定义常量值
        public const int INPUT_KEYBOARD = 1;
        public const int KEYEVENTF_UNICODE = 0x0004;
        public const int KEYEVENTF_KEYDOWN = 0x0000;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int MAPVK_VK_TO_VSC = 0;

        // 发送鼠标点击事件
        public static void SendMouseClick(int x, int y)
        {
            // 定义一个MOUSEINPUT结构体，用于描述鼠标点击事件
            MOUSEINPUT mouseInput = new MOUSEINPUT();
            mouseInput.dx = x;
            mouseInput.dy = y;
            mouseInput.mouseData = 0;
            mouseInput.dwFlags = MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP;
            mouseInput.time = 0;
            mouseInput.dwExtraInfo = IntPtr.Zero;

            // 定义一个INPUT结构体，将MOUSEINPUT结构体作为其联合体成员
            INPUT input = new INPUT();
            input.type = INPUT_MOUSE;
            input.u.mi = mouseInput;

            // 调用SendInput函数，发送鼠标点击事件
            SendInput(1, new INPUT[] { input }, Marshal.SizeOf(input));
        }

        // 发送Unicode字符串
        public static void SendUnicodeString(string str)
        {
            IntPtr ptr = Marshal.StringToHGlobalUni(str);

            for (int i = 0; i < str.Length; i++)
            {
                ushort c = (ushort)Marshal.ReadInt16(ptr, i * 2);

                INPUT down = new INPUT();
                down.type = INPUT_KEYBOARD;
                down.u.ki = new KEYBDINPUT();
                down.u.ki.wVk = 0;
                down.u.ki.wScan = c;
                down.u.ki.dwFlags = KEYEVENTF_UNICODE;
                down.u.ki.time = 1;
                down.u.ki.dwExtraInfo = IntPtr.Zero;

                INPUT up = new INPUT();
                up.type = INPUT_KEYBOARD;
                up.u.ki = new KEYBDINPUT();
                up.u.ki.wVk = 0;
                up.u.ki.wScan = c;
                up.u.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;
                up.u.ki.time = 1;
                up.u.ki.dwExtraInfo = IntPtr.Zero;

                INPUT[] inputs = new INPUT[] { down, up };
                SendInput(2, inputs, INPUT.Size);
            }

            Marshal.FreeHGlobal(ptr);
        }

        // 发送keypress事件
        public static void SendKeyPress(ushort keyCode)
        {
            INPUT down = new INPUT();
            down.type = INPUT_KEYBOARD;
            down.u.ki = new KEYBDINPUT();
            down.u.ki.wVk = 0;
            down.u.ki.wScan = keyCode;
            down.u.ki.dwFlags = 0;
            down.u.ki.time = 0;
            down.u.ki.dwExtraInfo = IntPtr.Zero;

            INPUT up = new INPUT();
            up.type = INPUT_KEYBOARD;
            up.u.ki = new KEYBDINPUT();
            up.u.ki.wVk = 0;
            up.u.ki.wScan = keyCode;
            up.u.ki.dwFlags = KEYEVENTF_KEYUP;
            up.u.ki.time = 0;
            up.u.ki.dwExtraInfo = IntPtr.Zero;

            INPUT[] inputs = new INPUT[] { down, up };
            SendInput(2, inputs, INPUT.Size);
        }
    }
}
