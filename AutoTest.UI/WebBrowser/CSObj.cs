﻿using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.WebBrowser
{
    public class CSObj
    {
        private System.Threading.SynchronizationContext context;
        private ChromiumWebBrowser browser;
        public CSObj(System.Threading.SynchronizationContext context, ChromiumWebBrowser browser)
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
    }
}
