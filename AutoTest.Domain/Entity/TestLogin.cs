using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestLogin:IComparable
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

        public int CompareTo(object obj)
        {
            if (obj is TestLogin)
            {
                return this.Id.CompareTo(((TestLogin)obj).Id);
            }

            return 1;
        }
    }
}
