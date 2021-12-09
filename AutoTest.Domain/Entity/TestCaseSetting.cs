using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestCaseSetting
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

        public string SettingJson
        {
            get;
            set;
        }
    }
}
