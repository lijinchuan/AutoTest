using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Util
{
    public class ScriptKeyWord : IEqualityComparer<ScriptKeyWord>
    {
        public string KeyWord
        {
            get;
            set;
        }

        public string Desc
        {
            get;
            set;
        }

        public Color HighColor
        {
            get;
            set;
        }


        public bool Equals(ScriptKeyWord x, ScriptKeyWord y)
        {
            if (x.KeyWord == null && y.KeyWord == null)
            {
                return true;
            }

            if (x.KeyWord == null || y.KeyWord == null)
            {
                return false;
            }

            var ret = x.KeyWord.ToUpper() == y.KeyWord.ToUpper();
            if (ret)
            {
                if (string.IsNullOrEmpty(x.Desc))
                {
                    x.Desc = y.Desc;
                }
                else if (string.IsNullOrEmpty(y.Desc))
                {
                    y.Desc = x.Desc;
                }
            }
            return ret;
        }

        public int GetHashCode(ScriptKeyWord obj)
        {
            if (obj.KeyWord == null)
            {
                return 0;
            }
            return obj.KeyWord.GetHashCode();
        }
    }

    public static class ScriptKeyWordHelper
    {
        private static List<ScriptKeyWord> KeyWordDic = new List<ScriptKeyWord>();

        static ScriptKeyWordHelper()
        {
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "$1_12_4",
                Desc = "jquery引用变量",
                HighColor = Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "_ask_back_download_",
                Desc = "插入到URL中任意位置请求后端拦截下载",
                HighColor = Color.Pink
            });

            
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "$VewBag",
                Desc = "页面变量",
                HighColor=Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "$VewBag.Bag",
                Desc = "页面用户自定义变量:$VewBag.Bag,object",
                HighColor = Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "$VewBag.Bag._maxScriptExeCount",
                Desc = "页面脚本执行最多次数，默认最多600次，可修改，int类型",
                HighColor = Color.Green
            });
            //特别注意，给服务端的变量名不能以__开头，否则收不到，亲测
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "$VewBag.Bag.apiTaskResult",
                Desc = "返回API结果给后台",
                HighColor = Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "$VewBag.Bag.__apiTaskParams",
                Desc="接收API参数",
                HighColor=Color.Green
            }) ;

            //KeyWordDic.Add(new ScriptKeyWord
            //{
            //    KeyWord = "$VewBag.WebRequestDatas",
            //    Desc = "网络数据，$VewBag.WebRequestDatas,Array",
            //    HighColor = Color.Green
            //});

            //KeyWordDic.Add(new ScriptKeyWord
            //{
            //    KeyWord = "$VewBag.WebRequestDatas[?].ResponseContent",
            //    Desc = "网络数据响应内容，$VewBag.WebRequestDatas[?].ResponseContent,string",
            //    HighColor = Color.Green
            //});

            //KeyWordDic.Add(new ScriptKeyWord
            //{
            //    KeyWord = "$VewBag.WebRequestDatas[?].Url",
            //    Desc = "网络请求地址，$VewBag.WebRequestDatas[?].Url,string",
            //    HighColor = Color.Green
            //});

            //KeyWordDic.Add(new ScriptKeyWord
            //{
            //    KeyWord = "$VewBag.WebRequestDatas[?].Code",
            //    Desc = "网络请求状态码，$VewBag.WebRequestDatas[?].Code,int",
            //    HighColor = Color.Green
            //});

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "startsWith",
                Desc = "检测字符串是否以指定的前缀开始",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "endsWith",
                Desc = "测试字符串是否以指定的后缀结束",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord= "ServerDriverClient",
                Desc="后台客户端，可调用后台方法",
                HighColor = Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord= "ServerDriverClient.reload",
                Desc = "重新刷新：ServerDriverClient.reload(bool ignoreCache)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.loadUrl",
                Desc = "加载URL：ServerDriverClient.loadUrl(string url)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.ehco",
                Desc = "输出信息到控制台：ServerDriverClient.ehco(string msg)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.sendInput",
                Desc = "使用键盘输入字符串:ServerDriverClient.sendInput(string s)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.sleep",
                Desc = "休眠:ServerDriverClient.sleep(int ms)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.click",
                Desc = "点击：ServerDriverClient.click(int x,int y)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.mouseUp",
                Desc = "鼠标弹起:ServerDriverClient.mouseUp(int x,int y)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.mouseMove",
                Desc = "鼠标移动:ServerDriverClient.mouseMove(int x,int y)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.mouseDown",
                Desc = "鼠标按下:ServerDriverClient.mouseDown(int x,int y)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord= "ServerDriverClient.saveFile",
                Desc= "ServerDriverClient.SaveFile(string fileName,bool replace,string content)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.setFileValue",
                Desc = "设置file控件路径:ServerDriverClient.setFileValue(string fileName,string script)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord= "ServerDriverClient.setPageIsLoading",
                Desc = "设置页面为加载状态，以确保后台在页面刷新前不会操作:ServerDriverClient.setPageIsLoading(bool isLoading)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.setCursorPos",
                Desc = "设置鼠标位置:ServerDriverClient.setCursorPos(int x,int y)",
                HighColor = Color.Red
            });


            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.readFile",
                Desc = "ServerDriverClient.ReadFile(string fileName)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.getLastAlertMsg",
                Desc="获取页面上最后一个提示信息",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord= "ServerDriverClient.getWebRequestData",
                Desc= "获取请求数据，需要再转换为JSON:ServerDriverClient.getWebRequestData(string url)",
                HighColor=Color.Red
                
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.getWebRequestData[?].ResponseContent",
                Desc = "网络数据响应内容，$VewBag.WebRequestDatas[?].ResponseContent,string",
                HighColor = Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.getWebRequestData[?].Url",
                Desc = "网络请求地址，ServerDriverClient.getWebRequestData[?].Url,string",
                HighColor = Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.getWebRequestData[?].Code",
                Desc = "网络请求状态码，ServerDriverClient.getWebRequestData[?].Code,int",
                HighColor = Color.Green
            });
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.openWindow",
                Desc = "弹出一个窗口展示HTML，ServerDriverClient.openWindow(string title,string html)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.compareHtml",
                Desc = "比较HTML，ServerDriverClient.compareHtml(string title,string oldHtml,string newHtml)",
                HighColor = Color.Red
            });

            //long Inc(string name, long init = 0)
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.inc",
                Desc = "取自增数，ServerDriverClient.inc(string name, long init = 0)",
                HighColor = Color.Red
            });

            //long GetCounterVal(string name)
            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.getCounterVal",
                Desc = "取自增数，ServerDriverClient.getCounterVal(string name)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.postJosn",
                Desc = @"发送JSON请求，需要再转换为JSON:ServerDriverClient.postJosn(url,data),结果{code,contentType,content}, 出错情况下:{ code,error}",
                HighColor = Color.Red

            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.loadOuterHtml",
                Desc = @"使用xpath解析web:ServerDriverClient.loadOuterHtml(url,xpath)",
                HighColor = Color.Red

            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.loadOuterHtmlEx",
                Desc = @"使用xpath解析web:ServerDriverClient.loadOuterHtmlEx(url,xpath,encoding)",
                HighColor = Color.Red

            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.outerHtml",
                Desc = @"使用xpath解析web:ServerDriverClient.outerHtml(string html,string xpath)",
                HighColor = Color.Red

            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.innerText",
                Desc = @"使用xpath解析web:ServerDriverClient.innerText(html,xpath)",
                HighColor = Color.Red

            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.loadInnerText",
                Desc = @"使用xpath解析web:ServerDriverClient.loadInnerText(url,xpath)",
                HighColor = Color.Red

            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.loadInnerTextEx",
                Desc = @"使用xpath解析web:ServerDriverClient.loadInnerTextEx(url,xpath,encoding)",
                HighColor = Color.Red

            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.innerHtml",
                Desc = @"使用xpath解析web:ServerDriverClient.innerHtml(html,xpath)",
                HighColor = Color.Red

            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.loadInnerHtml",
                Desc = @"使用xpath解析web:ServerDriverClient.loadInnerHtml(url,xpath)",
                HighColor = Color.Red

            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.loadInnerHtmlEx",
                Desc = @"使用xpath解析web:ServerDriverClient.loadInnerHtmlEx(url,xpath,encoding)",
                HighColor = Color.Red

            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.requestWebResource",
                Desc = @"发送web请求，ServerDriverClient.requestWebResource(url,data,applicatonType,method),结果{code,contentType,content},出错情况下:{code,error}",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "JSON.parse",
                Desc = "将json字符串转换成json对象",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "JSON.stringify",
                Desc = "将json对象转换成json对符串",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "location",
                Desc = "location 对象包含有关当前 URL 的信息",
                HighColor = Color.Blue
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "location.host",
                Desc = "设置或返回主机名和当前 URL 的端口号",
                HighColor = Color.Blue
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "location.hostname",
                Desc = "设置或返回当前 URL 的主机名",
                HighColor = Color.Blue
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "location.href",
                Desc = "设置或返回完整的 URL",
                HighColor = Color.Blue
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "location.pathname",
                Desc = "设置或返回当前 URL 的路径部分",
                HighColor = Color.Blue
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "location.port",
                Desc = "设置或返回当前 URL 的端口号",
                HighColor = Color.Blue
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "location.protocol",
                Desc = "设置或返回当前 URL 的协议",
                HighColor = Color.Blue
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "location.search",
                Desc = "设置或返回从问号 (?) 开始的 URL（查询部分）",
                HighColor = Color.Blue
            });


            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.logInfo",
                Desc = "记录info日志：ServerDriverClient.logInfo(string content)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.logError",
                Desc = "记录error日志：ServerDriverClient.logError(string content)",
                HighColor = Color.Red
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ServerDriverClient.logDebug",
                Desc = "记录error日志：ServerDriverClient.logDebug(string content)",
                HighColor = Color.Red
            });
        }

        private static void Deal()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var kw in KeyWordDic.Distinct().OrderBy(p => p.KeyWord))
            {
                sb.AppendLine(@"KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = """ + kw.KeyWord + @""",Desc = """ + kw.Desc + @"""
            });");
            }

            System.Diagnostics.Trace.WriteLine(sb.ToString());
        }

        public static List<ScriptKeyWord> GetKeyWordList()
        {
            return KeyWordDic;
        }

    }
}
