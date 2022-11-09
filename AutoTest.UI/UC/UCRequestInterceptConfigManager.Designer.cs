
namespace AutoTest.UI.UC
{
    partial class UCRequestInterceptConfigManager
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
            this.GVRequestIntercept = new System.Windows.Forms.DataGridView();
            this.GBEdit = new System.Windows.Forms.GroupBox();
            this.CBEnable = new System.Windows.Forms.CheckBox();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnSave = new System.Windows.Forms.Button();
            this.TBContent = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.CBTypes = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TBUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.CBMimeType = new System.Windows.Forms.ComboBox();
            this.BtnChooseFile = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.GVRequestIntercept)).BeginInit();
            this.GBEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // GVRequestIntercept
            // 
            this.GVRequestIntercept.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GVRequestIntercept.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GVRequestIntercept.Location = new System.Drawing.Point(3, 3);
            this.GVRequestIntercept.Name = "GVRequestIntercept";
            this.GVRequestIntercept.RowTemplate.Height = 23;
            this.GVRequestIntercept.Size = new System.Drawing.Size(345, 129);
            this.GVRequestIntercept.TabIndex = 0;
            // 
            // GBEdit
            // 
            this.GBEdit.Controls.Add(this.BtnChooseFile);
            this.GBEdit.Controls.Add(this.CBMimeType);
            this.GBEdit.Controls.Add(this.label4);
            this.GBEdit.Controls.Add(this.CBEnable);
            this.GBEdit.Controls.Add(this.BtnCancel);
            this.GBEdit.Controls.Add(this.BtnSave);
            this.GBEdit.Controls.Add(this.TBContent);
            this.GBEdit.Controls.Add(this.label3);
            this.GBEdit.Controls.Add(this.CBTypes);
            this.GBEdit.Controls.Add(this.label2);
            this.GBEdit.Controls.Add(this.TBUrl);
            this.GBEdit.Controls.Add(this.label1);
            this.GBEdit.Location = new System.Drawing.Point(14, 138);
            this.GBEdit.Name = "GBEdit";
            this.GBEdit.Size = new System.Drawing.Size(562, 181);
            this.GBEdit.TabIndex = 1;
            this.GBEdit.TabStop = false;
            this.GBEdit.Text = "groupBox1";
            // 
            // CBEnable
            // 
            this.CBEnable.AutoSize = true;
            this.CBEnable.Checked = true;
            this.CBEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBEnable.Location = new System.Drawing.Point(498, 51);
            this.CBEnable.Name = "CBEnable";
            this.CBEnable.Size = new System.Drawing.Size(48, 16);
            this.CBEnable.TabIndex = 8;
            this.CBEnable.Text = "使用";
            this.CBEnable.UseVisualStyleBackColor = true;
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnCancel.Location = new System.Drawing.Point(480, 152);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(52, 23);
            this.BtnCancel.TabIndex = 7;
            this.BtnCancel.Text = "取消";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnSave
            // 
            this.BtnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSave.Location = new System.Drawing.Point(423, 152);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Size = new System.Drawing.Size(51, 23);
            this.BtnSave.TabIndex = 6;
            this.BtnSave.Text = "保存";
            this.BtnSave.UseVisualStyleBackColor = true;
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // TBContent
            // 
            this.TBContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TBContent.Location = new System.Drawing.Point(77, 80);
            this.TBContent.Multiline = true;
            this.TBContent.Name = "TBContent";
            this.TBContent.Size = new System.Drawing.Size(469, 66);
            this.TBContent.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "返回内容：";
            // 
            // CBTypes
            // 
            this.CBTypes.FormattingEnabled = true;
            this.CBTypes.Location = new System.Drawing.Point(77, 47);
            this.CBTypes.Name = "CBTypes";
            this.CBTypes.Size = new System.Drawing.Size(121, 20);
            this.CBTypes.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "拦截方式：";
            // 
            // TBUrl
            // 
            this.TBUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TBUrl.Location = new System.Drawing.Point(77, 17);
            this.TBUrl.Name = "TBUrl";
            this.TBUrl.Size = new System.Drawing.Size(421, 21);
            this.TBUrl.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "拦截地址：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(227, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "Mime：";
            // 
            // CBMimeType
            // 
            this.CBMimeType.FormattingEnabled = true;
            this.CBMimeType.Location = new System.Drawing.Point(263, 46);
            this.CBMimeType.Name = "CBMimeType";
            this.CBMimeType.Size = new System.Drawing.Size(121, 20);
            this.CBMimeType.TabIndex = 10;
            // 
            // BtnChooseFile
            // 
            this.BtnChooseFile.Location = new System.Drawing.Point(6, 95);
            this.BtnChooseFile.Name = "BtnChooseFile";
            this.BtnChooseFile.Size = new System.Drawing.Size(65, 23);
            this.BtnChooseFile.TabIndex = 11;
            this.BtnChooseFile.Text = "选择文件";
            this.BtnChooseFile.UseVisualStyleBackColor = true;
            this.BtnChooseFile.Click += new System.EventHandler(this.BtnChooseFile_Click);
            // 
            // UCRequestInterceptConfigManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GBEdit);
            this.Controls.Add(this.GVRequestIntercept);
            this.Name = "UCRequestInterceptConfigManager";
            this.Size = new System.Drawing.Size(596, 333);
            ((System.ComponentModel.ISupportInitialize)(this.GVRequestIntercept)).EndInit();
            this.GBEdit.ResumeLayout(false);
            this.GBEdit.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView GVRequestIntercept;
        private System.Windows.Forms.GroupBox GBEdit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TBUrl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox CBTypes;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TBContent;
        private System.Windows.Forms.Button BtnSave;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.CheckBox CBEnable;
        private System.Windows.Forms.ComboBox CBMimeType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button BtnChooseFile;
    }
}
