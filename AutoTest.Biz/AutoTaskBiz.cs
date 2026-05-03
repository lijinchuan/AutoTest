using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Biz
{
    public class AutoTaskBiz
    {
        public List<TestTaskBag> GetNextRunTaskBagList()
        {
            if (RuntimeConfig.IsRepirMode)
            {
                return new List<TestTaskBag>();
            }

            var bagList = AutoTest.Data.DataStoreSwitcher.Current.Find<TestTaskBag>(nameof(TestTaskBag), nameof(TestTaskBag.BagName), new object[] { }).ToList();
            List<TestTaskBag> list = new List<TestTaskBag>();
            foreach (var bag in bagList)
            {
                var log = AutoTest.Data.DataStoreSwitcher.Current.Find<TaskBagLog>(nameof(TaskBagLog), nameof(TaskBagLog.TaskBagId), new object[] { bag.Id }).FirstOrDefault();

                if (log == null)
                {
                    log = new TaskBagLog
                    {
                        TaskBagId = bag.Id,
                        LastTime = DateTime.Now
                    };
                    AutoTest.Data.DataStoreSwitcher.Current.Insert(nameof(TaskBagLog), log);
                    continue;
                }

                var now = log.LastTime;

                var dt = CronHelper.GetNextDateTime(bag.Corn, now);
                if (dt == null)
                {
                    continue;
                }

                if (dt <= DateTime.Now)
                {

                    log.LastTime = DateTime.Now;
                    AutoTest.Data.DataStoreSwitcher.Current.Update(nameof(TaskBagLog), log);


                    list.Add(bag);
                }
            }

            return list;
        }
    }
}
