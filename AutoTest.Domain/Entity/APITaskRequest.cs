using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class APITaskRequest
    {
        public int Id
        {
            get;
            set;
        }

        public int CaseId
        {
            get;
            set;
        }

        public Dictionary<string, string> Params
        {
            get;
            set;
        }

        public DateTime CDate
        {
            get;
            set;
        }

        public int State
        {
            get;
            set;
        }
    }
}
