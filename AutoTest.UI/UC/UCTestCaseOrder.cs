using CefSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.UC
{
    public partial class UCTestCaseOrder : UserControl
    {
        public UCTestCaseOrder()
        {
            InitializeComponent();

            //这个一定要开启，否则注入C#的对象无效
            this.CBBroswer.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;

            ////构造要注入的对象，参数为当前线程的调度上下文
            //var cSObj = new CSUCTestCaseSelector(SynchronizationContext.Current, this);
            ////注册C#对象
            //this.CBBroswer.JavascriptObjectRepository.Register("sc", cSObj, false, BindingOptions.DefaultBinder);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
    }
}
