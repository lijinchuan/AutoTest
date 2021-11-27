using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.EventListener
{
    /// <summary>
    /// 事件监听接口，
    /// 说明：需要ResponseFilterFactory创建响应处理，不然底层截取不了请求和响应的数据。目前只支持JSON接口的监听。
    /// </summary>
    public interface IEventListener
    {
        /// <summary>
        /// 通知处理事件
        /// </summary>
        event Func<WebEvent, bool> OnProcess;

        /// <summary>
        /// 事件名称
        /// </summary>
        string GetEventName();

        /// <summary>
        /// 是否处理了
        /// </summary>
        /// <param name="webEvent"></param>
        /// <returns></returns>
        bool Process(WebEvent webEvent);
    }
}
