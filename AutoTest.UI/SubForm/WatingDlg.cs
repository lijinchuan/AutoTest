using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AutoTest.UI.SubForm
{
    public partial class WatingDlg : Form
    {
        public WatingDlg()
        {
            InitializeComponent();

            // 允许跨线程更新 UI：
            // 对话框由后台线程的消息泵驱动，主线程在工作期间需要
            // 通过 Msg 属性直接更新界面文字（主线程可能处于阻塞状态）
            CheckForIllegalCrossThreadCalls = false;

            this.loadingBox1.Location = new Point(0, 0);
            this.Width = this.loadingBox1.Width;
            this.Height = this.loadingBox1.Height;
        }

        /// <summary>
        /// 显示等待对话框。
        /// 主线程先打开对话框确保可靠显示，随后后台线程接管消息泵，
        /// 使主线程可继续执行而对话框保持响应（GIF 动画、文字更新）。
        /// </summary>
        public void Show(string msg)
        {
            this.Msg = msg;

            // 后台线程：短暂延迟后接管对话框的消息泵
            var bg = new Thread(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        Thread.Sleep(100);
                        this.DialogResult = DialogResult.Cancel;  // 释放主线程的 ShowDialog
                        Thread.Sleep(100);
                        this.ShowDialog();                        // 后台线程接管
                        break;

                    }
                    catch
                    {

                    }
                }
            });
            bg.IsBackground = true;
            bg.SetApartmentState(ApartmentState.STA);
            bg.Start();

            // 主线程阻塞在这里，确保对话框可靠显示
            this.ShowDialog();
        }

        /// <summary>
        /// 关闭对话框。
        /// </summary>
        public new void Hide()
        {
            this.DialogResult = DialogResult.Cancel;
        }

        public string Msg
        {
            set => this.loadingBox1.Msg = value;
        }
    }
}
