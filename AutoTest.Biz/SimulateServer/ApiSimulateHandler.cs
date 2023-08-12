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
                var taskResult = BigEntityTableEngine.LocalEngine.Find<APITaskResult>(nameof(APITaskResult), nameof(APITaskResult.TaskId), new object[] { taskId }).FirstOrDefault();

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
                    
                    if (newTask != null)
                    {
                        var addReq = new APITaskRequest
                        {
                            CaseId = req.CaseId,
                            CDate = DateTime.Now,
                            Params = req.Params,
                            State = 0
                        };

                        BigEntityTableEngine.LocalEngine.Insert(nameof(APITaskRequest), addReq);

                        ProcessTraceUtil.Trace("创建任务入库完成,触发任务");
                        ApiTaskTrigger.Trigger(newTask, addReq);

                        ProcessTraceUtil.Trace("触发任务完成");

                        if (req.WatingSecsForResult <= 0)
                        {
                            var result = new AddAPITaskResponse
                            {
                                TaskId = addReq.Id
                            };

                            response.ContentType = "text/json;charset=utf-8;";
                            response.Content = JsonUtil<object>.Serialize(result);
                        }
                        else
                        {
                            var result = QueryAPIResult(addReq.Id, req.WatingSecsForResult);

                            response.ContentType = "text/json;charset=utf-8;";
                            response.Content = JsonUtil<object>.Serialize(result);
                        }
                    }

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
