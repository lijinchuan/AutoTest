using AutoTest.UI.WebBrowser.EventListener;
using CefSharp;
using System;
using System.IO;
using System.Threading;

namespace AutoTest.UI.WebBrowser.ResponseFilters
{
    /// <summary>
    /// 响应过滤器基类
    /// </summary>
    public abstract class BaseResponseFilter : IResponseFilter
    {
        private readonly MemoryStream stream;
        private int streamReleased;

        /// <summary>
        /// 创建时间刻度，用于淘汰最旧的过滤器
        /// </summary>
        public long CreatedTick { get; private set; }

        /// <summary>
        /// 基础响应过滤器
        /// </summary>
        public BaseResponseFilter()
        {
            stream = new MemoryStream();
            CreatedTick = DateTime.Now.Ticks;
        }

        /// <summary>
        /// 注意：这里不能释放 stream。
        /// CefSharp 在响应读取结束、OnResourceLoadComplete 回调之前就会调用 Dispose，
        /// 而 OnResourceLoadComplete 还要读 stream，提前释放会抛“无法访问已关闭的流”。
        /// 响应体统一在读完之后用 ReleaseBuffer() 释放。
        /// </summary>
        public void Dispose()
        {
            //故意留空，见上面的说明
        }

        /// <summary>
        /// 释放响应内容缓存。必须在读完 stream 之后调用（OnResourceLoadComplete 的 finally），
        /// 否则抓取到的响应体会一直占着内存
        /// </summary>
        public void ReleaseBuffer()
        {
            if (Interlocked.Exchange(ref streamReleased, 1) == 0)
            {
                stream.Dispose();
            }
        }

        /// <summary>
        /// 获取文本内容
        /// </summary>
        /// <returns></returns>
        public abstract string GetContent(WebEvent webEvent);

        public virtual FilterStatus Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
        {
            try
            {

                if (dataIn == null || dataIn.Length == 0)
                {
                    dataInRead = 0;
                    dataOutWritten = 0;

                    return FilterStatus.Done;
                }
                //解决Transfer-Encoding: chunked数据不全的问题
                if (dataIn.Length > dataOut.Length)
                {
                    var data = new byte[dataOut.Length];
                    _ = dataIn.Seek(0, SeekOrigin.Begin);
                    var len = dataIn.Read(data, 0, data.Length);
                    dataOut.Write(data, 0, len);

                    dataInRead = dataOut.Length;
                    dataOutWritten = dataOut.Length;

                    if (streamReleased == 0)
                    {
                        stream.Write(data, 0, len);
                    }
                }
                else
                {
                    dataInRead = dataIn.Length;
                    dataOutWritten = Math.Min(dataInRead, dataOut.Length);

                    dataIn.CopyTo(dataOut);
                    _ = dataIn.Seek(0, SeekOrigin.Begin);
                    byte[] bs = new byte[dataIn.Length];
                    var len = dataIn.Read(bs, 0, bs.Length);
                    if (streamReleased == 0)
                    {
                        stream.Write(bs, 0, len);
                    }

                    dataInRead = dataIn.Length;
                    dataOutWritten = dataIn.Length;
                }

                return FilterStatus.NeedMoreData;
            }
            catch (Exception ex)
            {
                dataInRead = dataIn.Length;
                dataOutWritten = dataIn.Length;

                return FilterStatus.Done;
            }
        }

        public bool InitFilter()
        {
            return true;
        }

        public Stream GetStream()
        {
            _ = stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
