using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    /// <summary>
    /// 测试包
    /// </summary>
    public class TestSource : IComparable, ISearch
    {
        public int Id
        {
            get;
            set;
        }

        public string SourceName
        {
            get;
            set;
        }

        public string Desc
        {
            get;
            set;
        }

        public int CompareTo(object obj)
        {
            if (obj is TestSource)
            {
                return this.Id.CompareTo(((TestSource)obj).Id);
            }

            return 1;
        }

        public bool Search(string wd)
        {
            return (this.SourceName ?? string.Empty).IndexOf(wd, StringComparison.OrdinalIgnoreCase) > -1
                || (this.Desc ?? string.Empty).IndexOf(wd, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}
