using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestCookieContainer
    {
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

        public string Env
        {
            get;
            set;
        }

        public string Account
        {
            get;
            set;
        }

        public DateTime CreateTime
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
