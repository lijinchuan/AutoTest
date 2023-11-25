using AutoTest.Biz;
using AutoTest.Domain;
using AutoTest.UI.SubForm;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTest.Domain.Entity;
using System.Diagnostics;
using CefSharp;
using CefSharp.WinForms;
using CefSharp.DevTools.Overlay;
using AutoTest.UI.UC;

namespace AutoTest.UI
{
    public partial class MainFrm : Form
    {
        private System.Timers.Timer tasktimer = null;
        private WatingDlg wdlg = new WatingDlg();

        public static MainFrm Instance
        {
            get;
            private set;
        }


        private void InitFrm()
        {
            this.tsb_Excute.Enabled = false;
            this.TSBSave.Enabled = false;

            this.TabControl.Selected += new TabControlEventHandler(TabControl_Selected);

            this.TSCBServer.ForeColor = Color.HotPink;
            this.TSCBServer.Visible = false;
            this.TSCBServer.Image = Resources.Resource1.connect;
            this.TSCBServer.Alignment = ToolStripItemAlignment.Right;

            this.MspPanel.TextAlign = ContentAlignment.TopLeft;

            TSBSave.Click += TSBSave_Click;
            tsb_Excute.Click += Tsb_Excute_Click;

            TSBar.Image = Resources.Resource1.side_contract;
            TSBar.Click += TSBar_Click;
        }

        private void TSBar_Click(object sender, EventArgs e)
        {
            if (TSBar.Tag == null)
            {
                TSBar.Image = Resources.Resource1.side_expand;
                TSBar.Tag = panel1.Location;
                var location = dbServerView1.Location;
                location.Offset(2, 0);
                panel1.Location = location;
                panel1.Width += ((Point)TSBar.Tag).X - location.X;
                dbServerView1.Hide();
            }
            else
            {
                TSBar.Image = Resources.Resource1.side_contract;
                panel1.Width -= ((Point)TSBar.Tag).X - panel1.Location.X;
                panel1.Location = (Point)TSBar.Tag;

                TSBar.Tag = null;
                dbServerView1.Show();
            }
        }


        private void Tsb_Excute_Click(object sender, EventArgs e)
        {
            if (TabControl.SelectedTab != null && TabControl.SelectedTab is IExcuteAble)
            {
                (TabControl.SelectedTab as IExcuteAble).Execute();
            }
        }

        private void TSBSave_Click(object sender, EventArgs e)
        {
            if (TabControl.SelectedTab != null && TabControl.SelectedTab is ISaveAble)
            {
                (TabControl.SelectedTab as ISaveAble).Save();
            }
        }

