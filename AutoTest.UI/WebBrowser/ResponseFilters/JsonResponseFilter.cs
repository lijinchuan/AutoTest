using AutoTest.UI.WebBrowser.EventListener;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser.ResponseFilters
{
    /// <summary>
    /// json过滤器
    /// </summary>
    public class JsonResponseFilter : BaseResponseFilter
    {
        public override string GetContent(WebEvent webEvent)
        {
            return Encoding.GetEncoding(webEvent.ResponseChartSet).GetString(webEvent.Data);
        }
    }
}
