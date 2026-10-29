using LJC.FrameWorkV3.LogManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser.ResponseFilters
{
    public static class ResponseFilterFactory
    {
        /// <summary>
        /// 过滤器数量上限。响应没有正常结束（跳转、取消、崩溃）时过滤器会一直留在字典里，
        /// 每个都保存着完整响应体，不设上限会一直涨。
        /// </summary>
        private const int MaxFilterCount = 5000;

        /// <summary>
        /// 存放创建的过滤器
        /// </summary>
        private static ConcurrentDictionary<string, BaseResponseFilter> RESPONSEFILTERDIC = new ConcurrentDictionary<string, BaseResponseFilter>();

        /// <summary>
        /// 根据响应类型，创建过滤器
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="mimetype">响应类型</param>
        /// <returns></returns>
        public static BaseResponseFilter CreateResponseFilter(string guid, string mimetype, Func<string, BaseResponseFilter> getOtherFilter = null)
        {
            BaseResponseFilter filter = null;
            if (mimetype.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            {
                filter = new JsonResponseFilter();
            }
            else if (mimetype.Equals("text/html", StringComparison.OrdinalIgnoreCase)
                || mimetype.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
            {
                filter = new TextResponseFilter();
            }

            if (filter == null && getOtherFilter != null)
            {
                filter = getOtherFilter(mimetype);
            }

            if (filter != null)
            {
                if (RESPONSEFILTERDIC.TryAdd(guid, filter))
                {
                    TrimIfNeeded();
                    return filter;
                }

                //guid重复时新过滤器不会被使用，必须释放，否则它的 MemoryStream 没人回收
                filter.ReleaseBuffer();
            }

            LogHelper.Instance.Warn("找不到过滤器:" + mimetype);

            return null;
        }

        /// <summary>
        /// 超过上限时淘汰最旧的过滤器
        /// </summary>
        private static void TrimIfNeeded()
        {
            if (RESPONSEFILTERDIC.Count <= MaxFilterCount)
            {
                return;
            }

            var overflow = RESPONSEFILTERDIC
                .OrderBy(p => p.Value.CreatedTick)
                .Take(RESPONSEFILTERDIC.Count - MaxFilterCount)
                .ToList();

            foreach (var item in overflow)
            {
                BaseResponseFilter removed;
                if (RESPONSEFILTERDIC.TryRemove(item.Key, out removed))
                {
                    removed.ReleaseBuffer();
                }
            }

            LogHelper.Instance.Warn($"响应过滤器数量超过上限{MaxFilterCount}，已清理{overflow.Count}个未正常结束的响应");
        }

        /// <summary>
        /// 获取过滤器
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static BaseResponseFilter GetResponseFilter(string guid)
        {
            BaseResponseFilter filter;
            if (RESPONSEFILTERDIC.TryRemove(guid, out filter))
            {
                return filter;
            }

            return null;
        }
    }
}
