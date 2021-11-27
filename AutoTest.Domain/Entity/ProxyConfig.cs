using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class ProxyConfig
    {
        /// <summary>
        /// 启用代理，默认为启用
        /// </summary>
        public bool? Enabled { get; set; }

        /// <summary>
        /// 代理地址
        /// </summary>
        public string ProxyUri { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }
}
