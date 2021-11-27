using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    /// <summary>
    /// 任务控制配置
    /// </summary>
    public class TaskConfig
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName
        {
            get;
            set;
        }

        /// <summary>
        /// 是否使用
        /// </summary>
        public bool Enable
        {
            get;
            set;
        }

        /// <summary>
        /// 任务配置
        /// </summary>
        public string Corn
        {
            get;
            set;
        }

        /// <summary>
        /// 是否使用代理
        /// </summary>
        public bool UseProxy
        {
            get;
            set;
        }

        /// <summary>
        /// 抓取的最早时间，之前不处理了
        /// </summary>
        public DateTime MinGrabDate
        {
            get;
            set;
        }
    }
}
