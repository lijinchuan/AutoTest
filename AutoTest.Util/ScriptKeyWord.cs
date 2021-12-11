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
            internal set;
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
                KeyWord = "$VewBag",
                Desc = "页面变量",
                HighColor=Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "Bag",
                Desc = "页面用户自定义变量:$VewBag.Bag,object",
                HighColor = Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "WebRequestDatas",
                Desc = "网络数据，$VewBag.WebRequestDatas,Array",
                HighColor = Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "ResponseContent",
                Desc = "网络数据响应内容，$VewBag.WebRequestDatas[].ResponseContent,string",
                HighColor = Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "Url",
                Desc = "网络请求地址，$VewBag.WebRequestDatas[].Url,string",
                HighColor = Color.Green
            });

            KeyWordDic.Add(new ScriptKeyWord
            {
                KeyWord = "Code",
                Desc = "网络请求状态码，$VewBag.WebRequestDatas[].Code,int",
                HighColor = Color.Green
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
