using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Contract
{
    public class GetApiTaskResultRequest
    {
        public int TaskId
        {
            get;
            set;
        }

        /// <summary>
        /// 如果没有结果，则等待时长，最多120S，0,表示不等待
        /// </summary>
        public int WatingSecsForResult
        {
            get;
            set;
        }
    }
}
