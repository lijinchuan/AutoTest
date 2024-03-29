﻿using AutoTest.Domain.Entity;
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

            var bagList = BigEntityTableRemotingEngine.Find<TestTaskBag>(nameof(TestTaskBag), p => !string.IsNullOrWhiteSpace(p.Corn)).ToList();
            List<TestTaskBag> list = new List<TestTaskBag>();
            foreach (var bag in bagList)
            {
                var log = BigEntityTableRemotingEngine.Find<TaskBagLog>(nameof(TaskBagLog), nameof(TaskBagLog.TaskBagId), new object[] { bag.Id }).FirstOrDefault();

                if (log == null)
                {
                    log = new TaskBagLog
                    {
                        TaskBagId = bag.Id,
                        LastTime = DateTime.Now
                    };
                    BigEntityTableRemotingEngine.Insert(nameof(TaskBagLog), log);
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
                    BigEntityTableRemotingEngine.Update(nameof(TaskBagLog), log);


                    list.Add(bag);
                }
            }

            return list;
        }
    }
}
