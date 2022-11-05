﻿using AutoTest.UI.EventListener;
using AutoTest.UI.ResponseFilters;
using AutoTest.UI.WebBrowser;
using AutoTest.Util;
using CefSharp;
using CefSharp.Handler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.ResourceHandler
{
    /// <summary>
    /// 默认的请求处理器
    /// </summary>
    public class DefaultResourceHandler : ResourceRequestHandler
    {
        protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            if (request.Url.Equals("https://sofire.bdstatic.com/js/dfxaf3-635b4cd6.js", StringComparison.OrdinalIgnoreCase)
                || request.Url.Equals("https://x0.ifengimg.com/fe/lark/lark_comp/chip-294534ed97.js")
                || request.Url.IndexOf("https://ax.ifeng.com/showcode", StringComparison.OrdinalIgnoreCase) > -1)
            {
                return new TransferRequestHandler(Encoding.UTF8.GetBytes("function _0x982f(a,w){}"));
            }
            return base.GetResourceHandler(chromiumWebBrowser, browser, frame, request);
        }

        protected sealed override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            var guid = StringUtil.GetMD5(request.Url);
            //编码
            return ResponseFilterFactory.CreateResponseFilter(guid, response.MimeType);
        }

        protected sealed override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            var guid = StringUtil.GetMD5(request.Url);
            if (guid != null)
            {
                var filter = ResponseFilterFactory.GetResponseFilter(guid);
                if (filter != null)
                {
                    var buffer = new byte[512];
                    var stream = filter.GetStream();
                    byte[] data = null;
                    using (System.IO.MemoryStream ms = new MemoryStream())
                    {
                        int readlen;
                        while ((readlen = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, readlen);
                        }
                        data = ms.ToArray();
                    }

                    if (chromiumWebBrowser is DefaultChromiumWebBrowser)
                    {
                        //包装事件给各个事件处理器处理
                        var webevent = new WebEvent
                        {
                            Data = data,
                            SourceUrl = request.Url,
                            DataType = response.MimeType,
                            ResponseChartSet =string.IsNullOrWhiteSpace(response.Charset)?"utf-8": response.Charset,
                            StatusCode = response.StatusCode,
                            Browser=browser,
                            Frame=frame
                        };

                        webevent.Content = filter.GetContent(webevent);

                        foreach (var listener in (chromiumWebBrowser as DefaultChromiumWebBrowser).GetEventListeners())
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
