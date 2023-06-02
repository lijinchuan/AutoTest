using AutoTest.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Contract
{
    public class GetApiTaskResultResponse: ApiTaskBaseResponse
    {
        public APITaskResult Result
        {
            get;
            set;
        }
    }
}
