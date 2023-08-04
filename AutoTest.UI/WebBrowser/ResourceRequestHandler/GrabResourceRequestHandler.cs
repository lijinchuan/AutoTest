using AutoTest.UI.WebBrowser.EventListener;
using AutoTest.UI.WebBrowser.ResourceHandler;
using AutoTest.UI.WebBrowser.ResponseFilters;
using AutoTest.Util;
using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser.ResourceRequestHandler
{
    public class GrabResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            if (request.Url.Contains(DownLoadResourceHandler.UrlFlag))
            {
                return new DownLoadResourceHandler(request);
            }

            return base.GetResourceHandler(chromiumWebBrowser, browser, frame, request);
        }

        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            var guid = StringUtil.GetMD5(request.Url + request.Identifier);
            //编码
            return ResponseFilterFactory.CreateResponseFilter(guid, response.MimeType, mime =>
             {
                 if (mime.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
                 || mime.Equals("application/javascript", StringComparison.OrdinalIgnoreCase))
                 {
                     return new TextResponseFilter();
                 }

                 return null;
             });
        }

        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            var guid = StringUtil.GetMD5(request.Url + request.Identifier);
            if (guid != null)
            {
                var filter = ResponseFilterFactory.GetResponseFilter(guid);
                if (filter != null)
                {
                    var buffer = new byte[512];
                    var stream = filter.GetStream();
                    byte[] data = null;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int readlen;
                        while ((readlen = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, readlen);
                        }
                        data = ms.ToArray();
                    }

                    if (chromiumWebBrowser is GrabChromiumWebBrowser)
                    {
                        //包装事件给各个事件处理器处理
                        var webevent = new WebEvent
                        {
                            Data = data,
                            SourceUrl = request.Url,
                            DataType = response.MimeType,
                            ResponseChartSet = string.IsNullOrWhiteSpace(response.Charset) ? "utf-8" : response.Charset,
                            StatusCode = response.StatusCode,
                            Browser = browser,
                            Frame = frame
                        };

                        webevent.Content = filter.GetContent(webevent);

                        foreach (var listener in (chromiumWebBrowser as GrabChromiumWebBrowser).GetEventListeners())
                        {
                            if (listener.Process(webevent))
                            {
                                break;
                            }
                        }
                    }

                }
            }
        }
    }
}
