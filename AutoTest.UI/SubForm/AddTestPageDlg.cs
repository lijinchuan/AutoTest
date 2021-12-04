using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.SubForm
{
    public partial class AddTestPageDlg : SubBaseDlg
    {
        private int _siteId;
        private int _pageId;
        public AddTestPageDlg()
        {
            InitializeComponent();
        }

        public AddTestPageDlg(int siteId,int pageId)
        {
            _siteId = siteId;
            _pageId = pageId;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_pageId > 0)
            {
                var testPage = BigEntityTableEngine.LocalEngine.Find<TestPage>(nameof(TestPage), _pageId);
                if (testPage == null)
                {
                    MessageBox.Show("page不存在");
                    this.Close();
                    return;
                }

                TBName.Text = testPage.Name;
                TBUrl.Text = testPage.Url;
                NUDOrder.Value = testPage.Order;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (_pageId == 0)
            {
                BigEntityTableEngine.LocalEngine.Insert(nameof(TestPage), new TestPage
                {
                    Name = TBName.Text,
                    Order = (int)NUDOrder.Value,
                    SiteId = _siteId,
                    Url = TBUrl.Text
                });
            }
            else
            {
                BigEntityTableEngine.LocalEngine.Update(nameof(TestPage), new TestPage
                {
                    Id=_pageId,
                    Name = TBName.Text,
                    Order = (int)NUDOrder.Value,
                    SiteId = _siteId,
                    Url = TBUrl.Text
                });
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
