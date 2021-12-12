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
    public partial class ConfirmDlg : SubBaseDlg
    {
        private int ShowSecs = 5;
        private bool _timeOutOk = false;

        public ConfirmDlg()
        {
            InitializeComponent();
        }

        public ConfirmDlg(string source,string msg,int showSecs=5,bool timeOutOk=true)
        {
            InitializeComponent();
            this.Text = source;
            this.LBMsg.Text = msg;
            this.ShowSecs = showSecs;
            this.Timer.DoWork += Timer_DoWork;
            _timeOutOk = timeOutOk;
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
                    this.DialogResult = _timeOutOk ? DialogResult.OK : DialogResult.Cancel;
                }));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;

        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
