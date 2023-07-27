using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser.EventListener
{
    /// <summary>
    /// 网络事件
    /// </summary>
    public class WebEvent
    {
        /// <summary>
        /// 来源地址
        /// </summary>
        public string SourceUrl
        {
            get;
            set;
        }

        /// <summary>
        /// 响应内容
        /// </summary>
        public byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// 文本内容
        /// </summary>
        public string Content
        {
            get;
            set;
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType
        {
            get;
            set;
        }

        /// <summary>
        /// 响应编码
        /// </summary>
        public string ResponseChartSet
        {
            get;
            set;
        }

        /// <summary>
        /// 响应代码
        /// </summary>
        public int StatusCode
        {
            get;
            set;
        }

        public IBrowser Browser
        {
            get;
            set;
        }

        public IFrame Frame
        {
            get;
            set;
        }
    }
}
