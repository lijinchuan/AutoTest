using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser
{
    public class GrabChromiumWebBrowser: ChromiumWebBrowser
    {

        public GrabChromiumWebBrowser(string name, string address)
            : base(address)
        {
            Name = name;
            var setting = new RequestContextSettings()
            {
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CefSharp\\Cache_" + name),
                PersistSessionCookies = true,
                PersistUserPreferences = true,
            };
            var context = new RequestContext(setting);
            RequestContext = context;

           
            LifeSpanHandler = new DefLifeSpanHandler();
        }
    }
}
