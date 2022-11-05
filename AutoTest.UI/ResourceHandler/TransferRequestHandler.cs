using AutoTest.Domain.Entity;
using CefSharp;
using CefSharp.Callback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.ResourceHandler
{
    public class TransferRequestHandler : IResourceHandler
    {
        private byte[] _localResourceData = null;
        private string _localResourceFileName = null;
        private int _dataReadCount = 0;

        public TransferRequestHandler(string localFileName)
        {
            _localResourceFileName = localFileName;
        }

        public TransferRequestHandler(byte[] content)
        {
            _localResourceData = content;
        }

        public void Cancel()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void GetResponseHeaders(IResponse response, out long responseLength, out string redirectUrl)
        {
            response.Charset = "UTF-8";
            if (!string.IsNullOrWhiteSpace(_localResourceFileName))
            {
                using (FileStream fileStream = new FileStream(this._localResourceFileName, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        long length = fileStream.Length;
                        this._localResourceData = new byte[length];
                        // 读取文件中的内容并保存到私有变量字节数组中
                        binaryReader.Read(this._localResourceData, 0, this._localResourceData.Length);
                    }
                }
            }

            responseLength = _localResourceData == null ? 0 : _localResourceData.LongLength;
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
            int leftToRead = _localResourceData.Length - this._dataReadCount;
            if (leftToRead == 0)
            {
                bytesRead = 0;
                return false;
            }

            int needRead = Math.Min((int)dataOut.Length, leftToRead); // 最大为dataOut.Lenght
            dataOut.Write(this._localResourceData, this._dataReadCount, needRead);
            this._dataReadCount += needRead;
            bytesRead = needRead;
            return true;
        }

        //响应长度未知，才会调用
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
