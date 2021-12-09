using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestPage
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public int Order
        {
            get;
            set;
        }
    }
}
