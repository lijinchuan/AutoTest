using AutoTest.Domain.Contract;
using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Comm;
using LJC.FrameWorkV3.Data.EntityDataBase;
using LJC.FrameWorkV3.LogManager;
using LJC.FrameWorkV3.Net.HTTP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.Biz.SimulateServer
{
    public class ApiSimulateHandler : IHttpHandler
    {
        private object QueryAPIResult(int taskId,int waitSecs)
        {
            waitSecs = Math.Min(120, waitSecs);
            var secsCount = 0;
            while (true)
            {
                var taskResult = AutoTest.Data.DataStoreSwitcher.Current.Find<APITaskResult>(nameof(APITaskResult), nameof(APITaskResult.TaskId), new object[] { taskId }).FirstOrDefault();

                if (taskResult != null || ++secsCount > waitSecs)
                {
                    var result = new
                    {
                        Result = new
                        {
                            CDate = taskResult == null ? DateTime.Now : taskResult.CDate,
                            UseMillSecs = taskResult == null ? 0 : taskResult.UseMillSecs,
                            TaskId = taskId,
                            Result = taskResult == null ? null : JsonUtil<dynamic>.Deserialize(taskResult.Result)
                        },
                        Code = taskResult == null ? 404 : 200,
                        Message = taskResult == null ? "没有查到结果" : "成功"
                    };

                    ProcessTraceUtil.Trace($"{secsCount}次查询,{(taskResult == null ? "无果" : "成功")}，返回结果");
                    return result;
                }

                ProcessTraceUtil.Trace($"{secsCount}次查询，无果");
                Thread.Sleep(1000);
            }
        }

        private T GetRequest<T>(HttpRequest request)
        {
            T req;
            if ("get".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                foreach(var kv in request.Query.OrderBy(p => p.Key))
                {
                    if (kv.Key.IndexOf('.') > 0)
                    {
                        var parentdic = dic;
                        var subkeys = kv.Key.Split('.');

                        for (var i = 0; i < subkeys.Length; i++)
                        {
                            var key = subkeys[i];

                            if (subkeys.Length - 1 == i)
                            {
                                parentdic.Add(key, kv.Value);
                                break;
                            }

                            var dickey = string.Empty;
                            if (parentdic.ContainsKey(key))
                            {
                                dickey = key;
                            }
                            else
                            {
                                foreach (var k in parentdic.Keys)
                                {
                                    if (key.Equals(k, StringComparison.OrdinalIgnoreCase))
                                    {
                                        dickey = k;
                                        break;
                                    }
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(dickey))
                            {
                                var parent = parentdic[dickey];
                                parentdic = (Dictionary<string, object>)parent;
                            }
                            else
                            {
                                parentdic.Add(key, new Dictionary<string, object>());
                                parentdic = (Dictionary<string, object>)parentdic[key];
                            }
                        }
                    }
                    else
                    {
                        dic.Add(kv.Key, kv.Value);
                    }
                }
                req = JsonUtil<T>.Deserialize(JsonUtil<object>.Serialize(dic));
            }
            else
            {
                req = JsonUtil<T>.Deserialize(request.GetContent());
            }

            return req;
        }

        public bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            var url = request.Url.ToLower().Split('?')[0];
            if (url.StartsWith("http"))
            {
                var sqlArray = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (sqlArray.Length > 2)
                {
                    url = string.Join("/", sqlArray.Skip(2).ToArray());
                }
            }
            else
            {
                url = string.Join("/", url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            }

            try
            {
                ProcessTraceUtil.StartTrace();
                if (url.EndsWith("api/AddAPITask", StringComparison.OrdinalIgnoreCase))
                {
                    var req = GetRequest<AddAPITaskRequest>(request);

                    ProcessTraceUtil.Trace($"收到请求:api/AddAPITask,{Newtonsoft.Json.JsonConvert.SerializeObject(req)}");

                    var newTask = new TaskBiz().CreateTask(req.CaseId);
                    if (newTask == null)
                    {
                        response.ContentType = "text/json;charset=utf-8;";
                        response.Content = JsonUtil<object>.Serialize(new AddAPITaskResponse
                        {
                            Code = 404,
                            Message = "没有找到任务",
                            TaskId = 0
                        });
                        return true;
                    }

                    var addReq = new APITaskRequest
                    {
                        CaseId = req.CaseId,
                        CDate = DateTime.Now,
                        Params = req.Params,
                        State = 0
                    };

                    AutoTest.Data.DataStoreSwitcher.Current.Insert(nameof(APITaskRequest), addReq);

                    ProcessTraceUtil.Trace("创建任务入库完成,准备进入队列");
                    if (!ApiTaskTrigger.TryEnqueue(newTask, addReq))
                    {
                        AutoTest.Data.DataStoreSwitcher.Current.Delete<APITaskRequest>(nameof(APITaskRequest), addReq.Id);
                        response.ContentType = "text/json;charset=utf-8;";
                        response.Content = JsonUtil<object>.Serialize(new AddAPITaskResponse
                        {
                            Code = 429,
                            Message = "队列已满",
                            TaskId = 0
                        });
                        return true;
                    }

                    ProcessTraceUtil.Trace("任务已入队");

                    response.ContentType = "text/json;charset=utf-8;";
                    response.Content = JsonUtil<object>.Serialize(new AddAPITaskResponse
                    {
                        TaskId = addReq.Id
                    });

                    return true;
                }
                else if (url.EndsWith("api/GetAPITaskRequest", StringComparison.OrdinalIgnoreCase))
                {
                    var req = GetRequest<GetApiTaskResultRequest>(request);
                    ProcessTraceUtil.Trace($"收到请求:api/GetAPITaskRequest,{Newtonsoft.Json.JsonConvert.SerializeObject(req)}");

                    var result = QueryAPIResult(req.TaskId, req.WatingSecsForResult);

                    response.ContentType = "text/json;charset=utf-8;";
                    response.Content = JsonUtil<object>.Serialize(result);
                    return true;
                }
            }
            catch (Exception ex)
            {
                var result = new ApiTaskBaseResponse
                {
                    Code = 500,
                    Message = ex.Message
                };

                response.ContentType = "text/json;charset=utf-8;";
                response.Content = JsonUtil<object>.Serialize(result);
                return true;
            }
            finally
            {
                LogHelper.Instance.Debug(ProcessTraceUtil.PrintTrace());
            }

            return false;
        }
    }
}

