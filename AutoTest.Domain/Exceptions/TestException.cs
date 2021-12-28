using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Exceptions
{
    /// <summary>
    /// 测试异常
    /// </summary>
    public class TestException : Exception
    {
        public TestException(string message) : base(message)
        {

        }
    }
}
