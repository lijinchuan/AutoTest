using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class Mime
    {
        public const string JsonMime = "application/json";
        public const string JsMime = "application/javascript";
        public const string HtmlMime = "text/html";

        public static readonly Mime Default = new Mime() { MimeName = "未知" };

        public string MimeName
        {
            get;
            set;
        }

        public byte[] DefaultBytes
        {
            get;
            set;
        }

        public string DetaultText
        {
            get;
            set;
        }
    }
}
