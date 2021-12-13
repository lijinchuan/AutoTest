using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestEnvParam:IComparable
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

        public int EnvId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Val
        {
            get;
            set;
        }

        public int CompareTo(object obj)
        {
            if (obj is TestEnvParam)
            {
                return this.Id.CompareTo(((TestEnvParam)obj).Id);
            }

            return 1;
        }
    }
}
