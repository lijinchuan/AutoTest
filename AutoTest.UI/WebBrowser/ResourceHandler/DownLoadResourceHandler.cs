using CefSharp;
using CefSharp.Callback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser.ResourceHandler
{
    public class DownLoadResourceHandler : IResourceHandler
    {
        public const string UrlFlag = "_ask_back_download_";
        private const string CrosKey = "Access-Control-Allow-Origin";

        /// <summary>
        /// 共享 HttpClient。原来每个请求都 new 一个，每个实例都有自己的连接池和 socket，
        /// 高频下载时内存和句柄会持续增长
        /// </summary>
        private static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };

        private IRequest _orgRequest;
        private Task<HttpResponseMessage> _downTask;
        private int _dataReadCount = 0;
        private byte[] _contentBytes;

        private string TrimUrl(string url)
        {
            return url.Replace("&" + UrlFlag + "&", string.Empty)
               .Replace("&" + UrlFlag, string.Empty)
               .Replace(UrlFlag + "&", string.Empty)
               .Replace(UrlFlag, string.Empty);
        }

        public DownLoadResourceHandler(IRequest orgRequest)
        {
            _orgRequest = orgRequest;

            var httpMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(orgRequest.Method),
                RequestUri = new Uri(TrimUrl(orgRequest.Url))
            };
            for (var i = 0; i < orgRequest.Headers.Keys.Count; i++)
            {
                var name = orgRequest.Headers.Keys[i];
                var vals = orgRequest.Headers.GetValues(name);
                httpMessage.Headers.Add(name, vals);
            }
            if (!string.IsNullOrWhiteSpace(orgRequest.ReferrerUrl))
            {
                httpMessage.Headers.Referrer = new Uri(orgRequest.ReferrerUrl);
            }
            if (orgRequest.PostData != null)
            {
                HttpContent content = null;
                var eles = orgRequest.PostData.Elements;
                if (eles != null && eles.Any())
                {
                    foreach (var ele in eles)
                    {
                        if (ele.Type == PostDataElementType.File)
                        {
                            var mcontent = new MultipartFormDataContent();
                            mcontent.Add(new ByteArrayContent(ele.Bytes), ele.File, ele.File);
                            content = mcontent;
                        }
                        else if (ele.Type == PostDataElementType.Bytes)
                        {
                            content = new ByteArrayContent(ele.Bytes);
                        }
                    }
                }
                httpMessage.Content = content;
            }
            _downTask = httpClient.SendAsync(httpMessage);
        }

        public void Cancel()
        {
        }

        public void Dispose()
        {
            _contentBytes = null;

            try
            {
                if (_downTask == null)
                {
                    return;
                }

                if (_downTask.Status == TaskStatus.RanToCompletion)
                {
                    _downTask.Result?.Dispose();
                }
                else if (!_downTask.IsCompleted)
                {
                    //导航/取消导致响应提前结束，也要把已经拿到的内容释放掉
                    _ = _downTask.ContinueWith(t => t.Result?.Dispose(), TaskContinuationOptions.OnlyOnRanToCompletion);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 取响应字节，只读一次，避免每次 Read 都重新分配一份完整内容的副本
        /// </summary>
        private byte[] GetResponseBytes()
        {
            if (_contentBytes == null)
            {
                _downTask.Wait();
                _contentBytes = _downTask.Result.Content.ReadAsByteArrayAsync().Result;
            }

            return _contentBytes;
        }

        public void GetResponseHeaders(IResponse response, out long responseLength, out string redirectUrl)
        {
            _downTask.Wait();
            var headers = _downTask.Result.Headers;
            var contentHeaders = _downTask.Result.Content.Headers;
            foreach (var head in headers)
            {
                if (head.Key.Equals(CrosKey, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                //response.Headers.Set(head.Key, head.Value.First());
                response.SetHeaderByName(head.Key, head.Value.First(), true);
            }

            foreach (var head in contentHeaders)
            {
                //response.Headers.Set(head.Key, head.Value.First());
                response.SetHeaderByName(head.Key, head.Value.First(), true);
            }

            response.StatusCode = (int)_downTask.Result.StatusCode;
            response.SetHeaderByName(CrosKey, "*", true);
            responseLength = GetResponseBytes().Length;
            redirectUrl = null;
        }

        public bool Open(IRequest request, out bool handleRequest, ICallback callback)
        {
            handleRequest = true;
            return true;
        }

        public bool ProcessRequest(IRequest request, ICallback callback)
        {
            throw new NotImplementedException();
        }

        public bool Read(Stream dataOut, out int bytesRead, IResourceReadCallback callback)
        {
            var bytes = GetResponseBytes();
            int leftToRead = bytes.Length - _dataReadCount;
            if (leftToRead == 0)
            {
                bytesRead = 0;
                return false;
            }

            int needRead = Math.Min((int)dataOut.Length, leftToRead); // 最大为dataOut.Lenght
            dataOut.Write(bytes, _dataReadCount, needRead);
            _dataReadCount += needRead;
            bytesRead = needRead;
            return true; ;
        }

        public bool ReadResponse(Stream dataOut, out int bytesRead, ICallback callback)
        {
            throw new NotImplementedException();
        }

        public bool Skip(long bytesToSkip, out long bytesSkipped, IResourceSkipCallback callback)
        {
            throw new NotImplementedException();
        }
    }
}
