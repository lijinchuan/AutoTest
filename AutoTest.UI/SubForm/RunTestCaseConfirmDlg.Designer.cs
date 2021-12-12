
namespace AutoTest.UI.SubForm
{
    partial class RunTestCaseConfirmDlg
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.GVTaskList = new System.Windows.Forms.DataGridView();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GVTaskList)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.groupBox1.Location = new System.Drawing.Point(-7, 407);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(809, 1);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.GVTaskList);
            this.panel1.Location = new System.Drawing.Point(0, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(798, 402);
            this.panel1.TabIndex = 1;
            // 
            // BtnOK
            // 
            this.BtnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnOK.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnOK.Location = new System.Drawing.Point(623, 415);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(75, 23);
            this.BtnOK.TabIndex = 2;
            this.BtnOK.Text = "确定";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnCancel.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnCancel.Location = new System.Drawing.Point(713, 415);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 3;
            this.BtnCancel.Text = "取消";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // GVTaskList
            // 
            this.GVTaskList.BackgroundColor = System.Drawing.Color.White;
            this.GVTaskList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GVTaskList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GVTaskList.Location = new System.Drawing.Point(0, 0);
            this.GVTaskList.Name = "GVTaskList";
            this.GVTaskList.RowTemplate.Height = 23;
            this.GVTaskList.Size = new System.Drawing.Size(798, 402);
            this.GVTaskList.TabIndex = 9;
            // 
            // RunTestCaseConfirmDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Name = "RunTestCaseConfirmDlg";
            this.Text = "测试确认";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GVTaskList)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.DataGridView GVTaskList;
    }
}