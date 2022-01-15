using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Biz
{
    public static class CounterBiz
    {
        static object locker = new object();
        public static long Inc(string name, long init = 0)
        {
            lock (locker)
            {
                var cnt = BigEntityTableRemotingEngine.Find<Counter>(nameof(Counter),nameof(Counter.CounterName),new object[] { name }).FirstOrDefault();
                if (cnt == null)
                {
                    cnt = new Counter
                    {
                        CounterName = name,
                        Value = 0
                    };
                }

                if (init > cnt.Value)
                {
                    cnt.Value = init;
                }

                cnt.Value++;

                BigEntityTableRemotingEngine.Upsert(nameof(Counter), cnt);
                return cnt.Value;
            }
        }
    }
}
