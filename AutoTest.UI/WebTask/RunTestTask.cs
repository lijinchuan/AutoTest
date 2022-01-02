using AutoMapper;
using AutoTest.Domain.Entity;
using AutoTest.Domain.Exceptions;
using AutoTest.Domain.Model;
using AutoTest.UI.EventListener;
using AutoTest.UI.WebBrowser;
using AutoTest.Util;
using CefSharp;
using LJC.FrameWorkV3.Comm;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.UI.WebTask
{
    public class RunTestTask : WebTask
    {
        private const int TestTimeOut = 60000;

        private readonly IEventListener eventListener = null;
        private readonly RequestAutoResetEvent pageRequestAutoResetEvent = null;
        private readonly IMapper mapper = null;

        private TestSite _testSite;
        private TestPage _testPage;
        private TestLogin _testLogin;
        private TestCase _testCase;
        private TestEnv _testEnv;
        private List<TestEnvParam> _testEnvParams;
        private bool _readyFlag = false;
        private TestCaseData _testCaseData;
        
        private List<TestScript> _globScripts;
        private List<TestScript> _siteScripts;
        private TestResult _testResult;
        private dynamic bag;
        private event Action<TestResult> _notify;
        private object _locker = new object();

        /// <summary>
        /// 出错只警告
        /// </summary>
        private static string[] warnFileExt = new string[] { ".txt", ".css", ".js" , ".woff2", ".gif", ".png",".jpg",".jpeg" };

        public TestLogin TestLogin
        {
            get
            {
                return _testLogin;
            }
        }

        public TestCase TestCase
        {
            get
            {
                return _testCase;
            }
        }

        public RunTestTask(string taskname, bool useProxy, TestSite testSite, TestLogin testLogin,
            TestPage testPage, TestCase testCase, TestEnv testEnv, List<TestEnvParam> testEnvParams,
            List<TestScript> globScripts, List<TestScript> siteScripts,Action<TestResult> notify)
            : base(taskname, Util.ReplaceEvnParams(string.IsNullOrWhiteSpace(testCase.Url)?testPage.Url:testCase.Url, testEnvParams),
                  useProxy, false)
        {

            eventListener = new TestEventListener();
            eventListener.OnProcess += ProcessWebEvent;

            pageRequestAutoResetEvent = new RequestAutoResetEvent(true);

            mapper = AutofacBuilder.GetFromFac<IMapper>();

            _testSite = testSite;
            _testPage = testPage;
            _testCase = testCase;
            _testEnv = testEnv;
            _testEnvParams = testEnvParams;
            _testLogin = testLogin;
            _testCaseData = BigEntityTableRemotingEngine.Find<TestCaseData>(nameof(TestCaseData), nameof(TestCaseData.TestCaseId), new object[] { _testCase.Id }).FirstOrDefault();
            _globScripts = globScripts;
            _siteScripts = siteScripts;
            _notify = notify;

            _testResult = new TestResult
            {
                EnvId = _testEnv == null ? 0 : _testEnv.Id,
                TestCaseId = _testCase.Id,
                Success = false,
                TestStartDate = DateTime.Now,
                TestEndDate = DateTime.Now,
                IsTimeOut = false
            };
        }

        public override void DocumentCompletedHandler(IBrowser browser, IFrame frame, List<CefSharp.Cookie> cookies)
        {
            lock(_locker)
            {
                if (!_readyFlag)
                {
                    _readyFlag = true;

                    _testResult.TestStartDate = DateTime.Now;

                    FireTaskReady();
                }
            }
        }

        public override IEnumerable<CefSharp.Cookie> GetCookieList()
        {
            _testResult.TestStartDate = DateTime.Now;

            if (_testCaseData.Cookies?.Count > 0)
            {
                foreach (var cookie in _testCaseData.Cookies)
                {
                    if (cookie.Checked)
                    {
                        yield return new CefSharp.Cookie
                        {
                            Domain = new Uri(GetStartPageUrl()).Host,
                            Name = Util.ReplaceEvnParams(cookie.Name, _testEnvParams),
                            Value = Util.ReplaceEvnParams(cookie.Value, _testEnvParams),
                            Expires = DateTime.Now.AddYears(1),
                            Path = "/"
                        };
                    }
                }
            }
        }

        public override IEventListener GetEventListener()
        {
            return eventListener;
        }

        public override string GetSite()
        {
            return GetTaskName();
        }

        private void RegistTestScript(IBrowser browser, IFrame frame, TestScript testScript)
        {
            if (!testScript.Enable || string.IsNullOrWhiteSpace(testScript.Body))
            {
                return;
            }

            if (Regex.Match(testScript.Body.TrimStart(), "^(https?:|//)", RegexOptions.IgnoreCase).Success)
            {
                webBrowserTool.RegisterRomoteScript(browser, frame, testScript.Body.TrimStart());
            }
            else
            {
                webBrowserTool.ExecuteScript(browser, frame, testScript.Body);
            }
        }

        private void PrepareTest(IBrowser browser, IFrame frame, object userData)
        {
            if ((!string.IsNullOrWhiteSpace(_testCase.TestCode) || !string.IsNullOrWhiteSpace(_testCase.ValidCode)))
            {
                //注入JQUERY
                webBrowserTool.AddJqueryLib(browser, frame);

                PublishDebugMsg("注入JQUERY");

                //注入工具包
                if (_globScripts != null && _globScripts.Count > 0)
                {
                    foreach (var s in _globScripts.OrderBy(p => p.Order))
                    {
                        RegistTestScript(browser, frame, s);
                    }
                }

                PublishDebugMsg("注入全局工具包");


                if (_siteScripts != null && _siteScripts.Count > 0)
                {
                    foreach (var s in _siteScripts.OrderBy(p => p.Order))
                    {
                        RegistTestScript(browser, frame, s);
                    }
                }

                PublishDebugMsg("注入通用工具包");

                //注入变量
                SetVar(browser, frame, userData);

                PublishDebugMsg("注入变量");
            }
        }


        /// <summary>
        /// 获取用户变量数据
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        private dynamic GetUserVarData(IBrowser browser, IFrame frame)
        {
            var code = $"if(typeof {WebVar.VarName}!='undefined'&&{WebVar.VarName}.{nameof(WebVar.Bag)}) return {WebVar.VarName}.{nameof(WebVar.Bag)}";

            return webBrowserTool.ExecutePromiseScript(browser, frame, code) as dynamic;
        }

        /// <summary>
        /// 获取用户变量数据
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        private void UpdateUserVarData(IBrowser browser, IFrame frame)
        {
            var userData = GetUserVarData(browser, frame);

            if (userData != null)
            {
                bag = userData;
            }
        }        

        private void SetVar(IBrowser browser, IFrame frame, object userData)
        {
            var webRequestDatas = webEvents.Select(p => new WebRequestData
            {
                Code = p.StatusCode,
                ResponseContent = p.Content,
                Url = p.SourceUrl
            }).ToList();

            if (userData == null)
            {
                userData = new object();
            }

            var code = $"var {WebVar.VarName}={WebVar.VarName}||{{}};\n";

            code += $"{WebVar.VarName}.{nameof(WebVar.Bag)}={Newtonsoft.Json.JsonConvert.SerializeObject(userData)}\n";

            //code += $"{WebVar.VarName}.{nameof(WebVar.WebRequestDatas)}={Newtonsoft.Json.JsonConvert.SerializeObject(webRequestDatas)}\n";

            var ret = webBrowserTool.ExecuteScript(browser, frame, code);
        }

        private void AssertWebHasNoError()
        {
            StringBuilder sbError = new StringBuilder(), sbWarn = new StringBuilder();
            
            var failRequestList = webEvents.Where(p => !Util.IsSuccessStatusCode(p.StatusCode)).ToList();
            if (failRequestList.Any())
            {
                
                foreach (var item in failRequestList)
                {
                    if (isWarn(item.SourceUrl))
                    {
                        sbWarn.AppendLine($"{item.SourceUrl}返回错误状态:{item.StatusCode}");
                    }
                    else
                    {
                        sbError.AppendLine($"{item.SourceUrl}返回错误状态:{item.StatusCode}");
                    }
                    
                }
            }

            if (sbWarn.Length > 0)
            {
                _testResult.HasWarn = true;
                _testResult.WainMsg = sbWarn.ToString();
            }

            if (sbError.Length > 0)
            {
                throw new Exception($"请求出错：{sbError}");
            }

            bool isWarn(string url)
            {
                return warnFileExt.Any(p => url.EndsWith(p, StringComparison.OrdinalIgnoreCase) || url.ToLower().Contains($"{p}?"));
            }
        }

        private async Task<int> RunTestCode(IBrowser browser, IFrame frame)
        {
            int sleepMills = 1000;
            string lastErr = string.Empty;
            if (!string.IsNullOrWhiteSpace(_testCase.TestCode))
            {
                var tryCount = 0;

                while (true)
                {
                    if (_cancelFlag)
                    {
                        throw new Exception("任务取消");
                    }
                    AssertWebHasNoError();
                    webBrowserTool.WaitLoading(browser, _cancelFlag);
                    try
                    {
                        PrepareTest(browser, frame, bag);
                        var ret = webBrowserTool.ExecutePromiseScript(browser, frame, Util.ReplaceEvnParams(_testCase.TestCode, _testEnvParams));
                        UpdateUserVarData(browser, frame);
                        if (object.Equals(ret, false))
                        {
                            if (tryCount++ >= TestTimeOut / sleepMills)
                            {
                                PublishMsg("超时");
                                return await Task.FromResult(0);
                            }
                            Thread.Sleep(sleepMills);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (JSException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        UpdateUserVarData(browser, frame);
                        if (ex.Message != lastErr)
                        {
                            lastErr = ex.Message;
                            PublishMsg(ex.Message);
                        }

                        if (tryCount++ > TestTimeOut / sleepMills)
                        {
                            PublishMsg("超时");
                            return await Task.FromResult(0);
                        }
                        Thread.Sleep(sleepMills);
                    }
                }

            }

            return await Task.FromResult(1);

        }

        private async Task<int> RunValidCode(IBrowser browser, IFrame frame)
        {
            int validResult = 0;
            int sleepMills = 1000;
            string lastErr = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(_testCase.ValidCode))
                {
                    var tryCount = 0;

                    while (true)
                    {
                        if (_cancelFlag)
                        {
                            throw new Exception("任务取消");
                        }

                        AssertWebHasNoError();
                        PublishDebugMsg("检查请求没有异常");
                        try
                        {
                            PrepareTest(browser, frame, bag);

                            PublishDebugMsg("开始执行验证代码");
                            var ret = webBrowserTool.TryExecuteScript(browser, frame, Util.ReplaceEvnParams(_testCase.ValidCode, _testEnvParams));
                            UpdateUserVarData(browser, frame);
                            PublishDebugMsg("执行验证代码,结果:" + ret);
                            if (ret == null)
                            {
                                if (tryCount++ >= TestTimeOut / sleepMills)
                                {
                                    _testResult.IsTimeOut = true;
                                    _testResult.FailMsg = "检查结果一直为NULL";
                                    return await Task.FromResult(0);
                                }
                                Thread.Sleep(sleepMills);
                            }
                            else
                            {
                                validResult = object.Equals(ret, true) ? 1 : 0;
                                _testResult.Success = object.Equals(ret, true);
                                _testResult.FailMsg = "检查结果返回true/false";
                                break;
                            }
                        }
                        catch (JSException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            UpdateUserVarData(browser, frame);
                            if (ex.Message != lastErr)
                            {
                                lastErr = ex.Message;
                                PublishMsg("验证可能会失败：" + ex.Message);
                            }

                            if (tryCount++ >= TestTimeOut / sleepMills)
                            {
                                _testResult.IsTimeOut = true;
                                _testResult.FailMsg = "检查出错：" + ex.Message;
                                return await Task.FromResult(0);
                            }
                            Thread.Sleep(sleepMills);
                        }
                    }
                }
                else
                {
                    AssertWebHasNoError();
                    _testResult.Success = true;
                    _testResult.FailMsg = "没有验证脚本，默认为成功";

                    PublishDebugMsg("没有验证脚本，默认为成功");
                }
            }
            catch (Exception ex)
            {
                _testResult.Success = false;
                _testResult.FailMsg = ex.Message;
                PublishMsg(ex.Message);
                throw ex;
            }
            
            return await Task.FromResult(validResult);
        }

        private bool Check(IBrowser browser, IFrame frame)
        {
            var flag = true;
            if (!string.IsNullOrWhiteSpace(_testSite.CheckLoginCode))
            {
                var isLogin = webBrowserTool.TryExecuteScript(browser, frame, _testSite.CheckLoginCode);
                if (!object.Equals(isLogin, true))
                {
                    if (_testLogin == null)
                    {
                        throw new TestException("脚本检查没有登录，但是没有登录配置");
                    }
                    flag = false;
                    var loginTask = new RunTestLoginTask(_testSite.Name + "登陆", UseProxy, _testSite, _testLogin, _testEnv, _testEnvParams);
                    loginTask.SetNext(new RunTestTask(GetTaskName(), UseProxy, _testSite, _testLogin, _testPage, _testCase, _testEnv, _testEnvParams,_globScripts,_siteScripts,_notify));
                    this.SetNext(loginTask);
                }
            }
            return flag;
        }

        protected override async Task<int> ExecuteInner(IBrowser browser, IFrame frame)
        {
            if (!Check(browser, frame))
            {
                return await Task.FromResult(0);
            }

            var ret = 0;
            try
            {
                PublishDebugMsg($"{_testCase.CaseName}执行代码");
                ret = await RunTestCode(browser, frame);
                if (ret == 1)
                {
                    webBrowserTool.WaitLoading(browser, _cancelFlag);
                    PublishDebugMsg($"{_testCase.CaseName}执行代码成功，准备验证");
                    ret = await RunValidCode(browser, frame);
                    if (ret == 1)
                    {
                        PublishMsg($"{_testCase.CaseName}测试成功");
                    }
                    else
                    {
                        PublishMsg($"{_testCase.CaseName}测试失败");
                    }
                }
            }
            catch (Exception ex)
            {
                _testResult.Success = false;
                _testResult.FailMsg = ex.Message;
                PublishMsg($"{_testCase.CaseName}出错:{ex.Message}");
            }
            finally
            {
                if (bag != null)
                {
                    _testResult.ResultContent = Newtonsoft.Json.JsonConvert.SerializeObject(bag);
                }
                FinishTest();
            }
            return await Task.FromResult(ret);
        }

        private void FinishTest()
        {
            _testResult.TestEndDate = DateTime.Now;
            BigEntityTableRemotingEngine.Insert(nameof(TestResult), _testResult);

            _notify?.Invoke(_testResult);
            EventBus.NotifyTestResultAction?.Invoke(_testResult);
        }

        /// <summary>
        /// 监听底层数据方法
        /// </summary>
        /// <param name="webEvent"></param>
        /// <returns></returns>
        private bool ProcessWebEvent(WebEvent webEvent)
        {
            webEvents.Add(webEvent);
            return false;
        }

        public override string GetStartPageUrl()
        {
            var url = base.GetStartPageUrl();
            var paramlist2 =_testCaseData.Params?.Where(p => p.Checked);
            if (paramlist2?.Count() > 0)
            {
                if (url.IndexOf("?") == -1)
                {
                    url += "?";
                }

                url += string.Join("&", paramlist2.Select(p => $"{System.Net.WebUtility.UrlEncode(Util.ReplaceEvnParams(p.Name, _testEnvParams))}={System.Net.WebUtility.UrlEncode(Util.ReplaceEvnParams(p.Value, _testEnvParams))}"));
            }

            if (_testCase.AuthType == AuthType.ApiKey)
            {
                if (_testCaseData.ApiKeyAddTo != 0)
                {
                    if (url.IndexOf('?') == -1)
                    {
                        url += $"?{Util.ReplaceEvnParams(_testCaseData.ApiKeyName, _testEnvParams)}={Util.ReplaceEvnParams(_testCaseData.ApiKeyValue, _testEnvParams)}";
                    }
                    else
                    {
                        url += $"&{Util.ReplaceEvnParams(_testCaseData.ApiKeyName, _testEnvParams)}={Util.ReplaceEvnParams(_testCaseData.ApiKeyValue, _testEnvParams)}";
                    }
                }
            }

            return url;
        }

        public override IRequest GetTestRequest(IRequest request)
        {
            if (_testCaseData.Headers?.Count() > 0)
            {
                foreach (var header in _testCaseData.Headers)
                {
                    if (!header.Checked)
                    {
                        continue;
                    }

                    request.Headers.Add(Util.ReplaceEvnParams(header.Name, _testEnvParams), Util.ReplaceEvnParams(header.Value, _testEnvParams));

                }
            }

            if (_testCase.AuthType == AuthType.Bearer)
            {
                request.Headers.Add("Authorization", $"Bearer {Util.ReplaceEvnParams(_testCaseData.BearToken, _testEnvParams)}");
            }
            else if (_testCase.AuthType == AuthType.ApiKey)
            {
                if (_testCaseData.ApiKeyAddTo == 0)
                {
                    request.Headers.Add(Util.ReplaceEvnParams(_testCaseData.ApiKeyName, _testEnvParams), Util.ReplaceEvnParams(_testCaseData.ApiKeyValue, _testEnvParams));
                }
            }
            else if (_testCase.AuthType == AuthType.Basic)
            {
                request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Util.ReplaceEvnParams(_testCaseData.BasicUserName, _testEnvParams)}:{Util.ReplaceEvnParams(_testCaseData.BasicUserPwd, _testEnvParams)}"))}");
            }

            var bodydataType = _testCase.BodyDataType;

            if (bodydataType == BodyDataType.none&&_testCase.WebMethod==WebMethod.GET)
            {
                return base.GetTestRequest(request);
            }
            else
            {
                request.InitializePostData();
                byte[] data = null;
                if (bodydataType == BodyDataType.formdata)
                {
                    var dic = _testCaseData.FormDatas.Where(p => p.Checked).ToDictionary(p => Util.ReplaceEvnParams(p.Name, _testEnvParams), q => Util.ReplaceEvnParams(q.Value, _testEnvParams));
                    data =Encoding.UTF8.GetBytes(string.Join("&", dic.Select(p => $"{p.Key}={p.Value}")));
                }
                else if (bodydataType == BodyDataType.xwwwformurlencoded)
                {
                    var sdata = string.Join("&", _testCaseData.XWWWFormUrlEncoded.Where(p => p.Checked).Select(p => $"{Util.ReplaceEvnParams(p.Name, _testEnvParams)}={System.Net.WebUtility.UrlEncode(Util.ReplaceEvnParams(p.Value, _testEnvParams))}"));
                    data = Encoding.UTF8.GetBytes(sdata);
                }
                else if (bodydataType == BodyDataType.raw)
                {
                    data = Encoding.UTF8.GetBytes(Util.ReplaceEvnParams(_testCaseData.RawText, _testEnvParams));
                    request.SetHeaderByName("content-type", $"application/{_testCase.ApplicationType}",true);
                }
                else if (bodydataType == BodyDataType.wcf)
                {
                    data = Encoding.UTF8.GetBytes(Util.ReplaceEvnParams(_testCaseData.RawText, _testEnvParams));
                    request.SetHeaderByName("content-type", $"text/{_testCase.ApplicationType}", true);
                }
                else if (bodydataType == BodyDataType.binary)
                {
                    List<FormItemModel> formItems = new List<FormItemModel>();
                    foreach (var item in _testCaseData.Multipart_form_data)
                    {
                        var @value = Util.ReplaceEvnParams(item.Value, _testEnvParams);
                        var name = Util.ReplaceEvnParams(item.Name, _testEnvParams);
                        if (item.Checked)
                        {
                            if (@value?.StartsWith("[file]") == true)
                            {
                                var filename = @value.Replace("[file]", string.Empty);
                                request.PostData.AddFile(filename);
                            }
                            else if (item.Value?.StartsWith("[base64]") == true)
                            {
                                var filename = @value.Replace("[base64]", string.Empty);
                                if (!File.Exists(filename))
                                {
                                    throw new FileNotFoundException($"文件不存在;{filename}");
                                }

                                throw new NotImplementedException();
                            }
                            else
                            {
                                formItems.Add(new FormItemModel
                                {
                                    FileName = name,
                                    Key = name,
                                    Value = @value
                                });
                            }
                        }
                    }

                    if (formItems.Count > 0)
                    {
                        var sdata = string.Join("&", formItems.Select(p => $"{p.Key}={System.Net.WebUtility.UrlEncode(p.Value)}"));
                        data = Encoding.UTF8.GetBytes(sdata);
                    }
                }

                if (data != null)
                {
                    var element = request.PostData.CreatePostDataElement();
                    element.Bytes = data;
                    request.PostData.AddElement(element);
                }

                request.Url = GetStartPageUrl();
                request.Method = _testCase.WebMethod.ToString();
                return request;
            }
        }

        public override void ForceCancel(string reason)
        {
            PublishMsg($"{GetTaskName()}强制终止:{reason}");
            _testResult.TestEndDate = DateTime.Now;
            _testResult.IsTimeOut = !_readyFlag;
            _testResult.FailMsg = reason;
            FinishTest();
        }
    }
}
