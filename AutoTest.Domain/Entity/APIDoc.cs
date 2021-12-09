using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class APIDoc : INodeContents
    {
        public int Id
        {
            get;
            set;
        }

        public int TestSourceId
        {
            get;
            set;
        }

        public int TestCaseId
        {
            get;
            set;
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string Mark
        {
            get;
            set;
        }

        public NodeContentType GetNodeContentType()
        {
            return NodeContentType.DOC;
        }
    }
}
