using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestLogin : IUpdate, ISearch
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

        /// <summary>
        /// 帐号说明
        /// </summary>
        public string AccountInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 是否使用
        /// </summary>
        public bool Used
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

        public IComparable GetParentUpdate()
        {
            return new TestSite
            {
                Id = SiteId
            };
        }

        public bool Search(string wd)
        {
            return (this.Url ?? string.Empty).IndexOf(wd, StringComparison.OrdinalIgnoreCase) > -1
                || (this.LoginCode ?? string.Empty).IndexOf(wd, StringComparison.OrdinalIgnoreCase) > -1
                || (this.ValidCode ?? string.Empty).IndexOf(wd, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}
