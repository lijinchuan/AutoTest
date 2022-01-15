using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestCookieContainer
    {
        public static readonly string IX = $"{nameof(SiteId)}_{nameof(Env)}_{nameof(Account)}";

        public int Id
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public int Env
        {
            get;
            set;
        }

        public int Account
        {
            get;
            set;
        }

        public DateTime CreateTime
        {
            get;
            set;
        }

        public DateTime Expires
        {
            get;
            set;
        }

        public List<TestCookie> TestCookies
        {
            get;
            set;
        }
    }
}
