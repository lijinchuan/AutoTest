using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain
{
    public class NodeContents : INodeContents
    {
        private NodeContentType _nodeContentType;

        public NodeContents(NodeContentType nodeContentType)
        {
            _nodeContentType = nodeContentType;
        }

        public NodeContentType GetNodeContentType()
        {
            return _nodeContentType;
        }
    }
}
