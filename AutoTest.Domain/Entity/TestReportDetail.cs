using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestReportDetail
    {
        public int Id
        {
            get;
            set;
        }

        public int SorceId
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public int PageId
        {
            get;
            set;
        }

        public int ReportId
        {
            get;
            set;
        }

        public int TestCaseId
        {
            get;
            set;
        }

        public TestEnv TestEnv
        {
            get;
            set;
        }

        public TestEnvParam TestEnvParam
        {
            get;
            set;
        }

        public TestCase TestCase
        {
            get;
            set;
        }

        public TestResult TestResult
        {
            get;
            set;
        }
    }
}
