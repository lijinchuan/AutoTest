using AutoTest.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.Resources
{
    /// <summary>
    /// mime资源
    /// </summary>
    public class MimeResource
    {
        private static List<Mime> _mimeList = new List<Mime>();

        static MimeResource()
        {
            _mimeList.Add(Mime.Default);
            _mimeList.Add(new Mime
            {
                MimeName= "image/jpeg"
            });
            _mimeList.Add(new Mime
            {
                MimeName = "image/x-icon",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "image/png",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "image/gif",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "image/jpeg",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "image/bmp",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "image/jpeg",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "application/octet-stream",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "text/plain",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "text/css",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "text/xml",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "text/html",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "application/javascript",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "application/json",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "audio/mp4",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "video/mpeg",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "application/json",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "application/pdf",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "application/vnd.ms-excel",

            });
            _mimeList.Add(new Mime
            {
                MimeName = "application/xhtml+xml",

            });
        }

        public static List<Mime> GetMimes()
        {
            return _mimeList;
        }

        public static Mime Find(string mimeType)
        {
            foreach(var item in GetMimes())
            {
                if (item.MimeName.Equals(mimeType, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
        }
        
    }
}