        public MainFrm()
        {
            if (!Cef.IsInitialized)
            {
                var settings = new CefSettings();
                settings.LogSeverity = LogSeverity.Warning;
                //settings.CefCommandLineArgs.Add("--js-flags", $"--max_old_space_size=2048");
                Cef.Initialize(settings);
            }

            InitializeComponent();
            InitFrm();

            Instance = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Visible = false;
            wdlg.Show("初始化数据，请稍候...");

            try
            {
                TabPage selecedpage = null;
                foreach (var tp in Biz.RecoverManager.Recove())
                {
                    this.TabControl.TabPages.Add(tp.Item1);
                    if (tp.Item2 == true)
                    {
                        selecedpage = tp.Item1;
                    }
                }
                this.TabControl.SelectedIndex = -1;
                if (selecedpage != null)
                {
                    this.TabControl.SelectedTab = selecedpage;
                }
                else
                {
                    if (this.TabControl.TabPages.Count > 0)
                    {
                        this.TabControl.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.SendMsg(this, $"恢复关闭前选项卡失败:{ex.Message}", 180);
            }

            wdlg.Hide();
            this.Visible = true;

            TSMRepairMode.Checked = RuntimeConfig.IsRepirMode;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if ((e as FormClosingEventArgs)?.CloseReason == CloseReason.TaskManagerClosing)
            {
                e.Cancel = true;
                return;
            }
            if (MessageBox.Show("要退出吗？", "询问", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            this.Visible = false;
            wdlg.Show("程序退出...");

            base.OnClosing(e);

            if (tasktimer != null)
            {
                tasktimer.Stop();
                tasktimer.Close();
            }

            try
            {
                wdlg.Msg = "保存工作数据...";
                Thread.Sleep(1000);
                foreach (TabPage tab in this.TabControl.TabPages)
                {
                    bool isSelected = this.TabControl.SelectedTab == tab;
                    if (tab is IRecoverAble)
                    {
                        RecoverManager.AddRecoverInstance(tab, isSelected);
                    }
                }
                RecoverManager.SaveRecoverInstance();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            try
            {
                wdlg.Msg = "其它工作...";
                BigEntityTableEngine.LocalEngine.ShutDown();
            }
            catch(Exception ex)
            {

            }

            try
            {
                KillBrowserSubprocess();//杀死进程
                Cef.Shutdown();//释放资源
            }
            catch
            {

            }

            wdlg.Close();
        }

        void TabControl_Selected(object sender, TabControlEventArgs e)
        {
            this.TSCBServer.Visible = false;

            TSBSave.Enabled = e.TabPage is ISaveAble;
            tsb_Excute.Enabled = e.TabPage is IExcuteAble;
        }

        private void 断开对象资源管理器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var server = this.dbServerView1.DisConnectSelectDBServer();
            //if (server != null && this.TSCBServer.Items.Contains(server))
            //{
            //    this.TSCBServer.Items.Remove(server);
            //}
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "执行":
                    break;
            }
        }


        internal void SetMsg(string msg)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    this.MspPanel.Text = msg;
                    if (this.MspPanel.Width >= this.statusStrip1.Width - this.MspPanel.Width - 10)
                    {
                        this.MspPanel.Spring = true;
                        this.TSL_ClearMsg.Visible = true;
                    }
                }));
            }
            else
            {
                this.MspPanel.Text = msg;
                if (this.MspPanel.Width >= this.statusStrip1.Width - this.MspPanel.Width - 10)
                {
                    this.TSL_ClearMsg.Visible = true;
                    this.MspPanel.Spring = true;
                }
            }
        }

        internal void ClearMsg(string oldmsg)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    if (this.MspPanel.Text == oldmsg)
                    {
                        this.MspPanel.Text = string.Empty;
                    }
                }));
            }
            else
            {
                if (this.MspPanel.Text == oldmsg)
                {
                    this.MspPanel.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// 根据模型建表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubItemModelCreateTableTool_Click(object sender, EventArgs e)
        {

        }

        private void TabControl_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void 查看日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        public void SelectedTab(TabPage selpage)
        {
            foreach (TabPage page in this.TabControl.TabPages)
            {

                if (page == selpage)
                {
                    TabControl.SelectedTab = page;
                    break;
                }
            }
        }

        public IEnumerable<TestPanel> FindTestPanel()
        {
            foreach (TabPage page in this.TabControl.TabPages)
            {
                if (page is TestPanel tp)
                {
                    yield return tp;
                }
            }
        }

        public bool AddTab(string title, TabPage addpage)
        {
            bool isExists = false;
            foreach (TabPage page in this.TabControl.TabPages)
            {
                if (page.Text == title || page == addpage)
                {
                    isExists = true;
                    TabControl.SelectedTab = page;
                    break;
                }
            }

            if (!isExists)
            {
                this.TabControl.TabPages.Add(addpage);
                TabControl.SelectedTab = addpage;
                return true;
            }

            return false;
        }

        public TabPage TryAddTab(string title, Func<TabPage> func, Type tabPageType = null)
        {
            bool isExists = false;
            TabPage addpage = null;
            foreach (TabPage page in this.TabControl.TabPages)
            {
                if (page.Text == title || (tabPageType != null && page.GetType() == tabPageType))
                {
                    isExists = true;
                    TabControl.SelectedTab = page;
                    addpage = page;
                    break;
                }
            }

            if (!isExists)
            {
                addpage = func();
                if (addpage != null)
                {
                    if (string.IsNullOrWhiteSpace(addpage.Text))
                    {
                        addpage.Text = title;
                    }
                    this.BeginInvoke(new Action(() =>
                    {
                        TabControl.TabPages.Add(addpage);
                        TabControl.SelectedTab = addpage;
                    }));
                }
            }

            return addpage;
        }


        private void TSL_ClearMsg_Click(object sender, EventArgs e)
        {
            this.MspPanel.Text = "";
            TSL_ClearMsg.Visible = false;
            this.MspPanel.Spring = false;
        }

        private void mD5签名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SubForm.MD5Dlg dlg = new MD5Dlg();
            dlg.Owner = this;
            dlg.Show();
        }

        private void 时间戳ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://tool.lu/timestamp/");
        }

        private void bASE64ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://tool.lu/encdec/");
        }

        private void 正则表达式测试ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://tool.lu/regex/");
        }

        private void xML工具ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://tool.lu/xml/");
        }

        private void jSON工具ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://tool.lu/json/");
        }

        private void hTTP状态码ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://tool.lu/httpcode/");
        }

        private void gUIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.iamwawa.cn/guid.html");
        }

        private void 代理服务器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SubBaseDlg();
            dlg.Text = "全局代理服务器";
            var globProxyServer = BigEntityTableEngine.LocalEngine.Find<ProxyServer>(nameof(ProxyServer), p => p.Name.Equals(ProxyServer.GlobName)).FirstOrDefault();

            var ucproxy = new UC.UCProxy(globProxyServer);
            ucproxy.Dock = DockStyle.Fill;
            dlg.Controls.Add(ucproxy);
            dlg.FormClosing += (s, ee) =>
            {
                var proxyserver = ucproxy.GetProxyServer();

                if (ucproxy.HasChanged && MessageBox.Show("要保存吗?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (proxyserver.Id == 0)
                    {
                        proxyserver.Name = ProxyServer.GlobName;
                        BigEntityTableEngine.LocalEngine.Insert<ProxyServer>(nameof(ProxyServer), proxyserver);
                        Util.SendMsg(this, "新增成功");
                    }
                    else
                    {
                        BigEntityTableEngine.LocalEngine.Update<ProxyServer>(nameof(ProxyServer), proxyserver);
                        Util.SendMsg(this, "修改成功");
                    }

                }
            };
            dlg.ShowDialog();
        }

        private void TSMReportError_Click(object sender, EventArgs e)
        {
            new AboutDlg().ShowDialog();
            //try
            //{
            //    System.Diagnostics.Process.Start("mailto:403479851@qq.com?subject=api管理工具v1.0使用问题反馈");
            //}
            //catch
            //{
            //    MessageBox.Show("启动发送邮件应用失败，请手动发送邮件到：403479851@qq.com");
            //}
        }

        private void uRLEncodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SubForm.URLEncodeDlg dlg = new URLEncodeDlg();

            dlg.Show();
        }

        /// <summary>
        /// 清理CefSharp.BrowserSubprocess
        /// </summary>
        private void KillBrowserSubprocess()
        {
            try
            {
                string SYSPath = System.AppDomain.CurrentDomain.BaseDirectory;
                Process[] processs = Process.GetProcessesByName("CefSharp.BrowserSubprocess");
                if (processs != null && processs.Length > 0)
                {
                    for (int i = 0; i < processs.Length; i++)
                    {
                        Process process = processs[i];

                        bool bKill = false;
                        if (process.MainModule != null)
                        {
                            string FileName = process.MainModule.FileName;
                            if (SYSPath.Contains(FileName) || FileName.Contains(SYSPath))
                            {
                                bKill = true;
                            }
                        }
                        if (bKill)
                        {
                            process.Kill();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "清理CefSharp.BrowserSubprocess异常");
            }

        }

        private void 服务器设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new ServerSettingDlg();

            dlg.ShowDialog();
        }

        private void cron表达式生成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://cron.qqe2.com/");
        }

        private void xPath在线测试ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.ab173.com/other/xpath.php");
        }

        private void TSMGrabWeb_Click(object sender, EventArgs e)
        {
            var dlg = new WebSearchDlg();
            dlg.Show();
        }

        private void TSMRepairMode_Click(object sender, EventArgs e)
        {
            RuntimeConfig.IsRepirMode = TSMRepairMode.Checked = !TSMRepairMode.Checked;
            if (RuntimeConfig.IsRepirMode)
            {
                foreach(var tp in FindTestPanel())
                {
                    tp.CancelTasks();
                }
            }
        }
    }
}
