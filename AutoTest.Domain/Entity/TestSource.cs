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
    public class TestSource:IComparable
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
    }
}
