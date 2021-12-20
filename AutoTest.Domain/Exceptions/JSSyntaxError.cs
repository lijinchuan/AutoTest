using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Exceptions
{
    public class JSSyntaxError : JSException
    {
        public JSSyntaxError(string msg) : base(msg)
        {

        }
    }
}
