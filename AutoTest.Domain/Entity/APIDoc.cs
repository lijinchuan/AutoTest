using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class APIDoc : INodeContents,IComparable
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

        public int CompareTo(object obj)
        {
            if (obj is APIDoc)
            {
                return this.Id.CompareTo(((APIDoc)obj).Id);
            }

            return 1;
        }

        public NodeContentType GetNodeContentType()
        {
            return NodeContentType.DOC;
        }
    }
}
