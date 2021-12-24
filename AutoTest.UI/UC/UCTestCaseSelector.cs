using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTest.Domain.Entity;

namespace AutoTest.UI.UC
{
    public partial class UCTestCaseSelector : UserControl
    {
        private List<TestSource> _testSources = null;
        private List<TestSite> _testSites = null;
        private List<TestPage> _testPages = null;
        private List<TestCase> _testCases = null;
        public UCTestCaseSelector()
        {
            InitializeComponent();
        }

        public void Init(List<TestSource> testSources, List<TestSite> testSites, List<TestPage> testPages, List<TestCase> testCases)
        {
            _testSources = testSources;
            _testSites = testSites;
            _testPages = testPages;
            _testCases = testCases;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CBBroswer.FrameLoadEnd += CBBroswer_FrameLoadEnd;
            CBBroswer.LoadUrl($@"{LJC.FrameWorkV3.Comm.CommFun.GetCurrentAppForder()}HTML\JqTableTree\index.html");
        }

        private void CBBroswer_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            CBBroswer.GetBrowser().GetHost().ShowDevTools();
            ShowTable();
        }

        public void ShowTable()
        {
            StringBuilder sb = new StringBuilder();

            foreach(var s in _testSources.OrderBy(p => p.SourceName))
            {
                sb.Append($"<tr data-tt-id='{s.Id}' class='branch collapsed'><td><span class='folder'>{s.SourceName}</span></td><td>资源</td><td></td></tr>");

                foreach(var t in _testSites.Where(p => p.SourceId == s.Id).OrderBy(p => p.Name))
                {
                    sb.Append($"<tr data-tt-id='{s.Id}-{t.Id}' data-tt-parent-id='{s.Id}' class='branch' style='display:none;'><td><span class='indenter'></span><span class='folder'>{t.Name}</span></td><td>测试站点</td><td>--</td></tr>");

                    foreach (var g in _testPages.Where(p => p.SiteId == t.Id).OrderBy(p => p.Order))
                    {
                        sb.Append($"<tr data-tt-id='{s.Id}-{t.Id}-{g.Id}' data-tt-parent-id='{s.Id}-{t.Id}' class='branch' style='display:none;'><td><span class='indenter'></span><span class='folder'>{g.Name}</span></td><td>页面</td><td>--</td></tr>");

                        foreach(var c in _testCases.Where(p => p.PageId == g.Id).OrderBy(p => p.Order))
                        {
                            sb.Append($"<tr data-tt-id='{s.Id}-{t.Id}-{g.Id}-{c.Id}' data-tt-parent-id='{s.Id}-{t.Id}-{g.Id}' class='leaf' style='display:none;'><td><span class='indenter'></span><span class='file'>{c.CaseName}</span></td><td>case名称</td><td></td></tr>");
                        }
                    }
                }
            }
            
            this.CBBroswer.GetBrowser().MainFrame.EvaluateScriptAsync($"showTable(\"{sb.ToString()}\")");

        }

    }
}
