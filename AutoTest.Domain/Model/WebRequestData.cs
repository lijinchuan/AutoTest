using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Model
{
    public class WebRequestData
    {
        public int Code
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public string ResponseContent
        {
            get;
            set;
        }
    }
}
