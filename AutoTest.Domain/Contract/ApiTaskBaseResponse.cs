using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Contract
{
    public class ApiTaskBaseResponse
    {
        public int Code
        {
            get;
            set;
        } = 200;

        public string Message
        {
            get;
            set;
        } = "成功";
    }
}
