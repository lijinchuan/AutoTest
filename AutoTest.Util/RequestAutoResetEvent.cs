using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.Util
{
    public class RequestAutoResetEvent
    {
        private readonly AutoResetEvent autoResetEvent = null;
        private object resultdata = null;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="initialState">若要将初始状态设置为终止，则为 true；若要将初始状态设置为非终止，则为 false。</param>
        public RequestAutoResetEvent(bool initialState)
        {
            autoResetEvent = new AutoResetEvent(initialState);
        }

        public bool Reset()
        {
            return autoResetEvent.Reset();
        }

        public bool Set()
        {
            return autoResetEvent.Set();
        }

        public bool Set(object data)
        {
            resultdata = data;
            return autoResetEvent.Set();
        }

        public bool WaitOne(int millisecondsTimeout)
        {
            return autoResetEvent.WaitOne(millisecondsTimeout);
        }

        public object GetData()
        {
            return resultdata;
        }

    }
}
