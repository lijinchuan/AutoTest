using AutoTest.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Biz.SimulateServer
{
    public static class ApiTaskTrigger
    {
        public static event Action<TestTask, APITaskRequest> NewTaskRecived;

        public static void Trigger(TestTask testTask, APITaskRequest apiTaskRequest)
        {
            testTask.TestCase.CaseName += "@" + apiTaskRequest.Id;
            NewTaskRecived?.Invoke(testTask, apiTaskRequest);
        }
    }
}
