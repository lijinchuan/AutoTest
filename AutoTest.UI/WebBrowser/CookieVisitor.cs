using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser
{
    /// <summary>
    /// cookie访问者模式
    /// </summary>
    public class CookieVisitor : CefSharp.ICookieVisitor
    {
        public event Action<CefSharp.Cookie> SendCookie;

        public void Dispose()
        {
            // Method intentionally left empty.
        }

        public bool Visit(CefSharp.Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            deleteCookie = false;
            SendCookie?.Invoke(cookie);

            return true;
        }
    }
}
