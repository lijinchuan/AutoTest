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
            else
            {
                var testPageList = BigEntityTableEngine.LocalEngine.Find<TestPage>(nameof(TestPage), nameof(TestPage.SiteId),
                    new object[] { _siteId }).ToList();

                NUDOrder.Value = testPageList.Count + 1;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBName.Text.Trim()))
            {
                new AlertDlg("添加测试页面", "名称不能为空", null).ShowDialog();
                return;
            }
            var testPageList = BigEntityTableEngine.LocalEngine.Find<TestPage>(nameof(TestPage), nameof(TestPage.SiteId),
                    new object[] { _siteId }).ToList();

            if (testPageList.Any(p => p.Name == TBName.Text.Trim() && (_pageId == 0 || _pageId != p.Id)))
            {
                new AlertDlg("测试页面", $"名称{TBName.Text.Trim()}不能重复", null).ShowDialog();
                return;
            }

            if (_pageId == 0)
            {
                var page = new TestPage
                {
                    Name = TBName.Text.Trim(),
                    Order = (int)NUDOrder.Value,
                    SiteId = _siteId,
                    Url = TBUrl.Text
                };
                BigEntityTableEngine.LocalEngine.Insert(nameof(TestPage), page);
                _pageId = page.Id;

                EventBus.NotifyTestThingAddAction?.Invoke(page);
            }
            else
            {
                var page = new TestPage
                {
                    Id = _pageId,
                    Name = TBName.Text.Trim(),
                    Order = (int)NUDOrder.Value,
                    SiteId = _siteId,
                    Url = TBUrl.Text
                };
                BigEntityTableEngine.LocalEngine.Update(nameof(TestPage), page);

                EventBus.NotifyTestThingChangeAction?.Invoke(page);
            }

            var order = (int)NUDOrder.Value;
            var adjustPageList = testPageList.Where(p => p.Order >= order && p.Id != _pageId).OrderBy(p => p.Order).ToList();
            if (adjustPageList.Any() && adjustPageList.First().Order == order)
            {
                foreach(var p in adjustPageList)
                {
                    p.Order = ++order;

                    BigEntityTableEngine.LocalEngine.Update(nameof(TestPage), p);
                }
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
