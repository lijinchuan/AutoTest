using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser
{
    /// <summary>
    /// cookie访问者模式
    /// </summary>
    public class CookieVisitor : ICookieVisitor
    {
        private readonly ICookieManager cookieManager;

        public CookieVisitor(ICookieManager cookieManager)
        {
            this.cookieManager = cookieManager;
        }

        public void Dispose()
        {
            // Method intentionally left empty.
        }

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            deleteCookie = false;

            return true;
        }

        public async Task<List<Cookie>> GetCookies(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return await cookieManager.VisitAllCookiesAsync();
            }
            return await cookieManager.VisitUrlCookiesAsync(url, true);
        }
    }
}
