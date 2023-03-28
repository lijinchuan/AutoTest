using AutoTest.UI.EventListener;
using AutoTest.UI.ResourceHandler;
using AutoTest.UI.WebTask;
using AutoTest.Util;
using CefSharp;
using CefSharp.WinForms;
using LJC.FrameWorkV3.LogManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.WebBrowser
{
    /// <summary>
    /// 默认浏览器
    /// </summary>
    public class DefaultChromiumWebBrowser : ChromiumWebBrowser
    {
        /// <summary>
        /// 开始下载网页
        /// </summary>
        private event Action<IBrowser, IFrame> DocumentLoadStart;

        /// <summary>
        ///文档下载完成事件通知 
        /// </summary>
        private event Action<IBrowser,IFrame> DocumentLoadCompleted;

        /// <summary>
        /// 待执行任务列表
        /// </summary>
        private readonly ConcurrentQueue<IWebTask> webTaskList = new ConcurrentQueue<IWebTask>();

        /// <summary>
        /// 待执行任务列表hash，优化排重性能
        /// </summary>
        private readonly HashSet<string> webTaskListHash = new HashSet<string>();

        /// <summary>
        /// 事件处理链
        /// </summary>
        private readonly List<IEventListener> eventListeners = new List<IEventListener>();

        /// <summary>
        /// 是否初始化了，对只能进行一次的操作作控制
        /// </summary>
        private volatile bool isInit = false;

        /// <summary>
        /// 是否是在执行任务状态
        /// </summary>
        private volatile bool isRunningJob = false;

        /// <summary>
        /// 发消息事件
        /// </summary>
        public event Action<string> OnMsgPublished;

        /// <summary>
        /// 等待通知准备好
        /// </summary>
        private readonly AutoResetEvent readyResetEvent = new AutoResetEvent(true);

        /// <summary>
        /// 局部锁
        /// </summary>
        private readonly object localLocker = new object();

        private static SilenceJsDialogHandler silenceJsDialogHandler = new SilenceJsDialogHandler();

        private IDialogHandler dialogHandler = null;

        private CSObj cSObj = null;

        public event Action<IWebTask> OnTaskStart;

        private volatile bool cancelFlag = false;

        private IWebTask lastTask = null;

        readonly IWebBrowserTool webBrowserTool = AutofacBuilder.GetFromFac<IWebBrowserTool>();

        public bool CancelTasks()
        {
            if (isRunningJob && !cancelFlag)
            {
                ClearTask();
                readyResetEvent.Set();
                cancelFlag = true;

                if (lastTask != null)
                {
                    lastTask.Cancel();
                }
            }
            return true;
        }

        static DefaultChromiumWebBrowser()
        {
            if (!Cef.IsInitialized)
            {
                var settings = new CefSettings();
                settings.LogSeverity = LogSeverity.Warning;
                //settings.CefCommandLineArgs.Add("--js-flags", $"--max_old_space_size=2048");
                Cef.Initialize(settings);
            }
        }


        public DefaultChromiumWebBrowser(string name, string address)
            : base(address)
        {
            Name = name;
            var setting = new RequestContextSettings()
            {
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CefSharp\\Cache_" + name),
                PersistSessionCookies = true,
                PersistUserPreferences = true,
            };
            var context = new RequestContext(setting);
            RequestContext = context;

            dialogHandler = new CommonFileDialogHandler();
            DialogHandler = dialogHandler;
            LifeSpanHandler = new DefLifeSpanHandler();

            JsDialogHandler = silenceJsDialogHandler;
            
            silenceJsDialogHandler.OnAlert += WebTask_OnMsgPublish;

            FrameLoadStart += DefaultChromiumWebBrowser_FrameLoadStart;
            FrameLoadEnd += DefaultChromiumWebBrowser_FrameLoadEnd;
            LoadingStateChanged += DefaultChromiumWebBrowser_LoadingStateChanged;

            IsBrowserInitializedChanged += DefaultChromiumWebBrowser_IsBrowserInitializedChanged;

            RequestHandler = new DefaultRequestHandler();

            //这个一定要开启，否则注入C#的对象无效
            JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            //构造要注入的对象，参数为当前线程的调度上下文
            cSObj = new CSObj(SynchronizationContext.Current, this);
            cSObj.OnPublishMsg += this.WebTask_OnMsgPublish;
            //注册C#对象
            this.JavascriptObjectRepository.Register("ServerDriverClient", cSObj, false, BindingOptions.DefaultBinder);

        }

        private void DefaultChromiumWebBrowser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            

            //DocumentLoadStart?.BeginInvoke(e.Browser, e.Frame, null, null);
        }

        private void DefaultChromiumWebBrowser_IsBrowserInitializedChanged(object sender, EventArgs e)
        {
            if (IsBrowserInitialized)
            {
                if (!isInit)
                {
                    MenuHandler = new MenuHandler().Init(this);
                    isInit = true;
                }
            }
        }

        public async Task<List<Cookie>> GetCookies(string url)
        {
            var cookieManager = this.GetCookieManager();
            using (CookieVisitor visitor = new CookieVisitor(cookieManager))
            {
                return await visitor.GetCookies(url);
            }
        }

        private void DefaultChromiumWebBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            // Method intentionally left empty.
            if (DocumentLoadCompleted != null)
            {
                _ = new Action(() =>
                {
                    webBrowserTool.WaitLoading(GetBrowser(), cancelFlag, false);
                    DocumentLoadCompleted?.Invoke(e.Browser, e.Frame);
                }).BeginInvoke(null, null);
            }
        }

        private void DefaultChromiumWebBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            // Method intentionally left empty.
        }


        /// <summary>
        /// 添加事件处理器
        /// </summary>
        /// <param name="eventListener">事件处理器实例</param>
        public void AddEventListener(IEventListener eventListener)
        {
            lock (eventListeners)
            {
                if (!eventListeners.Any(p => p.GetEventName() == eventListener.GetEventName()))
                {
                    eventListeners.Add(eventListener);
                }

            }
        }

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        /// <param name="eventListener"></param>
        public void RemoveEventListener(IEventListener eventListener)
        {
            lock (eventListeners)
            {
                _ = eventListeners.Remove(eventListener);
            }
        }

        /// <summary>
        /// 获取事件处理
        /// </summary>
        /// <returns></returns>
        public List<IEventListener> GetEventListeners()
        {
            return eventListeners.Select(p => p).ToList();
        }

        private void Clear(IWebTask webTask)
        {
            webTask.OnMsgPublish -= WebTask_OnMsgPublish;
            webTask.OnTaskReady -= WebTask_OnTaskReady;
            DocumentLoadCompleted -= webTask.DocumentCompletedHandler;
            DocumentLoadStart -= webTask.DocumentLoadStartHandler;
            lock (webTaskList)
            {
                _ = webTaskListHash.Remove(webTask.GetTaskName());
            }
            RemoveEventListener(webTask.GetEventListener());
            if (webTask.ClearCookies)
            {
                //删除所有的COOKIE
                _ = ClearCookie(webTask.GetStartPageUrl());
            }
        }

        public List<IWebTask> GetTaskList()
        {
            return webTaskList.ToList();
        }

        public bool ClearCookie(string url)
        {
            return this.GetCookieManager().DeleteCookies(url);
        }

        public bool AddTask(IWebTask webTask, bool priority = false)
        {
            lock (webTaskList)
            {
                if (!webTaskListHash.Contains(webTask.GetTaskName()))
                {
                    _ = webTaskListHash.Add(webTask.GetTaskName());
                    if (priority)
                    {
                        webTaskList.ToList().Insert(0, webTask);
                    }
                    else
                    {
                        webTaskList.Enqueue(webTask);
                    }
                    LogHelper.Instance.Debug("添加任务成功:" + webTask.GetTaskName());
                    return true;
                }
                else
                {
                    LogHelper.Instance.Debug("webTaskListHash存在任务名称，已过滤:" + webTask.GetTaskName());
                }
            }

            return false;
        }

        public bool ClearTask()
        {
            lock (webTaskList)
            {
                while (webTaskList.TryDequeue(out IWebTask tempTask))
                {
                    webTaskListHash.Remove(tempTask.GetTaskName());
                }
            }

            return false;
        }

        public IWebTask GetOne()
        {
            IWebTask task;
            lock (webTaskList)
            {
                if (!webTaskList.TryDequeue(out task))
                {
                    task = null;
                }
            }
            return task;
        }

        public void InsertOne(IWebTask insertOne,int pos)
        {
            lock (webTaskList)
            {
                webTaskList.ToList().Insert(pos, insertOne);

                List<IWebTask> list = new List<IWebTask>();
                list.Add(insertOne);
                while (true)
                {
                    IWebTask webTask1;
                    if (!webTaskList.TryDequeue(out webTask1))
                    {
                        break;
                    }
                    list.Add(webTask1);
                }

                foreach (var item in list)
                {
                    webTaskList.Enqueue(item);
                }
            }
        }

        public bool SetCookie(string url, Cookie cookie)
        {
            var cookmanager = this.GetCookieManager();
            return cookmanager.SetCookie(url, cookie);
        }

        private void PepareTask(IWebTask webTask)
        {
            //第一步，更新COOKIE
            var cooklist = webTask.GetCookieList();
            if (cooklist.Any())
            {
                var cookmanager = this.GetCookieManager();
                var cookiecontainer = GetCookies(webTask.GetStartPageUrl()).Result;
                foreach (var c in cooklist)
                {
                    var oldcookie = cookiecontainer.FirstOrDefault(p => p.Name == c.Name && p.Domain == c.Domain);
                    if (oldcookie != null && oldcookie.Value != c.Value)
                    {
                        _ = cookmanager.DeleteCookies(webTask.GetSite(), c.Name);
                    }
                    if (oldcookie == null || oldcookie.Value != c.Value)
                    {
                        _ = cookmanager.SetCookie(webTask.GetSite(), new CefSharp.Cookie()
                        {
                            Domain = c.Domain,
                            Name = c.Name,
                            Value = c.Value,
                            Expires = DateTime.Now.AddYears(1),
                            Path = "/"
                        });
                    }
                }
            }
            webTask.IsDebug = OpenTaskDebug;
            DocumentLoadCompleted += webTask.DocumentCompletedHandler;
            DocumentLoadStart += webTask.DocumentLoadStartHandler;
            AddEventListener(webTask.GetEventListener());
            webTask.OnTaskCompleted += WebTask_OnTaskCompleted;
            _ = readyResetEvent.Reset();
            webTask.OnTaskReady += WebTask_OnTaskReady;
            cSObj.SetWebTask(webTask);
        }


        private void ResetUrl(string url)
        {
            var ar = new AutoResetEvent(false);
            DocumentLoadCompleted += DefaultChromiumWebBrowser_DocumentLoadCompleted;
            try
            {
                GetBrowser().MainFrame.LoadUrl(url);
                if (!ar.WaitOne(30000))
                {

                    throw new TimeoutException("重定向失败");
                }

            }
            finally
            {
                DocumentLoadCompleted -= DefaultChromiumWebBrowser_DocumentLoadCompleted;
            }

            void DefaultChromiumWebBrowser_DocumentLoadCompleted(IBrowser arg1, IFrame arg2)
            {
                ar.Set();
            }
        }


        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="webTask"></param>
        private async Task<bool> RunTask(IWebTask webTask)
        {
            try
            {
                if (DocumentLoadCompleted?.GetInvocationList().Length > 0)
                {
                    throw new NotSupportedException($"存在未清理的任务:{DocumentLoadCompleted?.GetInvocationList().Length}个");
                }
                lastTask = webTask;
                OnTaskStart?.Invoke(webTask);

                webTask.OnMsgPublish += WebTask_OnMsgPublish;
                (JsDialogHandler as JsDialogHandler)?.Clear();

                

                //if (webTask.UseProxy)
                //{
                //    var config = AutofacBuilder.GetFromFac<SpidersConfig>();
                //    if (config.AbuyunConfig != null && config.AbuyunConfig.Enabled == true && !string.IsNullOrWhiteSpace(config.AbuyunConfig.ProxyUri))
                //    {
                //        SetProxy(config.AbuyunConfig.ProxyUri);
                //    }
                //    else
                //    {
                //        ClearProxy();
                //    }
                //}
                //else
                //{
                //    ClearProxy();
                //}

                this.OnMsgPublished($"开始任务:{webTask.GetTaskName()},地址:{webTask.GetStartPageUrl()}");
                var frame = GetBrowser().MainFrame;
                using (var taskRequest = webTask.GetTestRequest(frame.CreateRequest(false)))
                {
                    if ("get".Equals(taskRequest.Method, StringComparison.OrdinalIgnoreCase)
                        && taskRequest.PostData == null
                        && taskRequest.Headers.Count == 0
                        && string.IsNullOrWhiteSpace(taskRequest.ReferrerUrl))
                    {
                        PepareTask(webTask);
                        GetBrowser().MainFrame.LoadUrl(taskRequest.Url);
                    }
                    else
                    {
                        var uri = new Uri(taskRequest.Url);
                        if (!new Uri(frame.Url).Host.Equals(uri.Host, StringComparison.OrdinalIgnoreCase))
                        {
                            var firstUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}/_forResetRequest";
                            ResetUrl(firstUrl);
                        }

                        PepareTask(webTask);
                        GetBrowser().MainFrame.LoadRequest(taskRequest);
                    }
                }

                if (!readyResetEvent.WaitOne(60000))
                {
                    OnMsgPublished($"任务超时:{webTask.GetTaskName()}");
                    webTask.ForceCancel("超时");
                    WebTask_OnTaskCompleted(webTask);
                }

                if (cancelFlag)
                {
                    OnMsgPublished($"任务取消:{webTask.GetTaskName()}");
                    WebTask_OnTaskCompleted(webTask);
                }
            }
            catch (Exception ex)
            {
                OnMsgPublished?.Invoke($"任务出错:{webTask.GetTaskName()},{ex.Message}");
                WebTask_OnTaskCompleted(webTask);
                throw;
            }

            return true;
        }

        private void WebTask_OnTaskReady(IWebTask webTask)
        {
            _ = readyResetEvent.Set();
            try
            {
                var cookieManager = this.GetCookieManager();
                _ = webTask.Execute(GetBrowser(), GetBrowser().MainFrame, cookieManager);
            }
            catch (Exception ex)
            {
                _ = OnMsgPublished?.BeginInvoke(ex.Message, null, null);
                WebTask_OnTaskCompleted(webTask);
            }
        }

        private void WebTask_OnMsgPublish(string msg)
        {
            OnMsgPublished?.Invoke(msg);
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="webTask"></param>
        public async Task<bool> RunTask()
        {
            lock (localLocker)
            {
                int tryTimes = 0;
                while (!isInit)
                {
                    Thread.Sleep(100);
                    if (tryTimes++ > 300)
                    {
                        OnMsgPublished?.Invoke("timeout:isInit");
                        throw new TimeoutException("isInit");
                    }
                }

                if (isRunningJob)
                {
                    return true;
                }
                cancelFlag = false;
                isRunningJob = true;
            }

            OnMsgPublished?.Invoke(Consts.CMDCLEARMSG);

            IWebTask task = GetOne();
            if (task != null)
            {
                return await RunTask(task);
            }
            else
            {
                isRunningJob = false;
                return false;
            }
        }

        private void WebTask_OnTaskCompleted(IWebTask webTask)
        {
            try
            {
                this.Stop();
                webBrowserTool.WaitLoading(GetBrowser(), cancelFlag, timeOutMs: 30000);
                Clear(webTask);

                if (webTask.GetNext() != null)
                {
                    InsertOne(webTask.GetNext(), 0);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    LogHelper.Instance.Error("出错（此错误可忽略）", ex);
                    this.LoadUrl("about:blank");
                    webBrowserTool.WaitLoading(GetBrowser(), cancelFlag, timeOutMs: 30000);
                    Clear(webTask);
                }
                catch (Exception exx)
                {
                    LogHelper.Instance.Error("出错", exx);
                    WebTask_OnMsgPublish("clear task失败,后面可能会出现串页情况：" + exx.Message);
                    Clear(webTask);
                }
            }
            finally
            {
                if (cancelFlag)
                {
                    ClearTask();
                    isRunningJob = false;
                    cancelFlag = false;
                    WebTask_OnMsgPublish("测试取消");
                }
                else
                {
                    IWebTask task = GetOne();
                    if (task != null)
                    {
                        _ = RunTask(task);
                    }
                    else
                    {
                        isRunningJob = false;
                        WebTask_OnMsgPublish("所有测试完成");
                    }
                }

            }
        }

        private void SetProxy(string ip)
        {
            _ = Cef.UIThreadTaskFactory.StartNew(() =>
            {
                var rc = GetBrowser().GetHost().RequestContext;
                var nowdic = rc.GetAllPreferences(true);
                if (!nowdic.ContainsKey("proxy") || nowdic["proxy"] == null || ((dynamic)nowdic["proxy"]).server != ip)
                {
                    var dict = new Dictionary<string, object>();
                    dict.Add("mode", "fixed_servers");
                    dict.Add("server", $"{ip}"); //此处替换成实际 ip地址：端口
                    string error;
                    bool success = rc.SetPreference("proxy", dict, out error);
                    if (!success)
                    {
                        OnMsgPublished?.Invoke("something happen with the prerence set up" + error);
                    }
                }
            });
        }

        private void ClearProxy()
        {
            _ = Cef.UIThreadTaskFactory.StartNew(() =>
              {
                  var rc = GetBrowser().GetHost().RequestContext;
                  var nowdic = rc.GetAllPreferences(true);
                  if (nowdic.ContainsKey("proxy") && nowdic["proxy"] != null)
                  {
                      var dict = new Dictionary<string, object>();
                      dict.Add("mode", null);
                      dict.Add("server", null); //此处替换成实际 ip地址：端口
                      string error;
                      bool success = rc.SetPreference("proxy", dict, out error);
                      if (!success)
                      {
                          OnMsgPublished?.Invoke("something happen with the prerence set up" + error);
                      }
                  }
              });
        }

        /// <summary>
        /// 是否是在执行任务
        /// </summary>
        /// <returns></returns>
        public bool IsRunningJob()
        {
            return isRunningJob;
        }

        private bool _taskDebug = false;
        public bool OpenTaskDebug
        {
            get
            {
                return _taskDebug;
            }
            set
            {
                if (lastTask != null)
                {
                    lastTask.IsDebug = value;
                }
                _taskDebug = value;
            }
        }

        /// <summary>
        /// 获取当前任务
        /// </summary>
        /// <returns></returns>
        public IWebTask GetCurrentTask()
        {
            return lastTask;
        }

        protected override void Dispose(bool disposing)
        {
            // Cleanup managed objects by calling their
            // Dispose() methods.

            OnMsgPublished = null;

            JavascriptObjectRepository.UnRegisterAll();//解绑对象 高版本才有
            GetBrowser().CloseBrowser(false);//关闭浏览器

            base.Dispose(disposing);
        }
    }
}
