using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser.EventListener
{
    /// <summary>
    /// 事件监听抽象类
    /// </summary>
    public abstract class EventListener : IEventListener
    {
        public event Func<WebEvent, bool> OnProcess;

        public abstract string GetEventName();

        public virtual bool Process(WebEvent webEvent)
        {
            if (OnProcess != null)
            {
                return OnProcess(webEvent);
            }

            return false;
        }
    }
}
