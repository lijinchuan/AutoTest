using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Contract
{
    public class AddAPITaskResponse: ApiTaskBaseResponse
    {
        public int TaskId
        {
            get;
            set;
        }
    }
}
