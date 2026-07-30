using AutoTest.Domain;
using AutoTest.Domain.Exceptions;
using CefSharp;
using CefSharp.DevTools;
using CefSharp.DevTools.IO;
using CefSharp.DevTools.Runtime;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser
{
    public class WebBrowserTool : IWebBrowserTool
    {
        /// <summary>
        /// 静态缓存: 每个 Browser 复用一个 DevToolsClient，避免频繁创建/销毁
        /// Key: browser.Identifier
        /// </summary>
        private static readonly ConcurrentDictionary<int, DevToolsClient> _devToolsClients =
            new ConcurrentDictionary<int, DevToolsClient>();

        /// <summary>
        /// 获取或创建指定 Browser 的 DevToolsClient（缓存复用）
        /// </summary>
        private static DevToolsClient GetOrCreateDevToolsClient(IBrowser browser)
        {
            if (browser.IsDisposed)
                throw new ObjectDisposedException(nameof(browser));

            return _devToolsClients.GetOrAdd(browser.Identifier, _ => browser.GetDevToolsClient());
        }

        /// <summary>
        /// 释放指定 Browser 的 DevToolsClient 并从缓存移除。
        /// Browser 关闭/Dispose 时调用，通常由 DefaultChromiumWebBrowser.Dispose() 触发。
        /// </summary>
        public void ReleaseDevToolsClient(IBrowser browser)
        {
            if (browser == null) return;

            if (_devToolsClients.TryRemove(browser.Identifier, out var client))
            {
                try
                {
                    client.Dispose();
                }
                catch
                {
                    // DevToolsClient 可能已处于不可用状态，忽略释放异常
                }
            }
        }

        //private static string ADDJQUERYLIBCODE = @"if(typeof jQuery == 'undefined'){
        //        var script = document.createElement(""script"");
        //        script.type = ""text/javascript"";
        //        script.src = ""https://code.jquery.com/jquery-1.12.4.min.js"";//script.src=""jquery-1.12.4.min.js"";
        //        document.getElementsByTagName('head')[0].appendChild(script);
        //        var script2 = document.createElement(""script"");
        //        script2.type = ""text/javascript"";
        //        script2.text = ""var $jq=jQuery.noConflict(true);"";
        //        document.getElementsByTagName('head')[0].appendChild(script2);
        //       }else{
        //           var $jq=jQuery;
        //       }";

        private static readonly string ADDJQUERYLIBCODE = File.ReadAllText("jquery-1.12.4.min.js", Encoding.UTF8);

        private static string REGISTERREMOTESCRIPTCODE = @"var script = document.createElement(""script"");
                script.type = ""text/javascript"";
                script.src = ""{0}"";
                document.getElementsByTagName('head')[0].appendChild(script);";

        private static string REGISTERRSCRIPTCODE = @"var script = document.createElement(""script"");
                script.type = ""text/javascript"";
                script.text = ""{0}"";
                document.getElementsByTagName('head')[0].appendChild(script);";

        private const string ADDEVALFUNCTIONCODE = @"var script = document.createElement(""script"");
                script.type = ""text/javascript"";
                script.text = ""function _$eval(code) { eval('_r_='+code); return _r_;}"";
                document.getElementsByTagName('head')[0].appendChild(script);";

        /// <summary>
        /// 添加COOKIE管理
        /// </summary>
        private const string ADDCOOKIEMANAGERFUNCTION = @"var script = document.createElement(""script"");
                script.type = ""text/javascript"";
                script.text = ""function setCookie(key,path,value,t){\
                                        var oDate=new Date();\
                                        oDate.setDate(oDate.getDate()+t);\
                                        document.cookie=key+'='+value+'; path='+path+'; expires='+oDate.toDateString();\
                                };\
                                function getCookie(key){\
                                        var arr1 = document.cookie.split('; ');\
                                        for (var i = 0; i < arr1.length; i++)\
                                        {\
                                                var arr2 = arr1[i].split('=');\
                                                if (arr2[0] == key)\
                                                {\
                                                    return decodeURI(arr2[1]);\
                                                }\
                                        }\
                                };\
                                function removeCookie(key,path){\
                                        setCookie(key,path, '', -1); \
                                }"";
                                document.getElementsByTagName('head')[0].appendChild(script);";

        private const int SCRIPT_TIMEOUT = 30000;

        private const string getBoundingClientRect = @"
            var element=$1_12_4({0})[0];
            if(!element)
                return {{x:-1,y:-1}};
            var rect = element.getBoundingClientRect();
            var x = rect.x, y = rect.y;
            while (element) {{
                x += (element.offsetLeft - element.scrollLeft + element.clientLeft);
                y += (element.offsetTop - element.scrollTop + element.clientTop);
                element = element.offsetParent;
            }}
            return {{ x:x, y:y }};
        ";

        private const string getElementScreenCoordinates = @"
            var element=$1_12_4({0})[0];
            if(!element)
                return {{x:-1,y:-1}};
            var rect = element.getBoundingClientRect();
            var x = rect.x, y = rect.y;
            while (element) {{
                x += (element.offsetLeft - element.scrollLeft + element.clientLeft);
                y += (element.offsetTop - element.scrollTop + element.clientTop);
                element = element.offsetParent;
            }}
            return {{ x:(window.screenLeft?window.screenLeft: window.screenX)+x, y:(window.screenTop?window.screenTop: window.screenY)+y }};
        ";

        public async Task AddEvalFuntion(IBrowser browser, IFrame frame)
        {
            //var resp = browser.EvaluateScriptAsync(ADDEVALFUNCTIONCODE);
            //Task.WaitAll(new[] { resp }, SCRIPT_TIMEOUT);

            await DevToolEvaluateScriptAsync(browser, ADDEVALFUNCTIONCODE);
        }

        private void AssertJavaScriptResult(Task<JavascriptResponse> resp, int timeOut = 0)
        {
            if (timeOut <= 0)
            {
                timeOut = SCRIPT_TIMEOUT;
            }
            if (!Task.WaitAll(new[] { resp }, timeOut))
            {
                throw new TimeoutException("执行代码超时，请检查代码是否有问题(例如，返回了DOM对象，而非JSON对象)");
            }

            if (!resp.Result.Success)
            {
                if (resp.Result.Message.IndexOf("SyntaxError", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    throw new JSSyntaxError(resp.Result.Message);
                }
                throw new ScriptException(resp.Result.Message);
            }
        }

        // ========== 旧同步方法（保留向后兼容） ==========

        public void AddJqueryLib(IBrowser browser, IFrame frame, bool force = false)
        {
            AddJqueryLibAsync(browser, frame, force).GetAwaiter().GetResult();
        }

        public bool AddCookeManagerFunction(IBrowser browser, IFrame frame)
        {
            AddCookeManagerFunctionAsync(browser, frame).GetAwaiter().GetResult();
            return true;
        }

        public bool RegisterRomoteScript(IBrowser browser, IFrame frame, string url)
        {
            RegisterRomoteScriptAsync(browser, frame, url).GetAwaiter().GetResult();
            return true;
        }

        public bool RegisterScript(IBrowser browser, IFrame frame, string code)
        {
            RegisterScriptAsync(browser, frame, code).GetAwaiter().GetResult();
            return true;
        }

        public object ExecuteScript(IBrowser browser, IFrame frame, string code, int timeOut = SCRIPT_TIMEOUT)
        {
            return ExecuteScriptAsync(browser, frame, code, timeOut).GetAwaiter().GetResult();
        }

        public object TryExecuteScript(IBrowser browser, IFrame frame, string code, int timeOut = SCRIPT_TIMEOUT)
        {
            return TryExecuteScriptAsync(browser, frame, code, timeOut).GetAwaiter().GetResult();
        }

        public void WaitLoading(IBrowser browser, bool breakFlag, bool checkScript = false, bool checkVar = false, int timeOutMs = 120000)
        {
            WaitLoadingAsync(browser, breakFlag, checkScript, checkVar, timeOutMs).GetAwaiter().GetResult();
        }

        // ========== 新异步方法 ==========

        public async Task AddJqueryLibAsync(IBrowser browser, IFrame frame, bool force = false)
        {
            if (!force)
            {
                await DevToolEvaluateScriptAsync(browser, ADDJQUERYLIBCODE);
            }
            else
            {
                await DevToolEvaluateScriptAsync(browser, ADDJQUERYLIBCODE.Replace("if (typeof jQuery === 'undefined') {", "if (true) {"));
            }
        }

        public async Task AddCookeManagerFunctionAsync(IBrowser browser, IFrame frame)
        {
            await DevToolEvaluateScriptAsync(browser, ADDCOOKIEMANAGERFUNCTION);
        }

        public async Task RegisterRomoteScriptAsync(IBrowser browser, IFrame frame, string url)
        {
            var code = string.Format(REGISTERREMOTESCRIPTCODE, url);
            await DevToolEvaluateScriptAsync(browser, code);
        }

        public async Task RegisterScriptAsync(IBrowser browser, IFrame frame, string code)
        {
            code = string.Format(REGISTERRSCRIPTCODE, code.Replace("\"", "\\\""));
            await DevToolEvaluateScriptAsync(browser, code);
        }

        public async Task<object> ExecuteScriptAsync(IBrowser browser, IFrame frame, string code, int timeOut = SCRIPT_TIMEOUT)
        {
            return await DevToolEvaluateScriptAsync(browser, code, timeOut);
        }

        public async Task<object> ExecutePromiseScript(IBrowser browser, IFrame frame, string code, int timeOut = SCRIPT_TIMEOUT)
        {
            return await DevEvaluateScriptAsPromiseAsync(browser, code, timeOut);
        }

        public static bool IsPromiseScript(string code)
        {
            return Regex.IsMatch(code, @"([^\w]|^)return([\r\n\s]+|$)", RegexOptions.IgnoreCase);
        }

        public async Task<object> TryExecuteScriptAsync(IBrowser browser, IFrame frame, string code, int timeOut = SCRIPT_TIMEOUT)
        {
            if (IsPromiseScript(code))
            {
                return await DevEvaluateScriptAsPromiseAsync(browser, code, timeOut);
            }

            return await DevToolEvaluateScriptAsync(browser, code, timeOut);
        }


        public void DragX(IBrowser browser, int startX, int startY, int endX, int endY)
        {
            var host = browser.GetHost();

            var mouseDown = new MouseEvent(startX, startY, CefEventFlags.LeftMouseButton);
            host.SendMouseClickEvent(mouseDown, MouseButtonType.Left, false, 1);

            int x = startX, y = startY, dx = 1, dy = 0, len = Math.Abs(endX - startX);

            while (true)
            {
                var mx = x + dx;
                var my = y + dy;

                var mouseMove = new MouseEvent(mx, my, CefEventFlags.LeftMouseButton);
                host.SendMouseMoveEvent(mouseMove, false);

                if (mx - x > len)
                {
                    var mouseUp = new MouseEvent(endX, endY, CefEventFlags.LeftMouseButton);
                    host.SendMouseClickEvent(mouseUp, MouseButtonType.Left, true, 1);

                    break;
                }
                else
                {
                    var random = new Random(Guid.NewGuid().GetHashCode());
                    dx += random.Next(10, 50);
                    dy = random.Next(-10, 10);
                }
                Task.Delay(30).Wait();
            }
        }

        public async Task<(double x, double y)> FindElementPosAsync(IBrowser browser, string ele)
        {
            var resp = await browser.MainFrame.EvaluateScriptAsPromiseAsync(string.Format(getBoundingClientRect, ele));
            var rect = (dynamic)resp.Result;
            if (rect == null)
            {
                return (-1, -1);
            }

            return (rect.x, rect.y);
        }

        public async Task<bool> IsLoadingAsync(IBrowser browser, bool checkVar = false)
        {
            if (browser.IsDisposed)
            {
                return false;
            }

            if (browser.IsLoading)
            {
                return true;
            }

            if (checkVar)
            {
                var code = $"if(typeof {CSObj.LoadVar} === 'undefined' || {CSObj.LoadVar} === false) return false; return true;";

                try
                {
                    var result = await DevEvaluateScriptAsPromiseAsync(browser, code);

                    if (result is bool)
                    {
                        return (bool)result;
                    }

                    // result为null表示执行上下文在脚本求值期间被销毁（页面跳转/刷新），视为仍在加载
                    if (result == null)
                    {
                        return true;
                    }
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
                catch (Exception) when (browser.IsDisposed)
                {
                    return false;
                }
                catch (Exception ex) when (ex.Message.Contains("Cannot find context"))
                {
                    // EvaluateAsync阶段就报上下文丢失，页面正在跳转或刷新，视为仍在加载中
                    return true;
                }
            }

            return false;
        }

        public async Task WaitLoadingAsync(IBrowser browser, bool breakFlag, bool checkScript = false, bool checkVar = false, int timeOutMs = 120000)
        {
            int pollingDelay = checkVar ? 500 : 200;
            int ms = 0;

            while (await IsLoadingAsync(browser, checkVar) && !breakFlag)
            {
                if (browser.IsDisposed)
                {
                    return;
                }

                await Task.Delay(pollingDelay);
                ms += pollingDelay;
                if (ms > timeOutMs)
                {
                    throw new TimeoutException($"{browser.MainFrame.Url}加载超时");
                }
            }

            while (checkScript && await IsScriptBusyAsync(browser) && !breakFlag)
            {
                if (browser.IsDisposed)
                {
                    return;
                }

                await Task.Delay(200);
                ms += 200;
                if (ms > timeOutMs)
                {
                    throw new TimeoutException($"{browser.MainFrame.Url}页面脚本超时");
                }
            }
        }

        public void EnableMenu(IBrowser browser)
        {
            _ = browser.EvaluateScriptAsPromiseAsync(@"document.oncontextmenu = function(evt) { evt.returnValue = true;}");

        }

        public void DisableMenu(IBrowser browser)
        {
            _ = browser.EvaluateScriptAsPromiseAsync(@"document.oncontextmenu = function (evt) {  evt.preventDefault();};");

        }

        public async Task<bool> IsScriptBusyAsync(IBrowser browser)
        {
            if (browser.IsDisposed)
            {
                return false;
            }

            try
            {
                var ret = await DevEvaluateScriptAsPromiseAsync(browser, "console.log('IsScriptBusy check');return 1;");

                if (1.Equals(ret))
                {
                    return false;
                }

                // ret为null表示执行上下文被销毁，视为脚本忙
                if (ret == null)
                {
                    return true;
                }
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (Exception) when (browser.IsDisposed)
            {
                return false;
            }
            catch (Exception ex) when (ex.Message.Contains("Cannot find context"))
            {
                // EvaluateAsync阶段就报上下文丢失，页面正在跳转或刷新，视为脚本忙
                return true;
            }

            return true;
        }

        /// <summary>
        /// 关闭所有的底层连接
        /// </summary>
        /// <param name="browser"></param>
        /// <returns></returns>
        public bool CloseAllConnections(IBrowser browser)
        {
            var boo = false;

            var callBack = new TaskCompletionCallback();
            browser.GetHost().RequestContext.CloseAllConnections(callBack);
            callBack.Task.Wait();
            boo = callBack.Task.Result;

            return boo;
        }

        #region
        /// <summary>
        ///
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="code"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="ScriptException"></exception>
        public async Task<object> DevToolEvaluateScriptAsync(IBrowser browser, string code, int timeout = SCRIPT_TIMEOUT)
        {
            var client = GetOrCreateDevToolsClient(browser);
            var result = await client.Runtime.EvaluateAsync(code, timeout: timeout);

            if (result.ExceptionDetails != null)
            {
                throw new ScriptException(result.ExceptionDetails.ToString());
            }

            return result?.Result?.Value;
        }

        public async Task<object> DevEvaluateScriptAsPromiseAsync(IBrowser browser, string code, int timeout = SCRIPT_TIMEOUT)
        {
            var client = GetOrCreateDevToolsClient(browser);
            var resp = await client.Runtime.EvaluateAsync($"(async function(){{{code}}})()", timeout: timeout);

            if (resp.ExceptionDetails != null)
            {
                if (resp.ExceptionDetails.Exception != null)
                {
                    throw new Exception(resp.ExceptionDetails.Exception.Description);
                }

                throw new ScriptException(resp.ExceptionDetails.Text);
            }

            try
            {
                var resp2 = await client.Runtime.AwaitPromiseAsync(resp.Result.ObjectId);
                if (resp2.ExceptionDetails != null)
                {
                    throw new ScriptException(resp2.ExceptionDetails.Text);
                }

                return resp2.Result?.Value;
            }
            catch (Exception ex) when (ex.Message.Contains("Cannot find context"))
            {
                // EvaluateAsync成功后，AwaitPromiseAsync之前页面发生了跳转/刷新，执行上下文被销毁
                return null;
            }
        }
        #endregion
    }
}
