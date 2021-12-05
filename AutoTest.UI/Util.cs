﻿using AutoTest.UI.SubForm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI
{
    public class Util
    {
        private static Dictionary<int, PopMessageDlg> PopDlgDic = new Dictionary<int, PopMessageDlg>();

        private static string MsgType = "";

        public static void SendMsg(Control ctl, string msg, uint showSecs = 5)
        {
            var parent = ctl;
            while (parent != null)
            {
                if (parent is MainFrm)
                {
                    ((MainFrm)parent).SetMsg(msg);
                    LJC.FrameWorkV3.Comm.TaskHelper.SetInterval((int)showSecs * 1000, () =>
                    {
                        try
                        {
                            ClearOldMsg(parent, msg);
                        }
                        catch
                        {

                        }
                        return true;
                    }, runintime: false);
                    break;
                }
                if (parent.Parent != null)
                {
                    parent = parent.Parent;
                }
                else if (parent is Form)
                {
                    parent = (parent as Form).Owner;
                }
                else
                {
                    parent = parent.Parent;
                }
            }
        }

        public static void SendMsg(Control ctl, string msgtype, string msg)
        {
            var parent = ctl;
            while (parent != null)
            {
                if (parent is MainFrm)
                {
                    MsgType = msgtype;
                    ((MainFrm)parent).SetMsg(msg);
                    break;
                }

                if (parent.Parent != null)
                {
                    parent = parent.Parent;
                }
                else if (parent is Form)
                {
                    parent = (parent as Form).Owner;
                }
                else
                {
                    parent = parent.Parent;
                }
            }
        }

        public static void ClearMsg(Control ctl, string msgtype)
        {
            if (msgtype == MsgType || string.IsNullOrWhiteSpace(MsgType))
            {
                var parent = ctl;
                while (parent != null)
                {
                    if (parent is MainFrm)
                    {
                        ((MainFrm)parent).SetMsg("");
                        break;
                    }

                    if (parent.Parent != null)
                    {
                        parent = parent.Parent;
                    }
                    else if (parent is Form)
                    {
                        parent = (parent as Form).Owner;
                    }
                    else
                    {
                        parent = parent.Parent;
                    }
                }
            }
        }

        public static void ClearOldMsg(Control ctl, string msg)
        {
            var parent = ctl;
            while (parent != null)
            {
                if (parent is MainFrm)
                {
                    ((MainFrm)parent).ClearMsg(msg);
                    break;
                }

                if (parent.Parent != null)
                {
                    parent = parent.Parent;
                }
                else if (parent is Form)
                {
                    parent = (parent as Form).Owner;
                }
                else
                {
                    parent = parent.Parent;
                }
            }
        }


        public static void ClosePopMsg(int msgid)
        {
            PopMessageDlg dlg = null;
            if (PopDlgDic.TryGetValue(msgid, out dlg))
            {
                dlg.Close();
            }
        }

        public static T FindParent<T>(Control ctl) where T : Control
        {
            if (ctl == null)
            {
                return null;
            }

            while (ctl.Parent != null)
            {
                if (ctl.Parent != null && ctl.Parent is T)
                {
                    return (T)ctl.Parent;
                }
                ctl = ctl.Parent;
            }

            return null;
        }

        public static bool AddToMainTab(Control ctl, string title, TabPage page)
        {
            if (string.IsNullOrWhiteSpace(page.Text))
            {
                page.Text = title;
            }
            var parent = ctl;
            while (parent != null)
            {
                if (parent is MainFrm)
                {
                    return ((MainFrm)parent).AddTab(title, page);
                }

                parent = parent.Parent;
            }

            return false;
        }

        public static TabPage TryAddToMainTab(Control ctl, string title, Func<TabPage> func, Type tabPageType = null)
        {
            var parent = ctl;
            while (parent != null)
            {
                if (parent is MainFrm)
                {
                    return ((MainFrm)parent).TryAddTab(title,func,tabPageType);
                }

                parent = parent.Parent;
            }

            return null;
        }

        public static void PopMsg(int msgid, string title, string content)
        {
            PopMessageDlg dlg = null;
            var cnt = 0;
            lock (PopDlgDic)
            {
                if (!PopDlgDic.TryGetValue(msgid, out dlg))
                {
                    dlg = new PopMessageDlg();
                    dlg.Text = title;
                    PopDlgDic.Add(msgid, dlg);
                    dlg.FormClosed += (s, e) =>
                    {
                        lock (PopDlgDic)
                        {
                            PopDlgDic.Remove(msgid);
                        }
                    };
                }

                foreach (var item in PopDlgDic)
                {
                    cnt++;
                    if (item.Value.Equals(dlg))
                    {
                        break;
                    }
                }
            }

            if (dlg.GetMsg() != content || dlg.Text != title)
            {
                dlg.SetMsg(title, content);
                dlg.PopShow(cnt);
            }
        }
    }
}
