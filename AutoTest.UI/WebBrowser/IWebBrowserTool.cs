using AutoTest.Domain;
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
        void AddJqueryLib(IBrowser browser, IFrame frame, bool force = false);

        /// <summary>
        /// 加载JQUERY库（异步）
        /// </summary>
        Task AddJqueryLibAsync(IBrowser browser, IFrame frame, bool force = false);

        /// <summary>
        /// 创建一个万能函数
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        Task AddEvalFuntion(IBrowser browser, IFrame frame);


        /// <summary>
        /// 添加COOKE管理方法
        /// </summary>
        bool AddCookeManagerFunction(IBrowser browser, IFrame frame);

        /// <summary>
        /// 添加COOKE管理方法（异步）
        /// </summary>
        Task AddCookeManagerFunctionAsync(IBrowser browser, IFrame frame);

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
        object ExecuteScript(IBrowser browser, IFrame frame, string code, int timeOut = 30000);

        /// <summary>
        /// 执行JS代码（异步）
        /// </summary>
        Task<object> ExecuteScriptAsync(IBrowser browser, IFrame frame, string code, int timeOut = 30000);

        Task<object> ExecutePromiseScript(IBrowser browser, IFrame frame, string code, int timeOut = 30000);

        Task<(double x, double y)> FindElementPosAsync(IBrowser browser, string ele);

        /// <summary>
        /// 关闭底层连接
        /// </summary>
        /// <param name="browser"></param>
        /// <returns></returns>
        bool CloseAllConnections(IBrowser browser);

        void WaitLoading(IBrowser browser, bool breakFlag, bool checkScript = false, bool checkVar = false, int timeOutMs = 120000);

        /// <summary>
        /// 等待加载完成（异步）
        /// </summary>
        Task WaitLoadingAsync(IBrowser browser, bool breakFlag, bool checkScript = false, bool checkVar = false, int timeOutMs = 120000);

        bool RegisterScript(IBrowser browser, IFrame frame, string code);

        /// <summary>
        /// 注册脚本（异步）
        /// </summary>
        Task RegisterScriptAsync(IBrowser browser, IFrame frame, string code);

        bool RegisterRomoteScript(IBrowser browser, IFrame frame, string url);

        /// <summary>
        /// 注册远程脚本（异步）
        /// </summary>
        Task RegisterRomoteScriptAsync(IBrowser browser, IFrame frame, string url);

        void EnableMenu(IBrowser browser);

        object TryExecuteScript(IBrowser browser, IFrame frame, string code, int timeOut = 30000);

        /// <summary>
        /// 尝试执行脚本（异步）
        /// </summary>
        Task<object> TryExecuteScriptAsync(IBrowser browser, IFrame frame, string code, int timeOut = 30000);

        Task<object> DevToolEvaluateScriptAsync(IBrowser browser, string code, int timeout);

        Task<object> DevEvaluateScriptAsPromiseAsync(IBrowser browser, string code, int timeout);

        /// <summary>
        /// 判断是否正在加载（异步）
        /// </summary>
        Task<bool> IsLoadingAsync(IBrowser browser, bool checkVar = false);

        /// <summary>
        /// 判断脚本是否繁忙（异步）
        /// </summary>
        Task<bool> IsScriptBusyAsync(IBrowser browser);

        /// <summary>
        /// 释放指定 Browser 缓存的 DevToolsClient。Browser 关闭时调用以释放资源。
        /// </summary>
        void ReleaseDevToolsClient(IBrowser browser);
    }
}
