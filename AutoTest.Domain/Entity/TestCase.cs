using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    /// <summary>
    /// 测试实例
    /// </summary>
    public class TestCase
    {
        public int Id
        {
            get;
            set;
        }

        public int BagId
        {
            get;
            set;
        }

        public string CaseName
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public List<TestScript> ScriptsOnLoad
        {
            get;
            set;
        }

        public List<TestScript> ScriptsCheckResult
        {
            get;
            set;
        }
    }
}
