namespace AutoTest.UI.UC
{
    partial class UCTestScript
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.TBName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.NUDNumber = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.TBBody = new AutoTest.UI.UC.JSCodeEditBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TBDesc = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.NUDNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "函数名称：";
            // 
            // TBName
            // 
            this.TBName.Location = new System.Drawing.Point(118, 21);
            this.TBName.Name = "TBName";
            this.TBName.Size = new System.Drawing.Size(170, 21);
            this.TBName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(71, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "顺序：";
            // 
            // NUDNumber
            // 
            this.NUDNumber.Location = new System.Drawing.Point(118, 62);
            this.NUDNumber.Name = "NUDNumber";
            this.NUDNumber.Size = new System.Drawing.Size(170, 21);
            this.NUDNumber.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 4;
            this.label3.Tag = "";
            this.label3.Text = "脚本或者远程地址：";
            // 
            // TBBody
            // 
            this.TBBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TBBody.Location = new System.Drawing.Point(122, 107);
            this.TBBody.Name = "TBBody";
            this.TBBody.SelectedText = "";
            this.TBBody.Size = new System.Drawing.Size(582, 519);
            this.TBBody.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(356, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "函数说明：";
            // 
            // TBDesc
            // 
            this.TBDesc.Location = new System.Drawing.Point(427, 21);
            this.TBDesc.Multiline = true;
            this.TBDesc.Name = "TBDesc";
            this.TBDesc.Size = new System.Drawing.Size(265, 62);
            this.TBDesc.TabIndex = 8;
            // 
            // UCTestScript
            // 
            this.Controls.Add(this.TBDesc);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.TBBody);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.NUDNumber);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TBName);
            this.Controls.Add(this.label1);
            this.Name = "UCTestScript";
            this.Size = new System.Drawing.Size(717, 639);
            ((System.ComponentModel.ISupportInitialize)(this.NUDNumber)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TBName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown NUDNumber;
        private System.Windows.Forms.Label label3;
        private JSCodeEditBox TBBody;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TBDesc;
    }
}
