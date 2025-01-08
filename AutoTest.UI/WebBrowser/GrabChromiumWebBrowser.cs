using AutoTest.UI.WebBrowser.EventListener;
using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser
{
    public class GrabChromiumWebBrowser: ChromiumWebBrowser
    {
        /// <summary>
        /// 事件处理链
        /// </summary>
        private readonly List<IEventListener> eventListeners = new List<IEventListener>();

        public GrabChromiumWebBrowser(string name, string address)
            : base(address)
        {
            Name = name;
            var setting = new RequestContextSettings()
            {
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CefSharp\\Cache_" + name),
                PersistSessionCookies = true
            };
            var context = new RequestContext(setting);
            RequestContext = context;

            RequestHandler = new RequestHandler.GrabRequestHandler();
           
            LifeSpanHandler = new DefLifeSpanHandler();
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
    }
}
