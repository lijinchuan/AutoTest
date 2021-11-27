using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestConfig
    {
        /// <summary>
        /// 代理服务器
        /// </summary>
        public ProxyConfig ProxyConfig
        {
            get;
            set;
        }

        /// <summary>
        /// 任务配置
        /// </summary>
        public TaskConfig[] TaskConfigs
        {
            get;
            set;
        }
    }
}
