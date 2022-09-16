using AutoTest.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Biz
{
    public class TestTaskBagBiz
    {
        public static List<TestCase> Order(List<TestCase> testCases, TestTaskBag testTaskBag)
        {
            var orderIdx = 0;
            return testCases.OrderBy(p =>
            {
                var idx = testTaskBag.OrderCaseId.IndexOf(p.Id);
                if (idx == -1)
                {
                    return orderIdx++;
                }
                orderIdx++;
                return idx;
            }).ToList();
        }

        public static List<TestTask> Order(List<TestTask> testCases, TestTaskBag testTaskBag)
        {
            var orderIdx = 0;
            return testCases.OrderBy(p =>
            {
                var idx = testTaskBag.OrderCaseId.IndexOf(p.TestCase.Id);
                if (idx == -1)
                {
                    return orderIdx++;
                }
                orderIdx++;
                return idx;
            }).ToList();
        }
    }
}
