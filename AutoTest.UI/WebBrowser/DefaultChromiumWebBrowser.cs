using AutoTest.UI.EventListener;
using AutoTest.UI.ResourceHandler;
using AutoTest.UI.WebTask;
using AutoTest.Util;
using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser
{
    /// <summary>
    /// 默认浏览器
    /// </summary>
    public class DefaultChromiumWebBrowser : ChromiumWebBrowser
    {
        /// <summary>
        /// 记录COOKIE
        /// </summary>
        private readonly List<Cookie> cookiecontainer = new List<Cookie>();

        /// <summary>
        ///文档下载完成事件通知 
        /// </summary>
        private event Action<IBrowser,IFrame,List<Cookie>> DocumentLoadCompleted;

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
        private bool isInit = false;

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
        private AutoResetEvent readyResetEvent = new AutoResetEvent(true);

        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger logger = null;

        /// <summary>
        /// 局部锁
        /// </summary>
        private readonly object localLocker = new object();

        public DefaultChromiumWebBrowser(string name, string address)
            : base(address)
        {
            Name = name;
            logger = AutofacBuilder.GetFromFac<ILogger>();
            var setting = new RequestContextSettings()
            {
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CefSharp\\Cache_" + name),
                PersistSessionCookies = true,
                PersistUserPreferences = true
            };
            var context = new RequestContext(setting);
            RequestContext = context;

            FrameLoadStart += DefaultChromiumWebBrowser_FrameLoadStart;
            FrameLoadEnd += DefaultChromiumWebBrowser_FrameLoadEnd;
            LoadingStateChanged += DefaultChromiumWebBrowser_LoadingStateChanged;

            IsBrowserInitializedChanged += DefaultChromiumWebBrowser_IsBrowserInitializedChanged;

            RequestHandler = new DefaultRequestHandler();

        }

        private void DefaultChromiumWebBrowser_IsBrowserInitializedChanged(object sender, EventArgs e)
        {
            if (IsBrowserInitialized)
            {
                if (!isInit)
                {
                    var cookieManager = this.GetCookieManager();
                    CookieVisitor visitor = new CookieVisitor();
                    visitor.SendCookie += Visitor_SendCookie;
                    //_ = cookieManager.VisitAllCookies(visitor);
                    
                    isInit = true;
                }
            }
        }

        private void DefaultChromiumWebBrowser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            
        }

        private void DefaultChromiumWebBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            // Method intentionally left empty.

            if (DocumentLoadCompleted != null)
            {
                _ = new Action(() =>
                {
                    DocumentLoadCompleted(e.Browser, e.Frame, GetCookie());
                }).BeginInvoke(null, null);
            }
        }

        private void DefaultChromiumWebBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            // Method intentionally left empty.
        }

        private void Visitor_SendCookie(CefSharp.Cookie cookie)
        {
            lock (cookiecontainer)
            {
                var oldcookie = cookiecontainer.FirstOrDefault(p => p.Name == cookie.Name && p.Domain == cookie.Domain);
                if (oldcookie != null)
                {
                    _ = cookiecontainer.Remove(oldcookie);
                }
                cookiecontainer.Add(cookie);
            }
        }

        public List<Cookie> GetCookie()
        {
            lock (cookiecontainer)
            {
                return cookiecontainer.Select(p => p).ToList();
            }
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
                _ = eventListeners.RemoveAll(p => p.GetEventName() == eventListener.GetEventName());
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
            _ = webTaskListHash.Remove(webTask.GetTaskName());
            RemoveEventListener(webTask.GetEventListener());
            //删除所有的COOKIE
            _ = this.GetCookieManager().DeleteCookies(webTask.GetSite());
        }

        public bool AddTask(IWebTask webTask)
        {
            lock (webTaskList)
            {
                if (!webTaskListHash.Contains(webTask.GetTaskName()))
                {
                    _ = webTaskListHash.Add(webTask.GetTaskName());
                    webTaskList.Enqueue(webTask);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="webTask"></param>
        private async Task<bool> RunTask(IWebTask webTask)
        {
            try
            {
                webTask.OnMsgPublish += WebTask_OnMsgPublish;
                //第一步，更新COOKIE
                var cooklist = webTask.GetCookieList();
                if (cooklist.Any())
                {
                    var cookmanager = this.GetCookieManager();
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
                if (DocumentLoadCompleted?.GetInvocationList().Length > 0)
                {
                    throw new NotSupportedException($"存在未清理的任务:{DocumentLoadCompleted?.GetInvocationList().Length}个");
                }
                DocumentLoadCompleted += webTask.DocumentCompletedHandler;
                AddEventListener(webTask.GetEventListener());
                webTask.OnTaskCompleted += WebTask_OnTaskCompleted;
                _ = readyResetEvent.Reset();
                webTask.OnTaskReady += WebTask_OnTaskReady;

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

                GetBrowser().MainFrame.LoadUrl(webTask.GetStartPageUrl());


                if (!readyResetEvent.WaitOne(60000))
                {
                    WebTask_OnTaskCompleted(webTask);
                }
            }
            catch (Exception)
            {
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
                _ = webTask.Execute(GetBrowser(), GetBrowser().MainFrame);
            }
            catch (Exception ex)
            {
                _ = OnMsgPublished?.BeginInvoke(ex.Message, null, null);
                logger.Error(ex, "非正常退出,taskname:{0},url:{1}", webTask.GetTaskName(), webTask.GetStartPageUrl());
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
                if (!isInit)
                {
                    return false;
                }

                if (isRunningJob)
                {
                    return true;
                }

                isRunningJob = true;
            }

            OnMsgPublished?.Invoke(Consts.CMDCLEARMSG);

            IWebTask task;
            lock (webTaskList)
            {
                if (!webTaskList.TryDequeue(out task))
                {
                    task = null;
                }
            }
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
                Clear(webTask);
            }
            catch (Exception ex)
            {
                WebTask_OnMsgPublish("clear task失败" + ex.Message);
            }
            finally
            {
                IWebTask task;
                lock (webTaskList)
                {
                    if (!webTaskList.TryDequeue(out task))
                    {
                        task = null;
                    }
                }

                if (task != null)
                {
                    _ = RunTask(task);
                }
                else
                {
                    isRunningJob = false;
                }

            }
        }

        private void SetProxy(string ip)
        {
            _ = Cef.UIThreadTaskFactory.StartNew(() =>
            {
                var rc = GetBrowser().GetHost().RequestContext;
                var nowdic = rc.GetAllPreferences(true);
                if (!nowdic.ContainsKey("proxy") || nowdic["proxy"] == null || nowdic["proxy"].ToString() != ip)
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
    }
}
