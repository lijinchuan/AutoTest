using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser
{
    public interface IWebBrowserTool
    {
        /// <summary>
        /// 加载JQUERY库
        /// </summary>
        void AddJqueryLib(IBrowser browser, IFrame frame,bool force);

        /// <summary>
        /// 创建一个万能函数
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        void AddEvalFuntion(IBrowser browser, IFrame frame);


        /// <summary>
        /// 添加COOKE管理方法
        /// </summary>
        bool AddCookeManagerFunction(IBrowser browser, IFrame frame);

        /// <summary>
        /// 模拟横向拖动鼠标事件
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="startX">开始X位置</param>
        /// <param name="startY">开始Y位置</param>
        /// <param name="endX">结束X位置</param>
        /// <param name="endY">结束Y位置</param>
        void DragX(IBrowser browser, int startX, int startY, int endX, int endY);

        /// <summary>
        /// 执行JS代码
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        object ExecuteScript(IBrowser browser, IFrame frame, string code,int timeOut);

        object ExecutePromiseScript(IBrowser browser, IFrame frame, string code, int timeOut);
    }
}
