using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Exceptions
{
    public class JSException:Exception
    {
        public JSException(string msg) : base(msg)
        {

        }
    }
}
