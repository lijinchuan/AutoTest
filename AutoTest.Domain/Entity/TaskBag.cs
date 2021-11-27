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
    public class TaskBag
    {
        public int Id
        {
            get;
            set;
        }

        public string BagName
        {
            get;
            set;
        }

        public AuthCofig AuthCofig
        {
            get;
            set;
        }
    }
}
