using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class APITaskResult
    {
        public int Id
        {
            get;
            set;
        }

        public int TaskId
        {
            get;
            set;
        }

        public string Result
        {
            get;
            set;
        }

        public DateTime CDate
        {
            get;
            set;
        }

        public double UseMillSecs
        {
            get;
            set;
        }
    }
}
