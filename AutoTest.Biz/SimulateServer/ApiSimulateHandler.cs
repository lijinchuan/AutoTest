using AutoTest.Domain.Contract;
using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Comm;
using LJC.FrameWorkV3.Data.EntityDataBase;
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
        public bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            var url = request.Url.ToLower();
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
                if (url.EndsWith("api/AddAPITask", StringComparison.OrdinalIgnoreCase))
                {
                    var req = JsonUtil<AddAPITaskRequest>.Deserialize(request.Content);

                    var testCase = BigEntityTableEngine.LocalEngine.Find<TestCase>(nameof(TestCase), req.CaseId);

                    if (testCase != null)
                    {
                        var addReq = new APITaskRequest
                        {
                            CaseId = req.CaseId,
                            CDate = DateTime.Now,
                            Params = req.Params,
                            State = 0
                        };

                        var page = BigEntityTableEngine.LocalEngine.Find<TestPage>(nameof(TestPage), testCase.PageId);
                        var site = BigEntityTableEngine.LocalEngine.Find<TestSite>(nameof(TestSite), page.SiteId);
                        var source = BigEntityTableEngine.LocalEngine.Find<TestSource>(nameof(TestSource), site.SourceId);
                        var scripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == source.Id).ToList();
                        var testLogin = BigEntityTableRemotingEngine.Find<TestLogin>(nameof(TestLogin), nameof(TestLogin.SiteId), new object[] { site.Id }).FirstOrDefault(p => p.Used);

                        var testEnvs = BigEntityTableRemotingEngine.Find<TestEnv>(nameof(TestEnv), nameof(TestEnv.SiteId), new object[] { site.Id });
                        var currentEnv = testEnvs.FirstOrDefault(p => p.Used);
                        List<TestEnvParam> testEnvParams = null;
                        if (currentEnv != null)
                        {
                            testEnvParams = BigEntityTableRemotingEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_EnvId", new object[] { site.Id, currentEnv.Id }).ToList();
                        }

                        var newTask = new TestTask
                        {
                            TestSource = source,
                            SiteTestScripts = scripts.Where(p => p.SiteId > 0).ToList(),
                            GlobalTestScripts = scripts.Where(p => p.SiteId == 0).ToList(),
                            TestCase = testCase,
                            TestLogin = testLogin,
                            TestPage = page,
                            TestSite = site,
                            TestEnv = currentEnv,
                            TestEnvParams = testEnvParams
                        };

                        BigEntityTableEngine.LocalEngine.Insert(nameof(APITaskRequest), addReq);

                        ApiTaskTrigger.Trigger(newTask, addReq);

                        var result = new AddAPITaskResponse
                        {
                            TaskId = addReq.Id
                        };

                        response.ContentType = "text/json;charset=utf-8;";
                        response.Content = JsonUtil<object>.Serialize(result);
                    }

                    return true;
                }
                else if (url.EndsWith("api/GetAPITaskRequest", StringComparison.OrdinalIgnoreCase))
                {
                    var req = JsonUtil<GetApiTaskResultRequest>.Deserialize(request.Content);

                    var waitSecs = Math.Min(120, req.WatingSecsForResult);
                    var secsCount = 0;
                    while (true)
                    {
                        var taskResult = BigEntityTableEngine.LocalEngine.Find<APITaskResult>(nameof(APITaskResult), nameof(APITaskResult.TaskId), new object[] { req.TaskId }).FirstOrDefault();

                        if (taskResult != null || ++secsCount > waitSecs)
                        {
                            var result = new
                            {
                                Result = new
                                {
                                    taskResult.CDate,
                                    taskResult.UseMillSecs,
                                    Result=JsonUtil<dynamic>.Deserialize(taskResult.Result)
                                },
                                Code = taskResult == null ? 404 : 200,
                                Message = taskResult == null ? "没有查到结果" : "成功"
                            };
                            
                            response.ContentType = "text/json;charset=utf-8;";
                            response.Content = JsonUtil<object>.Serialize(result);
                            break;
                        }

                        Thread.Sleep(1000);
                    }
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

            return false;
        }
    }
}
