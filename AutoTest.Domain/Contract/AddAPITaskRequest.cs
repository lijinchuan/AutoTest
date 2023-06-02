using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Contract
{
    public class AddAPITaskRequest
    {
        public int CaseId
        {
            get;
            set;
        }

        public Dictionary<string,string> Params
        {
            get;
            set;
        }
    }
}
