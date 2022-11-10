using AutoTest.Domain.Entity;
using AutoTest.Domain.Model;
using AutoTest.UI.EventListener;
using AutoTest.UI.WebBrowser;
using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoTest.UI.WebTask
{
    public abstract class WebTask : IWebTask
    {
        /// <summary>
        /// 任务完成事件
        /// </summary>
        public event Action<IWebTask> OnTaskCompleted;

        /// <summary>
        /// 发布消息事件
        /// </summary>
        public event Action<string> OnMsgPublish;

        /// <summary>
        /// 任务通知
        /// </summary>
        public event Action<IWebTask> OnTaskReady;

        /// <summary>
        /// 起始页
        /// </summary>
        private readonly string startPageUrl;

        /// <summary>
        /// 任务名称，用来展示
        /// </summary>
        private readonly string taskName;

        /// <summary>
        /// 是否是测试模式
        /// </summary>
        protected bool isTestMode = false;

        /// <summary>
        /// 上次发送未登录邮件的时间间隔
        /// </summary>
        protected const int lastSendNoLoginEmailTimeLimitMins = 720;
        /// <summary>
        /// 上次发送错误邮件的时间间隔
        /// </summary>
        protected const int lastSendErrorEmailTimeLimitMins = 30;

        protected IWebTask NextWebTask = null;

        /// <summary>
        /// 
        /// </summary>
        protected readonly WebBrowserTool webBrowserTool;

        protected List<WebEvent> webEvents = new List<WebEvent>();

        private bool _readyFlag = false;
        private readonly object _locker = new object();

        /// <summary>
        /// 是否使用代理
        /// </summary>
        public bool UseProxy
        {
            get;
            private set;
        }
        public bool ClearCookies 
        { 
            get; 
            private set; 
        }

        public bool IsDebug
        {
            get;
            set;
        } = false;

        protected volatile bool _cancelFlag = false;

        protected TestEnv _testEnv;
        protected List<TestEnvParam> _testEnvParams;

        protected WebTask(string strTaskName, string strStartPageUrl, bool useProxy,bool clearCookies,
            TestEnv testEnv, List<TestEnvParam> testEnvParams)
        {
            taskName = strTaskName;
            startPageUrl = strStartPageUrl;
            UseProxy = useProxy;
            ClearCookies = clearCookies;
            _testEnv = testEnv;
            _testEnvParams = testEnvParams;

            webBrowserTool = new WebBrowserTool();
        }

        public abstract void DocumentCompletedHandler(IBrowser browser, IFrame frame);

        /// <summary>
        /// 内部执行，更安全，防止不通知任务结束
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        protected abstract Task<int> ExecuteInner(IBrowser browser, IFrame frame, ICookieManager cookieManager);

        public async Task<int> Execute(IBrowser browser, IFrame frame,ICookieManager cookieManager)
        {
            try
            {
                return await ExecuteInner(browser, frame, cookieManager);
            }
            finally
            {
                FireTaskCompleted();
            }
        }

        public abstract IEnumerable<Cookie> GetCookieList();

        public abstract IEventListener GetEventListener();

        public abstract string GetSite();

        public virtual string GetStartPageUrl()
        {
            return startPageUrl;
        }

        /// <summary>
        /// 通知可以开始任务了
        /// </summary>
        protected void FireTaskReady()
        {
            lock (_locker)
            {
                if (!_readyFlag)
                {
                    _readyFlag = true;

                    OnTaskReady?.BeginInvoke(this, null, null);
                }
            }
        }

        /// <summary>
        /// 通知任务完成了
        /// </summary>
        private void FireTaskCompleted()
        {
            OnTaskCompleted?.Invoke(this);
        }

        protected void PublishMsg(string msg)
        {
            OnMsgPublish?.Invoke(msg);
        }

        protected void PublishDebugMsg(string msg)
        {
            if (IsDebug)
            {
                OnMsgPublish?.Invoke(msg);
            }
        }

        public string GetTaskName()
        {
            return taskName;
        }

        public void SetTestMode(bool bIsTestMode)
        {
            isTestMode = bIsTestMode;
        }

        public void SetNext(IWebTask webTask)
        {
            this.NextWebTask = webTask;
        }

        public IWebTask GetNext()
        {
            return NextWebTask;
        }

        protected void AddGlobalFunction(IBrowser browser, IFrame frame)
        {
            var fileName = "GlobalFunction.js";

            var jsFile = File.ReadAllText(fileName);

            webBrowserTool.ExecuteScript(browser, frame, jsFile);

        }

        public virtual void DocumentLoadStartHandler(IBrowser browser, IFrame frame)
        {
            

        }

        public virtual IRequest GetTestRequest(IRequest request)
        {
            request.Url = GetStartPageUrl();
            request.Method = "GET";
            return request;
        }

        public List<WebRequestData> GetWebRequestData(string url)
        {
            var list = webEvents.Where(p => p.SourceUrl.IndexOf(url, StringComparison.OrdinalIgnoreCase) > -1).ToList();

            return list.Select(p => new WebRequestData
            {
                Code = p.StatusCode,
                ResponseContent = p.Content,
                Url = p.SourceUrl
            }).ToList();
        }

        public void Cancel()
        {
            _cancelFlag = true;
        }

        public virtual void ForceCancel(string reason)
        {
        }

        protected void RegistTestScript(IBrowser browser, IFrame frame, TestScript testScript)
        {
            if (!testScript.Enable || string.IsNullOrWhiteSpace(testScript.Body))
            {
                return;
            }

            var body = Util.ReplaceEvnParams(testScript.Body, _testEnvParams);

            if (Regex.Match(body.TrimStart(), "^(https?:|//)", RegexOptions.IgnoreCase).Success)
            {
                webBrowserTool.RegisterRomoteScript(browser, frame, body.TrimStart());
            }
            else
            {
                webBrowserTool.ExecuteScript(browser, frame, body);
            }
        }

        public abstract TestCase GetTestCase();
    }
}
