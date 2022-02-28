using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
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

        public string Corn
        {
            get;
            set;
        }
    }
}
