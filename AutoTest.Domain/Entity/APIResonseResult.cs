using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class APIResonseResult
    {

        public byte[] Raw
        {
            get;
            set;
        }

        public Dictionary<string, string> Headers
        {
            get;
            set;
        }

        public List<RespCookie> Cookies
        {
            get;
            set;
        }
    }
}
