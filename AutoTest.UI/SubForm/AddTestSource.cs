using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Windows.Forms;

namespace AutoTest.UI.SubForm
{
    public partial class AddTestSource : Form
    {
        private int _sourceId;

        public AddTestSource()
        {
            InitializeComponent();
        }

        public AddTestSource(int sourceId)
        {
            InitializeComponent();
            _sourceId = sourceId;

            var source = AutoTest.Data.DataStoreSwitcher.Current.Find<TestSource>(nameof(TestSource), _sourceId);
            if (source == null)
            {
                MessageBox.Show("资源不存在或被删除");
                return;
            }
            TBSourceName.Text = source.SourceName;
            TBSourceDesc.Text = source.Desc;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBSourceName.Text))
            {
                MessageBox.Show("资源名称不能为空");
                return;
            }
            if (_sourceId > 0)
            {
                var source = AutoTest.Data.DataStoreSwitcher.Current.Find<TestSource>(nameof(TestSource), _sourceId);
                if (source == null)
                {
                    MessageBox.Show("资源不存在或被删除");
                    return;
                }
                source.SourceName = TBSourceName.Text.Trim();
                source.Desc = TBSourceDesc.Text.Trim();
                AutoTest.Data.DataStoreSwitcher.Current.Update(nameof(TestSource), source);
            }
            else
            {
                AutoTest.Data.DataStoreSwitcher.Current.Insert(nameof(TestSource), new TestSource
                {
                    SourceName = TBSourceName.Text.Trim(),
                    Desc = TBSourceDesc.Text.Trim()
                });
            }

            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}

