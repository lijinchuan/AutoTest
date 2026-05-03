using AutoTest.Data;
using AutoTest.Domain.Entity;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.Biz.SimulateServer
{
    public static class ApiTaskTrigger
    {
        private const int MaxQueueLength = 100;
        private const int MaxWorkerCount = 5;

        private class ApiQueueItem
        {
            public TestTask TestTask { get; set; }
            public APITaskRequest ApiTaskRequest { get; set; }
        }

        private static readonly BlockingCollection<ApiQueueItem> queue = new BlockingCollection<ApiQueueItem>(MaxQueueLength);
        private static readonly ConcurrentDictionary<int, TaskCompletionSource<bool>> runningTasks = new ConcurrentDictionary<int, TaskCompletionSource<bool>>();
        private static int started = 0;

        public static event Action<int, TestTask, APITaskRequest> NewTaskRecived;

        public static void Start()
        {
            if (Interlocked.Exchange(ref started, 1) == 1)
            {
                return;
            }

            for (var i = 0; i < MaxWorkerCount; i++)
            {
                var workerId = i;
                Task.Factory.StartNew(() => WorkerLoop(workerId), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        public static bool TryEnqueue(TestTask testTask, APITaskRequest apiTaskRequest)
        {
            testTask.TestCase.CaseName += "@" + apiTaskRequest.Id;
            return queue.TryAdd(new ApiQueueItem
            {
                TestTask = testTask,
                ApiTaskRequest = apiTaskRequest
            });
        }

        public static void NotifyCompleted(int taskId)
        {
            if (runningTasks.TryRemove(taskId, out var tcs))
            {
                tcs.TrySetResult(true);
            }
        }

        private static void WorkerLoop(int workerId)
        {
            foreach (var item in queue.GetConsumingEnumerable())
            {
                var tcs = new TaskCompletionSource<bool>();
                runningTasks[item.ApiTaskRequest.Id] = tcs;

                try
                {
                    if (NewTaskRecived == null)
                    {
                        SaveResult(item.ApiTaskRequest.Id, "没有可用的API执行器");
                        tcs.TrySetResult(true);
                    }
                    else
                    {
                        NewTaskRecived(workerId, item.TestTask, item.ApiTaskRequest);
                    }

                    tcs.Task.Wait();
                }
                catch (Exception ex)
                {
                    SaveResult(item.ApiTaskRequest.Id, ex.Message);
                }
                finally
                {
                    runningTasks.TryRemove(item.ApiTaskRequest.Id, out _);
                }
            }
        }

        private static void SaveResult(int taskId, string result)
        {
            APITaskResult exists = null;
            foreach (var item in DataStoreSwitcher.Current.Find<APITaskResult>(nameof(APITaskResult), nameof(APITaskResult.TaskId), new object[] { taskId }))
            {
                exists = item;
                break;
            }

            if (exists == null)
            {
                DataStoreSwitcher.Current.Insert(nameof(APITaskResult), new APITaskResult
                {
                    CDate = DateTime.Now,
                    TaskId = taskId,
                    Result = result,
                    UseMillSecs = 0
                });
            }
        }
    }
}
