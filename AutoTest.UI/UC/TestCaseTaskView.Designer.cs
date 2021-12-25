
namespace AutoTest.UI.UC
{
    partial class TestCaseTaskView
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.UCTestCaseSelector1 = new AutoTest.UI.UC.UCTestCaseSelector();
            this.BtnOk = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // UCTestCaseSelector1
            // 
            this.UCTestCaseSelector1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UCTestCaseSelector1.Location = new System.Drawing.Point(3, 3);
            this.UCTestCaseSelector1.Name = "UCTestCaseSelector1";
            this.UCTestCaseSelector1.Size = new System.Drawing.Size(540, 318);
            this.UCTestCaseSelector1.TabIndex = 0;
            // 
            // BtnOk
            // 
            this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnOk.Location = new System.Drawing.Point(378, 334);
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Size = new System.Drawing.Size(77, 31);
            this.BtnOk.TabIndex = 1;
            this.BtnOk.Text = "执行";
            this.BtnOk.UseVisualStyleBackColor = true;
            this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnCancel.Location = new System.Drawing.Point(461, 334);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 31);
            this.BtnCancel.TabIndex = 2;
            this.BtnCancel.Text = "停止";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // TestCaseTaskView
            // 
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOk);
            this.Controls.Add(this.UCTestCaseSelector1);
            this.Name = "TestCaseTaskView";
            this.Size = new System.Drawing.Size(546, 371);
            this.ResumeLayout(false);

        }

        #endregion

        private UCTestCaseSelector UCTestCaseSelector1;
        private System.Windows.Forms.Button BtnOk;
        private System.Windows.Forms.Button BtnCancel;
    }
}
