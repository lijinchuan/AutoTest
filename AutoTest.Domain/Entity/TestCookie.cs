using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestCookie
    {
        public string Name { get; set; }
        //
        // 摘要:
        //     The cookie value.
        public string Value { get; set; }
        //
        // 摘要:
        //     If domain is empty a host cookie will be created instead of a domain cookie.
        //     Domain cookies are stored with a leading "." and are visible to sub-domains whereas
        //     host cookies are not.
        public string Domain { get; set; }
        //
        // 摘要:
        //     Ss non-empty only URLs at or below the path will get the cookie value.
        public string Path { get; set; }
        //
        // 摘要:
        //     If true the cookie will only be sent for HTTPS requests.
        public bool Secure { get; set; }
        //
        // 摘要:
        //     Ss true the cookie will only be sent for HTTP requests.
        public bool HttpOnly { get; set; }
        //
        // 摘要:
        //     Expires or null if no expiry
        public DateTime Expires { get; set; }
        ////
        //// 摘要:
        ////     The cookie creation date. This is automatically populated by the system on cookie
        ////     creation.
        //public DateTime Creation { get; }
        ////
        //// 摘要:
        ////     The cookie last access date. This is automatically populated by the system on
        ////     access.
        //public DateTime LastAccess { get; }
        //
        // 摘要:
        //     Same site.
        public int SameSite { get; set; }
        //
        // 摘要:
        //     Priority
        public int Priority { get; set; }
    }
}
