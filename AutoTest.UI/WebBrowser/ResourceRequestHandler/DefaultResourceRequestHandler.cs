using AutoTest.Domain.Entity;
using AutoTest.UI.WebBrowser.EventListener;
using AutoTest.UI.WebBrowser.ResponseFilters;
using AutoTest.UI.WebBrowser;
using AutoTest.Util;
using CefSharp;
using CefSharp.Handler;
using LJC.FrameWorkV3.Comm;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoTest.UI.WebBrowser.ResourceHandler;

namespace AutoTest.UI.WebBrowser.ResourceRequestHandler
{
    /// <summary>
    /// 默认的请求处理器
    /// </summary>
    public class DefaultResourceRequestHandler :CefSharp.Handler.ResourceRequestHandler
    {
        protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            //if (request.Url.Equals("https://sofire.bdstatic.com/js/dfxaf3-635b4cd6.js", StringComparison.OrdinalIgnoreCase)
            //    || request.Url.Equals("https://x0.ifengimg.com/fe/lark/lark_comp/chip-294534ed97.js")
            //    || request.Url.IndexOf("https://ax.ifeng.com/showcode", StringComparison.OrdinalIgnoreCase) > -1)
            //{
            //    return new TransferRequestHandler(Encoding.UTF8.GetBytes("function _0x982f(a,w){}"));
            //}

            if(chromiumWebBrowser is DefaultChromiumWebBrowser)
            {
                var currTask = (chromiumWebBrowser as DefaultChromiumWebBrowser).GetCurrentTask();
                if (currTask != null && currTask.GetTestCase() != null)
                {
                    var configs = (List<RequestInterceptConfig>)LocalCacheManager<object>.Find("RequestInterceptConfig_" + currTask.GetTestCase().Id,
                        () => BigEntityTableRemotingEngine.Find<RequestInterceptConfig>(nameof(RequestInterceptConfig),
                    nameof(RequestInterceptConfig.TestCaseId), new object[] { currTask.GetTestCase().Id }).ToList(), 1);

                    foreach(var c in configs.Where(p=>p.Enabled))
                    {
                        byte[] data = null;
                        var mimeType = c.MimeType;
                        if (mimeType == Mime.Default.MimeName)
                        {
                            var ext = Path.GetExtension(request.Url).ToLower();
                            switch (ext)
                            {
                                case "png":
                                case "jpg":
                                case "gif":
                                    {
                                        mimeType = "image/" + ext;
                                        break;
                                    }
                                case "js":
                                    {
                                        mimeType = Mime.JsMime;
                                        break;
                                    }
                                case "json":
                                    {
                                        mimeType = Mime.JsonMime;
                                        break;
                                    }
                                case "html":
                                case "htm":
                                    {
                                        mimeType = Mime.HtmlMime;
                                        break;
                                    }
                            }
                        }
                        if (c.ResponseData != null)
                        {
                            data = c.ResponseData;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(c.Response))
                            {
                                var mime = Resources.MimeResource.Find(mimeType);
                                if (mime?.DefaultBytes != null)
                                {
                                    data = mime.DefaultBytes;
                                }
                                else if (!string.IsNullOrWhiteSpace(mime?.DetaultText))
                                {
                                    data = Encoding.UTF8.GetBytes(mime.DetaultText);
                                }
                                else
                                {
                                    data = new byte[0];
                                }
                            }
                            else
                            {
                                data = Encoding.UTF8.GetBytes(c.Response);
                            }
                        }
                        switch (c.MatchType)
                        {
                            case 0:
                                {
                                    if (request.Url.Equals(c.MatchUrl, StringComparison.OrdinalIgnoreCase))
                                    {
                                        return new TransferResourceHandler(mimeType, data);
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    if (request.Url.IndexOf(c.MatchUrl, StringComparison.OrdinalIgnoreCase)>-1)
                                    {
                                        return new TransferResourceHandler(mimeType, data);
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    try
                                    {
                                        if (Regex.IsMatch(request.Url, c.MatchUrl, RegexOptions.IgnoreCase))
                                        {
                                            return new TransferResourceHandler(mimeType, data);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.Data.Add("url", request.Url);
                                        ex.Data.Add("matchurl", c.MatchUrl);
                                        LJC.FrameWorkV3.LogManager.LogHelper.Instance.Error("正则截取请求地址错误", ex);
                                    }
                                    break;
                                }
                        }
                    }

                }
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
