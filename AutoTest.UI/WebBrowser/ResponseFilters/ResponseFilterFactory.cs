﻿using LJC.FrameWorkV3.LogManager;
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
                    return filter;
                }
            }

            LogHelper.Instance.Warn("找不到过滤器:" + mimetype);

            return null;
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
