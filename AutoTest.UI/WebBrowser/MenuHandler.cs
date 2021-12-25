using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.WebBrowser
{
    public class MenuHandler : IContextMenuHandler
    {
        private bool isShowDevTool = false;

        private DefaultChromiumWebBrowser defaultChromiumWebBrowserContext = null;

        public MenuHandler Init(DefaultChromiumWebBrowser defaultChromiumWebBrowser)
        {
            defaultChromiumWebBrowserContext = defaultChromiumWebBrowser;
            return this;
        }

        void IContextMenuHandler.OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            //主要修改代码在此处;如果需要完完全全重新添加菜单项,首先执行model.Clear()清空菜单列表即可.
            //需要自定义菜单项的,可以在这里添加按钮;
            if (model.Count > 0)
            {
                model.AddSeparator();//添加分隔符;
            }
            model.AddItem((CefMenuCommand)103, "刷新");
            model.AddItem((CefMenuCommand.CustomFirst + 1), "复制URL");
            if (model.Count > 0)
            {
                model.AddSeparator();//添加分隔符;
            }
            if (!isShowDevTool)
            {
                model.AddItem((CefMenuCommand)26501, "显示开发工具");
            }
            else
            {
                model.AddItem((CefMenuCommand)26502, "隐藏开发工具");
            }
            if (defaultChromiumWebBrowserContext != null)
            {
                model.AddSeparator();//添加分隔符;
                if (defaultChromiumWebBrowserContext.OpenTaskDebug)
                {
                    model.AddItem((CefMenuCommand.CustomFirst + 2), "关闭调试任务");
                }
                else
                {
                    model.AddItem((CefMenuCommand.CustomFirst + 2), "打开调试任务");
                }
            }
        }

        bool IContextMenuHandler.OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            //命令的执行,点击菜单做什么事写在这里.
            if (commandId == (CefMenuCommand)26501)
            {
                browser.GetHost().ShowDevTools();
                isShowDevTool = true;
                return true;
            }
            else if (commandId == (CefMenuCommand)26502)
            {
                browser.GetHost().CloseDevTools();
                isShowDevTool = false;
                return true;
            }
            else if (commandId == (CefMenuCommand)103)
            {
                browser.Reload(true);
                return true;
            }
            else if (commandId == (CefMenuCommand.CustomFirst + 1))
            {
                Clipboard.SetText(browser.MainFrame.Url);
                MessageBox.Show("URL已复制到剪切板");
                return true;
            }
            else if (commandId == (CefMenuCommand.CustomFirst + 2))
            {
                if (defaultChromiumWebBrowserContext != null)
                {
                    defaultChromiumWebBrowserContext.OpenTaskDebug = !defaultChromiumWebBrowserContext.OpenTaskDebug;
                }
            }
            return false;
        }

        void IContextMenuHandler.OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            var webBrowser = (ChromiumWebBrowser)browserControl;
            Action setContextAction = delegate ()
            {
                webBrowser.ContextMenu = null;
            };
            webBrowser.Invoke(setContextAction);
        }

        bool IContextMenuHandler.RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            //return false 才可以弹出;
            return false;
        }

        //下面这个官网Example的Fun,读取已有菜单项列表时候,实现的IEnumerable,如果不需要,完全可以注释掉;不属于IContextMenuHandler接口规定的
        private static IEnumerable<Tuple<string, CefMenuCommand, bool>> GetMenuItems(IMenuModel model)
        {
            for (var i = 0; i < model.Count; i++)
            {
                var header = model.GetLabelAt(i);
                var commandId = model.GetCommandIdAt(i);
                var isEnabled = model.IsEnabledAt(i);
                yield return new Tuple<string, CefMenuCommand, bool>(header, commandId, isEnabled);
            }
        }
    }
}
