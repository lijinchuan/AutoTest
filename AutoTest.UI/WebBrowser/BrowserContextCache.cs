using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoTest.UI.WebBrowser
{
    /// <summary>
    /// 浏览器上下文缓存。
    /// 原实现每 new 一个浏览器就 new 一个 RequestContext，而 CefSharp 内部不会替调用方释放它，
    /// 导致 CEF 的 browser context（网络栈、磁盘缓存、共享内存、渲染进程）无限累积。
    /// 这里按名称复用，并且总数有上限，应用退出时统一释放。
    /// </summary>
    public static class BrowserContextCache
    {
        /// <summary>
        /// 上下文数量上限。面板名可能来自用户标题（执行单个测试、[包名]执行定时测试……），
        /// 不设上限的话每个新标题都会新建一套网络栈。
        /// </summary>
        private const int MaxContexts = 16;

        private const string OverflowKey = "shared-overflow";

        private static readonly object locker = new object();
        private static readonly Dictionary<string, RequestContext> contexts =
            new Dictionary<string, RequestContext>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 获取（或创建）指定名称的浏览器上下文
        /// </summary>
        public static RequestContext Acquire(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "default";
            }

            lock (locker)
            {
                RequestContext context;
                if (contexts.TryGetValue(name, out context) && !context.IsDisposed)
                {
                    return context;
                }

                //达到上限后不再新建，统一退化到共享上下文，保证上下文数量有界
                if (contexts.Count >= MaxContexts)
                {
                    name = OverflowKey;
                    if (contexts.TryGetValue(name, out context) && !context.IsDisposed)
                    {
                        return context;
                    }
                }

                var settings = new RequestContextSettings
                {
                    CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CefSharp", "Cache_" + ToSafeFolderName(name)),
                    PersistSessionCookies = true
                };

                context = new RequestContext(settings);
                contexts[name] = context;

                return context;
            }
        }

        /// <summary>
        /// 面板名直接当目录名用，可能带 : \ / 等非法字符，这里清洗并加哈希避免重名
        /// </summary>
        private static string ToSafeFolderName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(name.Length);

            foreach (var ch in name)
            {
                sb.Append(Array.IndexOf(invalid, ch) >= 0 ? '_' : ch);
            }

            var folder = sb.ToString().Trim();
            if (folder.Length == 0)
            {
                folder = "default";
            }
            if (folder.Length > 40)
            {
                folder = folder.Substring(0, 40);
            }

            return folder + "_" + name.GetHashCode().ToString("x8");
        }

        /// <summary>
        /// 当前缓存的上下文数量，用于排查
        /// </summary>
        public static int Count
        {
            get
            {
                lock (locker)
                {
                    return contexts.Count;
                }
            }
        }

        /// <summary>
        /// 释放全部上下文。必须在 Cef.Shutdown() 之前调用。
        /// </summary>
        public static void DisposeAll()
        {
            RequestContext[] list;
            lock (locker)
            {
                list = new RequestContext[contexts.Count];
                contexts.Values.CopyTo(list, 0);
                contexts.Clear();
            }

            foreach (var context in list)
            {
                try
                {
                    if (!context.IsDisposed)
                    {
                        context.Dispose();
                    }
                }
                catch
                {
                    // CEF 已进入关闭流程时忽略
                }
            }
        }
    }
}
