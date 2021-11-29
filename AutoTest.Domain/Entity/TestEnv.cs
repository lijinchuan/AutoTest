using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestEnv : INodeContents
    {
        public int Id
        {
            get;
            set;
        }

        public int SourceId
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

        public NodeContentType GetNodeContentType()
        {
            return NodeContentType.ENV;
        }
    }
}
