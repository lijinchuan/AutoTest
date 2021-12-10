using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestScript
    {
        public int Id
        {
            get;
            set;
        }

        public int SourceId
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public string ScriptName
        {
            get;
            set;
        }

        public string Desc
        {
            get;
            set;
        }

        public int Order
        {
            get;
            set;
        }

        public string Body
        {
            get;
            set;
        }

        public bool Enable
        {
            get;
            set;
        }
    }
}
