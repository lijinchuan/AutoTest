﻿using AutoTest.UI.SubForm;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.WebBrowser
{
    public class JsDialogHandler : IJsDialogHandler
    {
        public string LastAlertMsg
        {
            get;
            private set;
        }

        public string LastConfirmMsg
        {
            get;
            private set;
        }

        public void Clear()
        {
            LastAlertMsg = null;
            LastConfirmMsg = null;
        }

        protected virtual void DealAlert(string originUrl,string messageText)
        {
            new AlertDlg(originUrl, messageText, null).ShowDialog();
        }

        protected virtual DialogResult DealComfirm(string messageText)
        {
            var dr = MessageBox.Show(messageText, "提示", MessageBoxButtons.YesNo);

            return dr;
        }

        public bool OnJSDialog(IWebBrowser browserControl, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
        {
            switch (dialogType)
            {
                case CefSharp.CefJsDialogType.Alert:
                    {
                        LastAlertMsg = messageText;
                        //MessageBox.Show(messageText, "提示");
                        DealAlert(originUrl, messageText);

                        callback.Continue(true, string.Empty);
                        suppressMessage = false;
                        return false;
                    }
                case CefSharp.CefJsDialogType.Confirm:
                    LastConfirmMsg = messageText;
                    var dr = DealComfirm(messageText);
                    if (dr == DialogResult.Yes)
                    {
                        callback.Continue(true, string.Empty);
                        suppressMessage = false;
                        return true;
                    }
                    else
                    {
                        callback.Continue(false, string.Empty);
                        suppressMessage = false;
                        return true;
                    }
                case CefSharp.CefJsDialogType.Prompt:
                    MessageBox.Show("系统不支持prompt形式的提示框", "提示");
                    break;
                default:
                    break;
            }
            return false;
        }

        public bool OnJSBeforeUnload(IWebBrowser browserControl, IBrowser browser, string message, bool isReload, IJsDialogCallback callback)
        {
            return true;
        }

        public void OnResetDialogState(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public void OnDialogClosed(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback)
        {
            return false;
        }
    }
}
