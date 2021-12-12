using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestTask
    {
        public TestSource TestSource
        {
            get;
            set;
        }

        public TestSite TestSite
        {
            get;
            set;
        }

        public TestPage TestPage
        {
            get;
            set;
        }

        public TestCase TestCase
        {
            get;
            set;
        }

        public TestLogin TestLogin
        {
            get;
            set;
        }

        public TestEnv TestEnv {
            get;
            set;
        }
        
        public List<TestEnvParam> TestEnvParams
        {
            get;
            set;
        }

        public List<TestScript> GlobalTestScripts
        {
            get;
            set;
        }

        public List<TestScript> SiteTestScripts
        {
            get;
            set;
        }

        public Action<TestResult> ResultNotify
        {
            get;
            set;
        }
    }
}
