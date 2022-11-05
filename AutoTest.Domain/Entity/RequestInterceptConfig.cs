using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class RequestInterceptConfig
    {
        public int Id
        {
            get;
            set;
        }

        public int TestCaseId
        {
            get;
            set;
        }

        public string MatchUrl
        {
            get;
            set;
        }

        public int MatchType
        {
            get;
            set;
        }

        public string Response
        {
            get;
            set;
        }

        public bool Enabled
        {
            get;
            set;
        }
    }
}
