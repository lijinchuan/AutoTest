using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestPage:IUpdate,ISearch
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

        public int CompareTo(object obj)
        {
            if (obj is TestPage)
            {
                return this.Id.CompareTo(((TestPage)obj).Id);
            }

            return 1;
        }

        public string GetDisplayText()
        {
            return Name;
        }

        public IComparable GetParentUpdate()
        {
            return new TestSite
            {
                Id = SiteId
            };
        }

        public bool Search(string wd)
        {
            return (this.Url ?? string.Empty).IndexOf(wd, StringComparison.OrdinalIgnoreCase) > -1
                || (this.Name ?? string.Empty).IndexOf(wd, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}
