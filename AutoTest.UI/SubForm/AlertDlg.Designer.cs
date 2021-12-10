namespace AutoTest.UI.SubForm
{
    partial class AlertDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LBMsg = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.LBSecs = new System.Windows.Forms.Label();
            this.Timer = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // LBMsg
            // 
            this.LBMsg.AutoSize = true;
            this.LBMsg.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LBMsg.ForeColor = System.Drawing.Color.Red;
            this.LBMsg.Location = new System.Drawing.Point(28, 24);
            this.LBMsg.Name = "LBMsg";
            this.LBMsg.Size = new System.Drawing.Size(55, 14);
            this.LBMsg.TabIndex = 0;
            this.LBMsg.Text = "label1";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(243, 117);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 40);
            this.button1.TabIndex = 1;
            this.button1.Text = "确认";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // LBSecs
            // 
            this.LBSecs.AutoSize = true;
            this.LBSecs.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LBSecs.ForeColor = System.Drawing.Color.Lime;
            this.LBSecs.Location = new System.Drawing.Point(63, 105);
            this.LBSecs.Name = "LBSecs";
            this.LBSecs.Size = new System.Drawing.Size(47, 12);
            this.LBSecs.TabIndex = 2;
            this.LBSecs.Text = "label1";
            // 
            // AlertDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(387, 169);
            this.Controls.Add(this.LBSecs);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.LBMsg);
            this.Name = "AlertDlg";
            this.Text = "AlertDlg";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LBMsg;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label LBSecs;
        private System.ComponentModel.BackgroundWorker Timer;
    }
}