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

namespace AutoTest.UI.SubForm
{
    public partial class AlertDlg : SubBaseDlg
    {
        private int ShowSecs = 10;

        private Action<object> callBack;

        public AlertDlg()
        {
            InitializeComponent();
        }

        public AlertDlg(string source,string msg,Action<object> callBack,int showSecs=10)
        {
            InitializeComponent();
            this.Text = source;
            this.LBMsg.Text = msg;
            this.ShowSecs = showSecs;
            this.callBack = callBack;
            this.Timer.DoWork += Timer_DoWork;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Timer.RunWorkerAsync();
        }

        private void Timer_DoWork(object sender, DoWorkEventArgs e)
        {
            var sec = ShowSecs;

            while (sec >= 0)
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        this.LBSecs.Text = $"{sec}秒后自动关闭";
                    }));
                }
                Thread.Sleep(1000);
                sec -= 1;
            }

            if (this.IsHandleCreated&&!this.IsDisposed)
            {
                this.BeginInvoke(new Action(() =>
                {
                    this.DialogResult = DialogResult.OK;
                }));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.callBack != null)
            {
                this.callBack(null);
            }
            if (this.Modal)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.Close();
            }
        }
    }
}
