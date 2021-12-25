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
using AutoTest.UI.WebBrowser;
using Newtonsoft.Json;

namespace AutoTest.UI.UC
{
    public partial class UCTestCaseSelector : UserControl
    {
        private List<TestSource> _testSources = null;
        private List<TestSite> _testSites = null;
        private List<TestPage> _testPages = null;
        private List<TestTask> _testCases = null;
        private List<int> _testCasesChoose = null;

        private bool _isLoad = false;
        
        public UCTestCaseSelector()
        {
            InitializeComponent();
        }

        public void Init(List<TestSource> testSources, List<TestSite> testSites, List<TestPage> testPages, List<TestTask> testCases,
            List<int> testCasesChoose)
        {
            _testSources = testSources;
            _testSites = testSites;
            _testPages = testPages;
            _testCases = testCases;
            _testCasesChoose = testCasesChoose;

            if (_isLoad)
            {
                this.CBBroswer.GetBrowser().MainFrame.EvaluateScriptAsync($"clearResults()");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CBBroswer.MenuHandler = new MenuHandler();
            CBBroswer.FrameLoadEnd += CBBroswer_FrameLoadEnd;
            CBBroswer.LoadUrl($@"{LJC.FrameWorkV3.Comm.CommFun.GetCurrentAppForder()}HTML\JqTableTree\index.html");
        }

        private void CBBroswer_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            //CBBroswer.GetBrowser().GetHost().ShowDevTools();
            ShowTable();
            _isLoad = true;
        }

        public List<TestTask> GetSelecteCase()
        {
            var ret=CBBroswer.GetBrowser().MainFrame.EvaluateScriptAsync("getSelCaseId()").Result;

            if (ret.Success)
            {
                var idList = (ret.Result as List<object>)?.Select(p => (int)p).ToList();
                if (idList != null && idList.Count > 0)
                {
                    return _testCases.Where(p => idList.Contains(p.TestCase.Id)).ToList();
                }
            }

            return new List<TestTask>();
        }

        public void SetTestResult(TestResult testResult)
        {
            var flg = 4;
            var msg = string.Empty;
            
            if (testResult.Success && testResult.HasWarn)
            {
                msg = testResult.WainMsg;
                flg = 2;
            }
            else if (testResult.Success)
            {
                msg = "成功";
                flg = 1;
            }
            else if (!testResult.Success)
            {
                msg = testResult.FailMsg??"未有错误信息";
                flg = 3;
            }
            else if (testResult.IsTimeOut)
            {
                msg = "超时";
                flg = 4;
            }

            msg = msg.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("'", "\'");
            CBBroswer.GetBrowser().MainFrame.EvaluateScriptAsync($"setTestCaseMsg('{testResult.TestCaseId}',{flg},'{msg}')");
        }

        public void ShowTable()
        {
            StringBuilder sb = new StringBuilder();

            foreach(var s in _testSources.OrderBy(p => p.SourceName))
            {
                sb.Append($"<tr data-tt-id='{s.Id}' class='branch collapsed'><td><span class='folder'><input class='casecb' type='checkbox' />{s.SourceName}</span></td><td>资源</td><td class='testresult'></td></tr>");

                foreach(var t in _testSites.Where(p => p.SourceId == s.Id).OrderBy(p => p.Name))
                {
                    sb.Append($"<tr data-tt-id='{s.Id}-{t.Id}' data-tt-parent-id='{s.Id}' class='branch' style='display:none;'><td><span class='indenter'></span><span class='folder'><input class='casecb' type='checkbox' />{t.Name}</span></td><td>测试站点</td><td class='testresult'></td></tr>");

                    foreach (var g in _testPages.Where(p => p.SiteId == t.Id).OrderBy(p => p.Order))
                    {
                        sb.Append($"<tr data-tt-id='{s.Id}-{t.Id}-{g.Id}' data-tt-parent-id='{s.Id}-{t.Id}' class='branch' style='display:none;'><td><span class='indenter'></span><span class='folder'><input class='casecb' type='checkbox' />{g.Name}</span></td><td>页面</td><td class='testresult'></td></tr>");

                        foreach(var c in _testCases.Where(p => p.TestCase.PageId == g.Id).OrderBy(p => p.TestCase.Order))
                        {
                            sb.Append($"<tr data-tt-id='{s.Id}-{t.Id}-{g.Id}-{c.TestCase.Id}' case-id='{c.TestCase.Id}' data-tt-parent-id='{s.Id}-{t.Id}-{g.Id}' class='leaf' style='display:none;'><td><span class='indenter'></span><span class='file'><input class='casecb' type='checkbox' />{c.TestCase.CaseName}</span></td><td>测试用例</td><td class='testresult tv_ready'></td></tr>");
                        }
                    }
                }
            }

            this.CBBroswer.GetBrowser().MainFrame.EvaluateScriptAsync($"showTable(\"\",\"{sb}\")");
            this.CBBroswer.GetBrowser().MainFrame.EvaluateScriptAsync("chooseTestCases("+JsonConvert.SerializeObject(_testCasesChoose) +")");

        }

    }
}
