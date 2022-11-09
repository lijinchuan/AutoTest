using AutoTest.Domain;
using AutoTest.Domain.Exceptions;
using CefSharp;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser
{
    public class WebBrowserTool : IWebBrowserTool
    {
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

        public void AddEvalFuntion(IBrowser browser, IFrame frame)
        {
            var resp = browser.MainFrame.EvaluateScriptAsync(ADDEVALFUNCTIONCODE);
            Task.WaitAll(new[] { resp }, SCRIPT_TIMEOUT);
        }

        private void AssertJavaScriptResult(Task<JavascriptResponse> resp,int timeOut=0)
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
                if (resp.Result.Message.IndexOf("SyntaxError", StringComparison.OrdinalIgnoreCase)>-1)
                {
                    throw new JSSyntaxError(resp.Result.Message);
                }
                throw new ScriptException(resp.Result.Message);
            }
        }

        public void AddJqueryLib(IBrowser browser, IFrame frame,bool force=false)
        {
            if (!force)
            {
                var resp = browser.MainFrame.EvaluateScriptAsync(ADDJQUERYLIBCODE);
                AssertJavaScriptResult(resp);
            }
            else
            {
                var resp = browser.MainFrame.EvaluateScriptAsync(ADDJQUERYLIBCODE.Replace("if (typeof jQuery === 'undefined') {", "if (true) {"));
                AssertJavaScriptResult(resp);
            }
        }

        public bool AddCookeManagerFunction(IBrowser browser, IFrame frame)
        {
            var resp = browser.MainFrame.EvaluateScriptAsync(ADDCOOKIEMANAGERFUNCTION);
            AssertJavaScriptResult(resp);
            return resp.Result.Success;
        }

        public bool RegisterRomoteScript(IBrowser browser,IFrame frame,string url)
        {
            var code = string.Format(REGISTERREMOTESCRIPTCODE, url);
            var resp = browser.MainFrame.EvaluateScriptAsync(code);
            AssertJavaScriptResult(resp);
            return resp.Result.Success;
        }

        public bool RegisterScript(IBrowser browser, IFrame frame, string code)
        {
            code = string.Format(REGISTERRSCRIPTCODE, code.Replace("\"","\\\""));
            var resp = browser.MainFrame.EvaluateScriptAsync(code);
            AssertJavaScriptResult(resp);
            return resp.Result.Success;
        }

        public object ExecuteScript(IBrowser browser, IFrame frame, string code, int timeOut = SCRIPT_TIMEOUT)
        {
            var resp = browser.MainFrame.EvaluateScriptAsync(code);
            AssertJavaScriptResult(resp, timeOut);
            return resp.Result.Result;
        }

        public object ExecutePromiseScript(IBrowser browser, IFrame frame, string code, int timeOut = SCRIPT_TIMEOUT)
        {
            var resp = browser.MainFrame.EvaluateScriptAsPromiseAsync(code);
            AssertJavaScriptResult(resp, timeOut);
            return resp.Result.Result;
        }

        public static bool IsPromiseScript(string code)
        {
            return Regex.IsMatch(code, @"([^\w]|^)return([\r\n\s]+|$)", RegexOptions.IgnoreCase);
        }

        public object TryExecuteScript(IBrowser browser, IFrame frame, string code, int timeOut = SCRIPT_TIMEOUT)
        {
            if (IsPromiseScript(code))
            {
                return ExecutePromiseScript(browser, frame, code, timeOut);
            }

            return ExecuteScript(browser, frame, code);
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

        public void WaitLoading(IBrowser browser, bool breakFlag, bool checkScript = false, int timeOutMs = 120000)
        {
            int ms = 0;
            while (browser.IsLoading && !breakFlag)
            {
                Thread.Sleep(10);
                ms += 10;
                if (ms > timeOutMs)
                {
                    throw new TimeoutException($"{browser.MainFrame.Url}加载超时");
                }
            }
            while (checkScript && IsScriptBusy(browser) && !breakFlag)
            {
                ms += 100;
                if (ms > timeOutMs)
                {
                    throw new TimeoutException($"{browser.MainFrame.Url}页面脚本超时");
                }
            }
        }

        public bool IsScriptBusy(IBrowser browser)
        {
            var resp = browser.MainFrame.EvaluateScriptAsPromiseAsync("return 1");

            if(Task.WaitAll(new[] { resp }, 100))
            {
                return false;
            }

            return true;
        }
    }
}
