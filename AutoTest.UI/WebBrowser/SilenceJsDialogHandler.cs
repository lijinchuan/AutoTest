using AutoTest.UI.SubForm;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.WebBrowser
{
    public class SilenceJsDialogHandler : JsDialogHandler
    {
        protected override void DealAlert(string originUrl, string messageText)
        {
        }
    }
}
