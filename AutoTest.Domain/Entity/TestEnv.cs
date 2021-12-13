using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestEnv : INodeContents,IComparable
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

        public string EnvName
        {
            get;
            set;
        }

        public string EnvDesc
        {
            get;
            set;
        }

        public bool Used
        {
            get;
            set;
        }

        public int CompareTo(object obj)
        {
            if (obj is TestEnv)
            {
                return this.Id.CompareTo(((TestEnv)obj).Id);
            }

            return 1;
        }

        public NodeContentType GetNodeContentType()
        {
            return NodeContentType.ENV;
        }
    }
}
