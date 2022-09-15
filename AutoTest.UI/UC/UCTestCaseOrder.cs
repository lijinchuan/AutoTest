using AutoTest.Domain.Entity;
using AutoTest.UI.WebBrowser;
using CefSharp;
using Newtonsoft.Json;
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
        private List<TestTask> testTasks = new List<TestTask>();
        private bool _isLoad = false;
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

        public void SetTestTasks(List<TestTask> testTasks)
        {
            this.testTasks = testTasks;
            if (CBBroswer.IsBrowserInitialized)
            {
                CBBroswer.ExecuteScriptAsync("setEles", JsonConvert.SerializeObject(testTasks.Select(p => new
                {
                    TaskName = p.GetTaskName()
                })));
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CBBroswer.MenuHandler = new MenuHandler();
            CBBroswer.FrameLoadEnd += CBBroswer_FrameLoadEnd;

            CBBroswer.LoadUrl($@"{LJC.FrameWorkV3.Comm.CommFun.GetCurrentAppForder()}HTML\DragOrder.html");
        }

        public object GetOrders()
        {
            return CBBroswer.GetBrowser().MainFrame.EvaluateScriptAsync("getEles()").Result.Result;
        }

        private void CBBroswer_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            _isLoad = true;
            SetTestTasks(this.testTasks);
        }
    }
}
