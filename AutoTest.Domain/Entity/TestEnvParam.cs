using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestEnvParam
    {
        public int Id
        {
            get;
            set;
        }

        public int APISourceId
        {
            get;
            set;
        }

        public int EnvId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Val
        {
            get;
            set;
        }
    }
}
