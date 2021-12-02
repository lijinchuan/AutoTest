using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestLogin
    {
        public int Id
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public string LoginCode
        {
            get;
            set;
        }

        public string ValidCode
        {
            get;
            set;
        }

        /// <summary>
        /// 是否人工
        /// </summary>
        public bool IsMannual
        {
            get;
            set;
        }
    }
}
