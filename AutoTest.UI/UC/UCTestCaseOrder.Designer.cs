
namespace AutoTest.UI.UC
{
    partial class UCTestCaseOrder
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
            this.CBBroswer = new CefSharp.WinForms.ChromiumWebBrowser();
            this.SuspendLayout();
            // 
            // CBBroswer
            // 
            this.CBBroswer.ActivateBrowserOnCreation = false;
// TODO: “”的代码生成失败，原因是出现异常“无效的基元类型: System.IntPtr。请考虑使用 CodeObjectCreateExpression。”。
            this.CBBroswer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CBBroswer.Location = new System.Drawing.Point(0, 0);
            this.CBBroswer.Name = "CBBroswer";
            this.CBBroswer.Size = new System.Drawing.Size(542, 370);
            this.CBBroswer.TabIndex = 1;
            // 
            // UCTestCaseOrder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CBBroswer);
            this.Name = "UCTestCaseOrder";
            this.Size = new System.Drawing.Size(542, 370);
            this.ResumeLayout(false);

        }

        #endregion

        private CefSharp.WinForms.ChromiumWebBrowser CBBroswer;
    }
}
