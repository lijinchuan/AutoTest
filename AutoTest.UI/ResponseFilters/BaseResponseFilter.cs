using CefSharp;
using System;
using System.IO;

namespace AutoTest.UI.ResponseFilters
{
    /// <summary>
    /// 响应过滤器基类
    /// </summary>
    public abstract class BaseResponseFilter : IResponseFilter
    {

        private readonly MemoryStream stream;
        /// <summary>
        /// 基础响应过滤器
        /// </summary>
        public BaseResponseFilter()
        {
            stream = new MemoryStream();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            // Method intentionally left empty.
        }

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

                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    dataInRead = dataIn.Length;
                    dataOutWritten = Math.Min(dataInRead, dataOut.Length);

                    dataIn.CopyTo(dataOut);
                    _ = dataIn.Seek(0, SeekOrigin.Begin);
                    byte[] bs = new byte[dataIn.Length];
                    var len = dataIn.Read(bs, 0, bs.Length);
                    stream.Write(bs, 0, len);

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
