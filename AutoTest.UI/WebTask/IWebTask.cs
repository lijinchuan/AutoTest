using AutoTest.Domain.Model;
using AutoTest.UI.EventListener;
using AutoTest.UI.WebBrowser;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebTask
{
    public interface IWebTask
    {
        /// <summary>
        /// 准备好了，可以执行任务通知
        /// </summary>
        event Action<IWebTask> OnTaskReady;

        /// <summary>
        /// 调用方注册任务完成通知
        /// </summary>
        event Action<IWebTask> OnTaskCompleted;

        /// <summary>
        /// 消息输出事件
        /// </summary>
        event Action<string> OnMsgPublish;

        bool UseProxy
        {
            get;
        }

        bool ClearCookies
        {
            get;
        }

        bool IsDebug
        {
            get;
            set;
        }

        /// <summary>
        /// 任务名称，防止重复
        /// </summary>
        string GetTaskName();

        /// <summary>
        /// 返回站点地址，同一个站点不同的任务也要用同一个站点，用来标记网站的COOKIE
        /// </summary>
        /// <returns></returns>
        string GetSite();

        /// <summary>
        /// 起始页面，作一些初始化的任务
        /// </summary>
        /// <returns></returns>
        string GetStartPageUrl();

        /// <summary>
        /// 构造一个测试请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        IRequest GetTestRequest(IRequest request);

        /// <summary>
        /// 从数据库拿网站的COOKIE
        /// </summary>
        /// <returns></returns>
        IEnumerable<CefSharp.Cookie> GetCookieList();

        /// <summary>
        /// 事件监听，比如监听请求数据，返回数据
        /// </summary>
        /// <returns></returns>
        IEventListener GetEventListener();

        /// <summary>
        /// 执行方法体，不要等待，完了会通知
        /// </summary>
        Task<int> Execute(IBrowser browser, IFrame frame, ICookieManager cookieManager);

        /// <summary>
        /// 页面加载完成通知，主要用来初始化起始页，登录拿COOKIE等作用
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <param name="cookies"></param>
        void DocumentCompletedHandler(IBrowser browser, IFrame frame);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        void DocumentLoadStartHandler(IBrowser browser, IFrame frame);

        /// <summary>
        /// 设置当前是测试模式
        /// </summary>
        void SetTestMode(bool isTestMode);

        IWebTask GetNext();

        List<WebRequestData> GetWebRequestData(string url);

        void Cancel();

        void ForceCancel(string reason);
    }
}