using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestResult:IComparable
    {
        public static readonly string Index_TestCaseId_EnvId_TestDate = $"{nameof(TestCaseId)}_{nameof(EnvId)}_{nameof(TestStartDate)}";

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

        public int EnvId
        {
            get;
            set;
        }

        public DateTime TestStartDate
        {
            get;
            set;
        }

        public DateTime TestEndDate
        {
            get;
            set;
        }

        public bool Success
        {
            get;
            set;
        }

        public bool IsTimeOut
        {
            get;
            set;
        }

        public string ResultContent
        {
            get;
            set;
        }

        public string FailMsg
        {
            get;
            set;
        }

        public int CompareTo(object obj)
        {
            if (obj is TestResult)
            {
                return this.Id.CompareTo(((TestResult)obj).Id);
            }

            return 1;
        }
    }
}
