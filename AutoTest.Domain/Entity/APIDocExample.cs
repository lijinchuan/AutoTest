using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TestCaseDocExample
    {
        public int Id
        {
            get;
            set;
        }

        public int TestCaseId
        {
            get;
            set;
        }

        /// <summary>
        /// 场景名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        public string Req
        {
            get;
            set;
        }

        public string RespEncode
        {
            get;
            set;
        }

        public string Resp
        {
            get;
            set;
        }
    }
}
