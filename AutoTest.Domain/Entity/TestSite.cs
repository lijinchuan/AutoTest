using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestSite:IComparable
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

        public int SourceId
        {
            get;
            set;
        }

        public string CheckLoginCode
        {
            get;
            set;
        }

        public int CompareTo(object obj)
        {
            if (obj is TestSite)
            {
                return this.Id.CompareTo(((TestSite)obj).Id);
            }

            return 1;
        }
    }
}
