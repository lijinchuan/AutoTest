using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestCaseSettingObj
    {
        public int TimeOut
        {
            get;
            set;
        }

        public bool NoPrxoy
        {
            get;
            set;
        }

        /// <summary>
        /// 并发请求数
        /// </summary>
        public int PSendNumber
        {
            get;
            set;
        } = 1;

        public bool SaveResp
        {
            get;
            set;
        } = true;

        public bool Create_SSL_TLS_secure_channel
        {
            get;
            set;
        } = true;
    }
}
