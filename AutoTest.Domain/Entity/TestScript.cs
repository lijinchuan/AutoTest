using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestScript:IComparable
    {
        public static readonly string Index3 = $"{nameof(SourceId)}_{nameof(SiteId)}_{nameof(ScriptName)}";

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

        public int CompareTo(object obj)
        {
            if (obj is TestScript)
            {
                return this.Id.CompareTo(((TestScript)obj).Id);
            }

            return 1;
        }
    }
}
