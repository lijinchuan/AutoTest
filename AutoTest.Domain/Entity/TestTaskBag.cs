using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestTaskBag
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

        public string BagName
        {
            get;
            set;
        }

        public int TestEnvId
        {
            get;
            set;
        }

        public int TestLoginId
        {
            get;
            set;
        }

        public List<int> CaseId
        {
            get;
            set;
        }

        /// <summary>
        /// 重新排序
        /// </summary>
        public List<int> OrderCaseId
        {
            get;
            set;
        }

        public string Corn
        {
            get;
            set;
        }
    }
}
