
namespace AutoTest.UI.SubForm
{
    partial class WebSearchDlg
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
            this.PanelLeft = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.TBSearchContent = new System.Windows.Forms.TextBox();
            this.BtnSearch = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.TBUrl = new System.Windows.Forms.TextBox();
            this.DGVResult = new System.Windows.Forms.DataGridView();
            this.PBBtn = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGVResult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBBtn)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.PBBtn);
            this.groupBox1.Controls.Add(this.TBUrl);
            this.groupBox1.Controls.Add(this.PanelLeft);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(601, 468);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // PanelLeft
            // 
            this.PanelLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelLeft.Location = new System.Drawing.Point(6, 41);
            this.PanelLeft.Name = "PanelLeft";
            this.PanelLeft.Size = new System.Drawing.Size(589, 421);
            this.PanelLeft.TabIndex = 0;
            this.PanelLeft.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelLeft_Paint);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.DGVResult);
            this.panel1.Controls.Add(this.BtnSearch);
            this.panel1.Controls.Add(this.TBSearchContent);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(619, 29);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(313, 448);
            this.panel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "搜索内容：";
            // 
            // TBSearchContent
            // 
            this.TBSearchContent.Location = new System.Drawing.Point(80, 24);
            this.TBSearchContent.Name = "TBSearchContent";
            this.TBSearchContent.Size = new System.Drawing.Size(173, 21);
            this.TBSearchContent.TabIndex = 3;
            // 
            // BtnSearch
            // 
            this.BtnSearch.Location = new System.Drawing.Point(259, 23);
            this.BtnSearch.Name = "BtnSearch";
            this.BtnSearch.Size = new System.Drawing.Size(45, 23);
            this.BtnSearch.TabIndex = 4;
            this.BtnSearch.Text = "搜索";
            this.BtnSearch.UseVisualStyleBackColor = true;
            this.BtnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "网址：";
            // 
            // TBUrl
            // 
            this.TBUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TBUrl.Location = new System.Drawing.Point(48, 14);
            this.TBUrl.Name = "TBUrl";
            this.TBUrl.Size = new System.Drawing.Size(432, 21);
            this.TBUrl.TabIndex = 6;
            // 
            // DGVResult
            // 
            this.DGVResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DGVResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGVResult.Location = new System.Drawing.Point(11, 52);
            this.DGVResult.Name = "DGVResult";
            this.DGVResult.RowTemplate.Height = 23;
            this.DGVResult.Size = new System.Drawing.Size(293, 393);
            this.DGVResult.TabIndex = 5;
            // 
            // PBBtn
            // 
            this.PBBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PBBtn.Location = new System.Drawing.Point(490, 17);
            this.PBBtn.Name = "PBBtn";
            this.PBBtn.Size = new System.Drawing.Size(17, 17);
            this.PBBtn.TabIndex = 7;
            this.PBBtn.TabStop = false;
            this.PBBtn.Click += new System.EventHandler(this.PBBtn_Click);
            // 
            // WebSearchDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 492);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Name = "WebSearchDlg";
            this.Text = "web资源搜索";
            this.Load += new System.EventHandler(this.WebSearchDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGVResult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBBtn)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel PanelLeft;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox TBSearchContent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtnSearch;
        private System.Windows.Forms.TextBox TBUrl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView DGVResult;
        private System.Windows.Forms.PictureBox PBBtn;
    }
}