using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutoTest.Domain.Entity
{
    [Serializable]
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
        [NonSerialized]
        private Action<TestResult> _resultNotify;
        [XmlIgnore]
        public Action<TestResult> ResultNotify
        {
            get
            {
                return _resultNotify;
            }
            set
            {
                _resultNotify = value;
            }
        }

        public TestTaskBag Bag
        {
            get;
            set;
        }

        public bool Check
        {
            get;
            set;
        } = true;

        public string GetTaskName()
        {
            return this.TestSource.SourceName + "_" + this.TestSite.Name + "_" + this.TestPage.Name + "_" + this.TestCase.CaseName;
        }
    }
}
