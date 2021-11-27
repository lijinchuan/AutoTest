using CefSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser
{
    public class WebBrowserTool : IWebBrowserTool
    {
        private static string ADDJQUERYLIBCODE = @"var script = document.createElement(""script"");
                script.type = ""text/javascript"";
                script.src = ""https://code.jquery.com/jquery-1.12.4.min.js"";//script.src=""jquery-1.12.4.min.js"";
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

        public void AddEvalFuntion(IBrowser browser, IFrame frame)
        {
            _ = browser.MainFrame.EvaluateScriptAsync(ADDEVALFUNCTIONCODE).Result;
        }

        public void AddJqueryLib(IBrowser browser, IFrame frame)
        {
            _ = browser.MainFrame.EvaluateScriptAsync(ADDJQUERYLIBCODE).Result;
            var script = @"$('#aaaaa').length";
            var start = DateTime.Now;
            while (true)
            {
                var changetabresult = browser.MainFrame.EvaluateScriptAsync(script).Result;
                //这里要确认JQUERY加载成功
                if (changetabresult.Success)
                {
                    break;
                }
                else
                {
                    if (DateTime.Now.Subtract(start).TotalSeconds > 30)
                    {
                        throw new TimeoutException("加载JQUERY类库超时");
                    }
                    Thread.Sleep(10);
                }

            }
        }

        public bool AddCookeManagerFunction(IBrowser browser, IFrame frame)
        {
            var resp = browser.MainFrame.EvaluateScriptAsync(ADDCOOKIEMANAGERFUNCTION).Result;

            return resp.Success;
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
    }
}
