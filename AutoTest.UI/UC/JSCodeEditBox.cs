using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.UC
{
    public partial class JSCodeEditBox : EditTextBox
    {
        public JSCodeEditBox() : base()
        {
            InitializeComponent();

            Dictionary<string, Color> dic = new Dictionary<string, Color>();

            dic.Add("var", Color.Blue);
            dic.Add("break", Color.Blue);
            dic.Add("while", Color.Blue);
            dic.Add("undefined", Color.Blue);
            dic.Add("window", Color.Blue);
            dic.Add("alert", Color.Red);
            dic.Add("confirm", Color.Red);
            dic.Add("function", Color.Red);
            dic.Add("return", Color.Blue);
            dic.Add("setTimeout", Color.Red);
            dic.Add("setInterval", Color.Red);
            dic.Add("escape", Color.Red);
            dic.Add("unescape", Color.Red);
            dic.Add("eval", Color.Red);
            dic.Add("isNaN", Color.Red);
            dic.Add("parseFloat", Color.Red);
            dic.Add("parseInt", Color.Red);
            dic.Add("prompt", Color.Red);
            dic.Add("join", Color.Red);
            dic.Add("reverse", Color.Red);
            dic.Add("sort", Color.Red);
            dic.Add("Date", Color.Red);
            dic.Add("getMonth", Color.Red);
            dic.Add("getDate", Color.Red);
            dic.Add("getYear", Color.Red);
            dic.Add("getDay", Color.Red);
            dic.Add("concat", Color.Red);
            dic.Add("getHours", Color.Red);
            dic.Add("getMinutes", Color.Red);
            dic.Add("getSeconds", Color.Red);
            dic.Add("getMilliseconds", Color.Red);
            dic.Add("getTime", Color.Red);
            dic.Add("getTimezoneOffset", Color.Red);
            dic.Add("big", Color.Red);
            dic.Add("blink", Color.Red);
            dic.Add("bold", Color.Red);
            dic.Add("charAt", Color.Red);
            dic.Add("fixed", Color.Red);
            dic.Add("fontcolor", Color.Red);
            dic.Add("fontsize", Color.Red);
            dic.Add("indexOf", Color.Red);
            dic.Add("italics", Color.Red);
            dic.Add("lastIndexOf", Color.Red);
            dic.Add("link", Color.Red);
            dic.Add("small", Color.Red);
            dic.Add("strike", Color.Red);
            dic.Add("sub", Color.Red);
            dic.Add("substring", Color.Red);
            dic.Add("sup", Color.Red);
            dic.Add("toLowerCase", Color.Red);
            dic.Add("toUpperCase", Color.Red);
            dic.Add("true", Color.Green);
            dic.Add("false", Color.Green);

            for (int i = 0; i < 10; i++)
            {
                dic.Add(i.ToString(), Color.Yellow);
            }

            foreach (var kv in dic)
            {
                this.KeyWords.AddKeyWord(kv.Key, kv.Value);
            }
        }
    }
}
