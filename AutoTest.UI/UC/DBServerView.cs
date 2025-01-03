﻿using AutoTest.Biz;
using AutoTest.Domain;
using AutoTest.Domain.Entity;
using AutoTest.UI.SubForm;
using AutoTest.UI.WebTask;
using LJC.FrameWorkV3.Comm;
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

namespace AutoTest.UI.UC
{
    public partial class DBServerView : UserControl
    {
        private static TestLogin currentTestLogin = null;
        private System.Timers.Timer autoTimer = null;
        public DBServerView()
        {
            InitializeComponent();
            ts_serchKey.Height = 20;
            tv_DBServers.ImageList = new ImageList();

            tv_DBServers.ImageList.Images.Add(Resources.Resource1.ForderClose);
            tv_DBServers.ImageList.Images.Add(Resources.Resource1.ForderDB);
            tv_DBServers.ImageList.Images.Add(Resources.Resource1.ForderOpen);
            tv_DBServers.ImageList.Images.Add("LOADING", Resources.Resource1.loading);
            tv_DBServers.ImageList.Images.Add("API", Resources.Resource1.DB7);
            tv_DBServers.ImageList.Images.Add("COL", Resources.Resource1.DB6);
            tv_DBServers.ImageList.Images.Add("COLQ", Resources.Resource1.ColQ);
            tv_DBServers.ImageList.Images.Add("LOGIC", Resources.Resource1.logic);
            tv_DBServers.ImageList.Images.Add("DOC", Resources.Resource1.Index);
            tv_DBServers.ImageList.Images.Add("FPAGE", Resources.Resource1.folder_page);//9
            tv_DBServers.ImageList.Images.Add("CASE", Resources.Resource1.page_world);
            tv_DBServers.ImageList.Images.Add("SITE", Resources.Resource1.site);
            tv_DBServers.ImageList.Images.Add("BAG", Resources.Resource1.application_double);
            tv_DBServers.ImageList.Images.Add("ENV", Resources.Resource1.folder_palette);//13
            tv_DBServers.ImageList.Images.Add("BOX", Resources.Resource1.box);
            tv_DBServers.ImageList.Images.Add("USERLOGIN", Resources.Resource1.user_earth);
            tv_DBServers.ImageList.Images.Add("FORDERSELECTED", Resources.Resource1.folder_star);//16
            tv_DBServers.ImageList.Images.Add("SCRIPTCODE", Resources.Resource1.script_code);
            tv_DBServers.ImageList.Images.Add("bullet_white", Resources.Resource1.bullet_white);
            tv_DBServers.ImageList.Images.Add("bullet_green", Resources.Resource1.bullet_green);
            tv_DBServers.ImageList.Images.Add("bullet_red", Resources.Resource1.bullet_red);//20
            tv_DBServers.ImageList.Images.Add("bullet_yellow", Resources.Resource1.bullet_yellow);
            tv_DBServers.ImageList.Images.Add("user_star", Resources.Resource1.user_star);
            tv_DBServers.ImageList.Images.Add("group", Resources.Resource1.group);
            tv_DBServers.ImageList.Images.Add("package", Resources.Resource1.package);//24
            tv_DBServers.ImageList.Images.Add("box", Resources.Resource1.box);

            tv_DBServers.Nodes.Add(new TreeNodeEx("资源管理器", 0, 1));
            tv_DBServers.BeforeExpand += Tv_DBServers_BeforeExpand;
            tv_DBServers.BeforeCollapse += Tv_DBServers_BeforeCollapse;
            tv_DBServers.NodeMouseClick += new TreeNodeMouseClickEventHandler(tv_DBServers_NodeMouseClick);
            tv_DBServers.NodeMouseDoubleClick += Tv_DBServers_NodeMouseDoubleClick;

            tv_DBServers.HideSelection = false;
            this.DBServerviewContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(OnMenuStrip_ItemClicked);
        }

        private void Tv_DBServers_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node is Biz.TreeNodeEx)
            {
                e.Node.ImageIndex = e.Node.SelectedImageIndex = (e.Node as Biz.TreeNodeEx).CollapseImgIndex;
            }
        }

        private void Tv_DBServers_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node is Biz.TreeNodeEx)
            {
                e.Node.ImageIndex = e.Node.SelectedImageIndex = (e.Node as Biz.TreeNodeEx).ExpandImgIndex;
            }
        }

        T FindParentNode<T>(TreeNode node)
        {
            var parent = node;
            while (parent != null)
            {
                if (parent.Tag is T)
                {
                    return (T)parent.Tag;
                }
                parent = parent.Parent;
            }

            return default(T);
        }

        TreeNode FindParentNodeNode<T>(TreeNode node)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                if (parent.Tag is T)
                {
                    return parent;
                }
                parent = parent.Parent;
            }

            return null;
        }

        TreeNode FindParentNode(TreeNode node, NodeContentType nodeContentType)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                if (parent.Tag is INodeContents && (parent.Tag as INodeContents).GetNodeContentType() == nodeContentType)
                {
                    return parent;
                }
                parent = parent.Parent;
            }

            return null;
        }


        void ReLoadDBObj(TreeNode selNode, bool loadall = true)
        {
            //TreeNode selNode = tv_DBServers.SelectedNode;
            if (selNode == null)
                return;
            AsyncCallback callback = null;
            if (loadall)
            {
                callback = new AsyncCallback((o) =>
                {
                    var node = selNode;
                    foreach (TreeNode c in node.Nodes)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            ReLoadDBObj(c, loadall);
                        }));
                    }
                });
            }
            if (selNode.Level == 0)
            {
                Biz.UILoadHelper.LoadTestResurceAsync(this.ParentForm, selNode, callback, selNode);
            }
            else if (selNode.Tag is TestSource)
            {
                var sid = (selNode.Tag as TestSource).Id;
                Biz.UILoadHelper.LoadTestSiteAsync(this.ParentForm, selNode, sid, callback, selNode);
            }
            else if (selNode.Tag is TestSite)
            {
                var sid = FindParentNode<TestSite>(selNode).Id;
                Biz.UILoadHelper.LoadTestPageAsync(ParentForm, selNode, sid, callback, selNode);
            }
            else if (selNode.Tag is TestPage)
            {
                var pid = (selNode.Tag as TestPage).Id;
                var envData = GetCurrEnvData(selNode);
                var envId = 0;
                if (envData.env != null)
                {
                    envId = envData.env.Id;
                }
                Biz.UILoadHelper.LoadTestCaseAsync(this.ParentForm, selNode, pid, envId, callback, selNode);
            }
            else if (selNode.Tag is INodeContents && (selNode.Tag as INodeContents).GetNodeContentType() == NodeContentType.ENVPARENT)
            {
                var sid = FindParentNode<TestSite>(selNode).Id;
                Biz.UILoadHelper.LoadTestEnvAsync(ParentForm, selNode, sid, callback, selNode);
            }
            else if (selNode.Tag is TestEnv)
            {
                var sid = FindParentNode<TestSite>(selNode).Id;
                var envid = (selNode.Tag as TestEnv).Id;
                Biz.UILoadHelper.LoadTestEnvParamsAsync(this.ParentForm, selNode, sid, envid, callback, selNode);
            }
            else if (selNode.Tag is INodeContents && (selNode.Tag as INodeContents).GetNodeContentType() == NodeContentType.SCRIPTPARENT)
            {
                var sid = FindParentNode<TestSource>(selNode).Id;
                var testSite = FindParentNode<TestSite>(selNode);
                Biz.UILoadHelper.LoadTestScriptAsync(this.ParentForm, selNode, sid, testSite == null ? 0 : testSite.Id, callback, selNode);
            }
            else if (selNode.Tag is INodeContents && (selNode.Tag as INodeContents).GetNodeContentType() == NodeContentType.LOGINACCOUNTS)
            {
                var testSite = FindParentNode<TestSite>(selNode);
                Biz.UILoadHelper.LoadLoginAccountsAsync(this.ParentForm, selNode, testSite.Id, callback, selNode);
            }
            else if (selNode.Tag is INodeContents && (selNode.Tag as INodeContents).GetNodeContentType() == NodeContentType.TESTBAG)
            {
                var sid = FindParentNode<TestSite>(selNode).Id;
                Biz.UILoadHelper.LoadTestBagAsync(this.ParentForm, selNode, sid, callback, selNode);
            }
        }

        private (TestEnv env, List<TestEnvParam> envParams, bool hasEvn) GetCurrEnvData(TreeNode node)
        {
            var testSite = FindParentNode<TestSite>(node);
            if (testSite == null)
            {
                return (null, null, false);
            }

            var testEnvs = BigEntityTableRemotingEngine.Find<TestEnv>(nameof(TestEnv), nameof(TestEnv.SiteId), new object[] { testSite.Id });
            var currentEnv = testEnvs.FirstOrDefault(p => p.Used);
            List<TestEnvParam> testEnvParams = null;
            if (currentEnv != null)
            {
                testEnvParams = BigEntityTableRemotingEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_EnvId", new object[] { testSite.Id, currentEnv.Id }).ToList();
            }
            return (currentEnv, testEnvParams, testEnvs.Count() > 0);
        }

        private List<TreeNode> GetTestCaseTaskList(TreeNode node)
        {
            var list = new List<TreeNode>();

            if (node.Tag is TestCase)
            {
                list.Add(node);
            }

            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode n in node.Nodes)
                {
                    list.AddRange(GetTestCaseTaskList(n));
                }
            }

            return list;
        }

        private TreeNode FindNode(TreeNodeCollection topNodes, object tag)
        {
            foreach (TreeNode node in topNodes)
            {
                if (node.Tag != null && tag != null)
                {
                    if (node.Tag.GetType() == tag.GetType()
                        && (node.Tag as IComparable) != null)
                    {
                        if ((node.Tag as IComparable).CompareTo(tag) == 0)
                        {
                            return node;
                        }
                    }
                }

                if (node.Nodes.Count > 0)
                {
                    var findNode = FindNode(node.Nodes, tag);
                    if (findNode != null)
                    {
                        return findNode;
                    }
                }
            }

            return null;
        }

        private string GetTestTaskName(TreeNode selnode)
        {

            var source = FindParentNode<TestSource>(selnode);
            var page = FindParentNode<TestPage>(selnode);
            var site = FindParentNode<TestSite>(selnode);
            var @case = FindParentNode<TestCase>(selnode);
            if (source == null)
            {
                return null;
            }
            var listName = source.SourceName;

            if (site != null)
            {
                listName += "_" + site.Name;
            }

            if (page != null)
            {
                listName += "_" + page.Name;
            }

            if (@case != null)
            {
                listName += "_" + @case.CaseName;
            }

            return listName;
        }

        void RunTest(TreeNode selnode)
        {
            List<TestSource> sources = new List<TestSource>();
            List<TestSite> testSites = new List<TestSite>();
            List<TestPage> testPages = new List<TestPage>();
            List<TestCase> testCases = new List<TestCase>();
            var testTaskList = new List<TestTask>();
            var testLoginList = BigEntityTableRemotingEngine.List<TestLogin>(nameof(TestLogin), 1, int.MaxValue).ToList();
            var tabName = "";

            if (selnode.Tag is TestTaskBag)
            {
                var testTaskBag = selnode.Tag as TestTaskBag;
                sources.Add(FindParentNode<TestSource>(selnode));
                testSites.Add(FindParentNode<TestSite>(selnode));
                testCases.AddRange(BigEntityTableRemotingEngine.FindBatch<TestCase>(nameof(TestCase), testTaskBag.CaseId.Select(p => (object)p)));
                testPages.AddRange(BigEntityTableRemotingEngine.FindBatch<TestPage>(nameof(TestPage), testCases.Select(p => (object)p.PageId).Distinct()));
                var globalScripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == sources.First().Id && s.SiteId == 0).ToList();
                var siteScripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == sources.First().Id && s.SiteId == testSites.First().Id).ToList();
                var env = BigEntityTableRemotingEngine.Find<TestEnv>(nameof(TestEnv), testTaskBag.TestEnvId);
                var envParams = env == null ? new List<TestEnvParam>() : BigEntityTableRemotingEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_EnvId", new object[] { testSites.First().Id, env.Id }).ToList();

                testCases = testCases.OrderBy(p =>
                 {
                     return testPages.FirstOrDefault(q => q.Id == p.PageId)?.Order;
                 }).ThenBy(p =>
                 {
                     return p.Order;
                 }).ToList();

                foreach (var tc in testCases)
                {
                    var testLogin = GetTestLogin(tc, testSites.First());

                    testTaskList.Add(new TestTask
                    {
                        TestSource = sources.First(),
                        SiteTestScripts = siteScripts,
                        GlobalTestScripts = globalScripts,
                        TestCase = tc,
                        TestLogin = testLogin,
                        TestPage = testPages.Find(x => x.Id == tc.PageId),
                        TestSite = testSites.First(),
                        TestEnv = env,
                        TestEnvParams = envParams
                    });
                }

                tabName = $"测试包_{testTaskBag.BagName}";
            }
            else
            {
                var testCaseNodeList = GetTestCaseTaskList(selnode);
                var scriptsDic = new Dictionary<object, List<TestScript>>();
                var envDic = new Dictionary<object, (TestEnv env, List<TestEnvParam> envParams, bool hasEvn)>();


                foreach (var node in testCaseNodeList)
                {
                    var testSource = FindParentNode<TestSource>(node);
                    var testPage = FindParentNode<TestPage>(node);
                    var testSite = FindParentNode<TestSite>(node);
                    var testCase = FindParentNode<TestCase>(node);

                    if (!sources.Any(p => p.Id == testSource.Id))
                    {
                        sources.Add(testSource);
                    }

                    if (!testSites.Any(p => p.Id == testSite.Id))
                    {
                        testSites.Add(testSite);
                    }

                    if (!testPages.Any(p => p.Id == testPage.Id))
                    {
                        testPages.Add(testPage);
                    }

                    testCases.Add(testCase);

                    var testLogin = GetTestLogin(testCase, testSite);

                    //if (testLogin != null && testLogin.Id != testCase.OnlyUserId && testCase.OnlyUserId != 0)
                    //{
                    //    continue;
                    //}

                    var key = "globalScripts_" + testSource.Id;
                    List<TestScript> globalScripts = null;
                    if (!scriptsDic.TryGetValue(key, out globalScripts))
                    {
                        globalScripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == 0).ToList();
                        scriptsDic.Add(key, globalScripts);
                    }

                    key = "siteScripts_" + testSource.Id + "_" + testSite.Id;
                    List<TestScript> siteScripts = null;
                    if (!scriptsDic.TryGetValue(key, out siteScripts))
                    {
                        siteScripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == testSite.Id).ToList();
                        scriptsDic.Add(key, siteScripts);
                    }
                    (TestEnv env, List<TestEnvParam> envParams, bool hasEvn) ep;
                    if (!envDic.TryGetValue(testSite.Id, out ep))
                    {
                        ep = GetCurrEnvData(node);
                        envDic.Add(testSite.Id, ep);
                    }
                    if (ep.hasEvn && ep.env == null)
                    {
                        MessageBox.Show("请选择一个测试环境");
                        return;
                    }
                    testTaskList.Add(new TestTask
                    {
                        TestSource = testSource,
                        SiteTestScripts = siteScripts,
                        GlobalTestScripts = globalScripts,
                        TestCase = testCase,
                        TestLogin = testLogin,
                        TestPage = testPage,
                        TestSite = testSite,
                        TestEnv = ep.env,
                        TestEnvParams = ep.envParams
                    });
                }

                tabName = $"测试_{GetTestTaskName(selnode)} {testTaskList.First().TestEnv?.EnvName}";

            }

            if (selnode.Tag is TestCase)
            {
                if (new ConfirmDlg("询问", "执行测试吗？").ShowDialog() == DialogResult.OK)
                {
                    var testPanel = (TestPanel)Util.TryAddToMainTab(this, $"执行单个测试", () =>
                    {
                        var panel = new TestPanel("执行单个测试");

                        panel.OnTaskStart += t =>
                        {
                            var rt = t as RunTestTask;
                            if (rt != null && rt.TestLogin != null && (currentTestLogin == null || currentTestLogin.Id != rt.TestLogin.Id))
                            {
                                currentTestLogin = rt.TestLogin;
                                panel.ClearCookie(rt.TestLogin.Url);

                                var cookies = TestCookieContainerBiz.GetCookies(rt.TestLogin.SiteId, rt.TestEnv?.Id, rt.TestLogin.Id);
                                if (cookies?.Count > 0)
                                {
                                    panel.SetCookie(rt.GetStartPageUrl(), cookies);
                                }
                            }
                        };

                        panel.Load();
                        return panel;
                    }, null);

                    if (testPanel.IsRunning())
                    {
                        Util.SendMsg(this, "正在执行测试，请稍后再试");
                        return;
                    }

                    if (!testPanel.Reset())
                    {
                        Util.SendMsg(this, "任务未开始，有测试在执行");
                        return;
                    }


                    TaskHelper.SetInterval(1000, () =>
                    {
                        var runTaskList = testTaskList.Select(task => new RunTestTask(task.GetTaskName(), false, task.TestSite, task.TestLogin, task.TestPage, task.TestCase, task.TestEnv, task.TestEnvParams, task.GlobalTestScripts, task.SiteTestScripts, task.ResultNotify));
                        BeginInvoke(new Action(() => testPanel.RunTest(runTaskList)));
                        return true;
                    }, runintime: false);
                }
            }
            else
            {
                var testTasksView = (TestCaseTaskView)Util.TryAddToMainTab(this, tabName, () =>
                {
                    var panel = new TestCaseTaskView();
                    return panel;
                }, null);

                testTasksView.Init(selnode.Tag as TestTaskBag, sources, testSites, testPages, testTaskList, testTaskList.Select(p => p.TestCase.Id).ToList());
            }

            TestLogin GetTestLogin(TestCase testCase, TestSite testSite)
            {
                TestLogin testLogin = null;
                if (testCase.OnlyUserId > 0)
                {
                    testLogin = testLoginList.FirstOrDefault(p => p.Id == testCase.OnlyUserId && p.SiteId == testSite.Id);
                    if (testLogin == null)
                    {
                        Util.SendMsg(this, $"账号不存在：{testCase.OnlyUserId}");
                        return testLogin;
                    }
                }
                else
                {
                    if (testLoginList.Where(p => p.SiteId == testSite.Id).Count() == 1)
                    {
                        testLogin = testLoginList.FirstOrDefault(p => p.SiteId == testSite.Id);
                    }
                    else if (testLoginList.Where(p => p.SiteId == testSite.Id).Count() > 1)
                    {
                        testLogin = testLoginList.FirstOrDefault(p => p.SiteId == testSite.Id && p.Used);
                        if (testLogin == null)
                        {
                            MessageBox.Show($"{testSite.Name}:请选择一个测试帐号");
                            return testLogin;
                        }
                    }

                }

                return testLogin;
            }
        }

        private void NotifyTestResult(TestResult r)
        {
            BeginInvoke(new Action(() =>
            {
                var lastNode = FindNode(tv_DBServers.Nodes, new TestCase
                {
                    Id = r.TestCaseId
                });
                if (lastNode == null)
                {
                    return;
                }
                tv_DBServers.SelectedNode = lastNode;
                var nodeEx = (TreeNodeEx)lastNode;
                var imgIndex = 19;
                if (r.Success)
                {
                    if (r.HasWarn)
                    {
                        imgIndex = 21;
                    }
                }
                else
                {
                    imgIndex = 20;
                }
                nodeEx.SelectedImageIndex = nodeEx.ImageIndex = nodeEx.ExpandImgIndex = nodeEx.CollapseImgIndex = imgIndex;
            }));

        }

        private void NotifyTestStart(IWebTask webTask)
        {
            var testcase = (webTask as RunTestTask)?.TestCase;
            if (testcase == null)
            {
                return;
            }
            var lastNode = FindNode(tv_DBServers.Nodes, testcase);
            if (lastNode == null)
            {
                return;
            }
            tv_DBServers.SelectedNode = lastNode;
            var nodeEx = lastNode as TreeNodeEx;
            if (nodeEx != null)
            {
                nodeEx.SelectedImageIndex = nodeEx.ImageIndex = nodeEx.ExpandImgIndex = nodeEx.CollapseImgIndex = 18;
            }
        }

        private void NotifyTestThingAdd(IUpdate update)
        {
            var comparable = update.GetParentUpdate();
            var node = FindNode(tv_DBServers.Nodes, comparable);
            if (node != null)
            {
                var parentNode = node.Parent;
                ReLoadDBObj(parentNode, true);

                node = FindNode(parentNode.Nodes, comparable);
                if (node != null)
                {
                    tv_DBServers.SelectedNode = node;
                }
            }
        }

        private void NotifyTestThingChange(IUpdate comparable)
        {
            var node = FindNode(tv_DBServers.Nodes, comparable);
            if (node != null)
            {
                if (comparable.GetDisplayText() != node.Text)
                {
                    node.Text = comparable.GetDisplayText();
                }

                node.Tag = comparable;
            }
        }

        public void SelectTestCase(int caseid)
        {
            var node = FindNode(tv_DBServers.Nodes, new TestCase { Id = caseid });
            if (node != null)
            {
                tv_DBServers.SelectedNode = node;
            }
        }

        private void UpdateNode(TreeNode node, string text, object tag)
        {
            if (node.Text != text)
            {
                node.Text = text;
            }

            node.Tag = tag;
        }

        void OnMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                DBServerviewContextMenuStrip.Visible = false;
                var selnode = tv_DBServers.SelectedNode;
                if (selnode == null)
                {
                    return;
                }
                switch (e.ClickedItem.Text)
                {
                    case "添加测试资源":
                        {
                            if (new AddTestSource().ShowDialog() == DialogResult.OK)
                            {
                                Bind();
                            }
                            break;
                        }
                    case "编辑":
                        {
                            if (selnode.Tag is TestCase)
                            {
                                var testSite = FindParentNode<TestSite>(selnode);
                                var testPage = FindParentNode<TestPage>(selnode);
                                var testCase = selnode.Tag as TestCase;

                                _ = (UCAddCaseParam)Util.TryAddToMainTab(this, $"[{testSite.Name}]-{testPage.Name}-{testCase.CaseName}", () => new UCAddCaseParam(testSite, testPage, testCase));
                            }
                            else if (selnode.Tag is TestPage)
                            {
                                var testSite = FindParentNode<TestSite>(selnode);
                                var dlg = new AddTestPageDlg(testSite.Id, (selnode.Tag as TestPage).Id);
                                dlg.ShowDialog();
                            }
                            else if (selnode.Tag is TestSite)
                            {
                                var testSource = FindParentNode<TestSource>(selnode);
                                AddTestSiteDlg dlg = new AddTestSiteDlg(testSource.Id, (selnode.Tag as TestSite).Id);
                                dlg.ShowDialog();
                            }
                            else if (selnode.Tag is TestSource)
                            {
                                if (new AddTestSource((selnode.Tag as TestSource).Id).ShowDialog() == DialogResult.OK)
                                {
                                    Bind();
                                }
                            }
                            else if (selnode.Tag is TestLogin)
                            {
                                var testSite = FindParentNode<TestSite>(selnode);
                                var dlg = new AddTestLoginDlg(testSite.Id, selnode.Tag as TestLogin);
                                dlg.ShowDialog();
                            }
                            else if (selnode.Tag is TestScript)
                            {
                                var testScript = selnode.Tag as TestScript;
                                var testResource = FindParentNode<TestSource>(selnode);
                                var testSite = FindParentNode<TestSite>(selnode);
                                var scripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testResource.Id && s.SiteId == 0).ToList();

                                Util.AddToMainTab(this, $"{testResource.SourceName}_{testSite?.Name}_{testScript.ScriptName}(脚本)",
                                        new UCTestScript(testScript, scripts));
                            }
                            else if (selnode.Tag is TestEnvParam)
                            {
                                var testSite = FindParentNode<TestSite>(selnode);
                                var envparam = selnode.Tag as TestEnvParam;
                                if (envparam.Id == 0)
                                {
                                    envparam = BigEntityTableRemotingEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_Name", new object[] { testSite.Id, envparam.Name }).FirstOrDefault();
                                }
                                var dlg = new AddTestEnvParamDlg(testSite.Id, envparam.Id);
                                dlg.ShowDialog();
                            }
                            else if (selnode.Tag is TestTaskBag)
                            {
                                var testBag = selnode.Tag as TestTaskBag;
                                var parentNode = selnode.Parent;
                                var taskBag = Util.TryAddToMainTab(this, $"编辑测试包{testBag.BagName}", () => new UCTestTaskBagView(testBag.SiteId, testBag.Id, (t) =>
                                 {
                                     ReLoadDBObj(parentNode);
                                 }));
                                (taskBag as UCTestTaskBagView).Init();
                            }
                            break;
                        }
                    case "删除":
                        {

                            Func<bool> delFunc = null;

                            if (selnode.Tag is TestCase)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableRemotingEngine.Delete<TestCase>(nameof(TestCase), (selnode.Tag as TestCase).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestPage)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableRemotingEngine.Delete<TestPage>(nameof(TestPage), (selnode.Tag as TestPage).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestSite)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableRemotingEngine.Delete<TestSite>(nameof(TestSite), (selnode.Tag as TestSite).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestSource)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableRemotingEngine.Delete<TestSource>(nameof(TestSource), (selnode.Tag as TestSource).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestEnv)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableRemotingEngine.Delete<TestEnv>(nameof(TestEnv), (selnode.Tag as TestEnv).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestScript)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableRemotingEngine.Delete<TestScript>(nameof(TestScript), (selnode.Tag as TestScript).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestTaskBag)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableRemotingEngine.Delete<TestTaskBag>(nameof(TestTaskBag), (selnode.Tag as TestTaskBag).Id);
                                    return true;
                                };
                            }

                            if (delFunc != null)
                            {
                                if (MessageBox.Show("确认删除吗？", "删除确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                                {
                                    return;
                                }
                                if (delFunc())
                                {
                                    ReLoadDBObj(selnode.Parent);
                                }
                            }
                            break;
                        }
                    case "添加API":
                        {

                            break;
                        }
                    case "添加环境":
                        {
                            var apisource = FindParentNode<TestSite>(selnode);
                            var sourceid = apisource.Id;
                            var dlg = new AddEnvDlg(sourceid);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                this.ReLoadDBObj(selnode);
                            }
                            break;
                        }
                    case "添加环境变量":
                        {
                            var testSite = FindParentNode<TestSite>(selnode);
                            var siteId = testSite.Id;
                            var dlg = new AddTestEnvParamDlg(siteId, 0);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                this.ReLoadDBObj(selnode);
                            }
                            break;
                        }
                    case "参数定义":
                        {
                            break;
                        }
                    case "刷新":
                        {
                            ReLoadDBObj(tv_DBServers.SelectedNode);
                            break;
                        }
                    case "新增逻辑关系图":
                        {
                            break;
                        }
                    case "删除逻辑关系图":
                        {
                            break;
                        }
                    case "复制对象名":
                        {
                            Clipboard.SetText(selnode.Text);
                            Util.SendMsg(this, "已复制到剪贴板");
                            break;
                        }
                    case "添加WCF接口":
                        {
                            break;
                        }
                    case "如何使用":
                        {
                            break;
                        }
                    case "批量复制引用":
                        {
                            StringBuilder sb = new StringBuilder();

                            Clipboard.SetText(sb.ToString());
                            Util.SendMsg(this, "已复制到剪切板");
                            break;
                        }
                    case "添加测试站点":
                        {
                            var testSource = FindParentNode<TestSource>(selnode);
                            AddTestSiteDlg dlg = new AddTestSiteDlg(testSource.Id, 0);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                ReLoadDBObj(selnode);
                            }
                            break;
                        }
                    case "添加测试页面":
                        {
                            var testSite = FindParentNode<TestSite>(selnode);
                            var dlg = new AddTestPageDlg(testSite.Id, 0);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                ReLoadDBObj(selnode);
                            }
                            break;
                        }
                    case "添加测试用例":
                        {
                            var testSource = FindParentNode<TestSource>(selnode);
                            var testSite = FindParentNode<TestSite>(selnode);
                            var testPage = FindParentNode<TestPage>(selnode);
                            var scripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == 0).ToList();
                            var siteScripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == testSite.Id).ToList();
                            scripts.AddRange(siteScripts);
                            var step1dlg = new AddTestCaseDlg(testPage.Id, 0, scripts);

                            if (step1dlg.ShowDialog() == DialogResult.OK)
                            {
                                ReLoadDBObj(selnode);
                                Util.AddToMainTab(this, $"[{testSite.Name}]-{testPage.Name}-{step1dlg.TestCase.CaseName}", new UCAddCaseParam(testSite, testPage, step1dlg.TestCase));
                            }
                            break;
                        }
                    case "运行测试":
                        {
                            RunTest(selnode);
                            break;
                        }
                    case "添加登陆页":
                        {
                            var testSite = FindParentNode<TestSite>(selnode);
                            var dlg = new AddTestLoginDlg(testSite.Id, null);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                ReLoadDBObj(selnode);
                            }
                            break;
                        }
                    case "添加测试包":
                        {
                            var testBag = FindParentNode<TestSite>(selnode);

                            var tab = new UCTestTaskBagView(testBag.Id, 0, t =>
                             {
                                 ReLoadDBObj(selnode);
                             });

                            Util.AddToMainTab(this, "添加测试包", tab);

                            break;
                        }
                    case "登陆":
                        {
                            var testSite = FindParentNode<TestSite>(selnode);
                            var testLogin = FindParentNode<TestLogin>(selnode);
                            var ep = GetCurrEnvData(selnode);

                            var globalScripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSite.SourceId && s.SiteId == 0).ToList();
                            var siteScripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSite.SourceId && s.SiteId == testLogin.SiteId).ToList();

                            var testPanel = (TestPanel)Util.TryAddToMainTab(this, $"执行测试", () =>
                            {
                                var panel = new UC.TestPanel(testSite.Name);
                                panel.Load();

                                return panel;
                            }, typeof(TestPanel));

                            LJC.FrameWorkV3.Comm.TaskHelper.SetInterval(1000, () =>
                            {
                                BeginInvoke(new Action(() => testPanel.RunTest(new RunTestLoginTask(testLogin.Url, false, testSite, testLogin, ep.env, ep.envParams, globalScripts, siteScripts))));
                                return true;
                            }, runintime: false);
                            break;
                        }
                    case "退出":
                        {
                            var testSite = FindParentNode<TestSite>(selnode);

                            var testLogin = FindParentNode<TestLogin>(selnode);
                            var ep = GetCurrEnvData(selnode);
                            var testPanel = (TestPanel)Util.TryAddToMainTab(this, $"执行测试", () => null, typeof(TestPanel));
                            if (testPanel != null)
                            {
                                if (new ConfirmDlg(testSite.Name, "退出吗？", 30).ShowDialog() != DialogResult.OK)
                                {
                                    return;
                                }
                                TestCookieContainerBiz.SetExpired(testSite.Id, ep.env?.Id, testLogin.Id);
                                var loginTask = new RunTestLoginTask(testLogin.Url, false, testSite, testLogin, ep.env, ep.envParams,null,null);
                                if (testPanel.ClearCookie(loginTask.GetStartPageUrl()))
                                {
                                    Util.SendMsg(this, "cookie清理成功");
                                }
                                else
                                {
                                    Util.SendMsg(this, "cookie清理失败");
                                }
                            }
                            else
                            {
                                new AlertDlg(testSite.Name, "cookie清理失败，执行窗口未开启", null).ShowDialog();
                            }

                            break;
                        }
                    case "切换":
                        {
                            if (selnode.Tag is TestEnv)
                            {
                                var currEnv = selnode.Tag as TestEnv;
                                if (!currEnv.Used)
                                {
                                    var testSite = FindParentNode<TestSite>(selnode);

                                    var envList = BigEntityTableRemotingEngine.Find<TestEnv>(nameof(TestEnv), nameof(TestEnv.SiteId), new object[] { testSite.Id });
                                    foreach (var envUse in envList.Where(p => p.Used))
                                    {
                                        envUse.Used = false;
                                        BigEntityTableRemotingEngine.Update(nameof(TestEnv), envUse);
                                    }

                                    currEnv.Used = true;
                                    BigEntityTableRemotingEngine.Update(nameof(TestEnv), currEnv);

                                    ReLoadDBObj(selnode.Parent);
                                }
                            }
                            else if (selnode.Tag is TestLogin)
                            {
                                var currTestLogin = selnode.Tag as TestLogin;
                                if (!currTestLogin.Used)
                                {
                                    var testSite = FindParentNode<TestSite>(selnode);

                                    var testLoginList = BigEntityTableRemotingEngine.Find<TestLogin>(nameof(TestLogin), nameof(TestLogin.SiteId), new object[] { testSite.Id });
                                    foreach (var testLoginUse in testLoginList.Where(p => p.Used))
                                    {
                                        testLoginUse.Used = false;
                                        BigEntityTableRemotingEngine.Update(nameof(TestLogin), testLoginUse);
                                    }

                                    currTestLogin.Used = true;
                                    BigEntityTableRemotingEngine.Update(nameof(TestLogin), currTestLogin);

                                    ReLoadDBObj(selnode.Parent);
                                }
                            }

                            break;
                        }
                    case "添加脚本":
                        {
                            var testResource = FindParentNode<TestSource>(selnode);
                            var testSite = FindParentNode<TestSite>(selnode);
                            var scriptNameDlg = new SubForm.InputStringDlg("输入脚本名称");
                            if (scriptNameDlg.ShowDialog() == DialogResult.OK)
                            {
                                if (BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript),
                                    $"{nameof(TestScript.SourceId)}_{nameof(TestScript.SiteId)}_{nameof(TestScript.ScriptName)}",
                                    new object[] { testResource.Id, testSite == null ? 0 : testSite.Id, scriptNameDlg.InputString }).Count() > 0)
                                {
                                    Util.SendMsg(this, $"{scriptNameDlg.InputString}已经存在，不能重复添加。");
                                }
                                else
                                {
                                    var testScript = new TestScript
                                    {
                                        ScriptName = scriptNameDlg.InputString,
                                        Enable = true,
                                        SiteId = testSite == null ? 0 : testSite.Id,
                                        SourceId = testResource.Id
                                    };

                                    BigEntityTableRemotingEngine.Insert(nameof(TestScript), testScript);
                                    var scripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testResource.Id && s.SiteId == 0).ToList();
                                    ReLoadDBObj(selnode);
                                    Util.AddToMainTab(this, $"{testResource.SourceName}_{testSite?.Name}_{testScript.ScriptName}(脚本)",
                                        new UC.UCTestScript(testScript, scripts));
                                }
                            }
                            break;
                        }
                    case "复制":
                        {
                            if (selnode.Tag is TestCase)
                            {
                                var testSource = FindParentNode<TestSource>(selnode);
                                var testSite = FindParentNode<TestSite>(selnode);
                                var testPage = FindParentNode<TestPage>(selnode);
                                var scripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == 0).ToList();
                                var siteScripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == testSite.Id).ToList();
                                scripts.AddRange(siteScripts);

                                var step1dlg = new AddTestCaseDlg(testPage.Id, 0, scripts, selnode.Tag as TestCase);
                                if (step1dlg.ShowDialog() == DialogResult.OK)
                                {
                                    ReLoadDBObj(selnode.Parent);
                                    Util.AddToMainTab(this, $"[{testSite.Name}]-{testPage.Name}-{step1dlg.TestCase.CaseName}", new UC.UCAddCaseParam(testSite, testPage, step1dlg.TestCase));
                                }
                            }
                            else if (selnode.Tag is TestLogin)
                            {
                                var testSite = FindParentNode<TestSite>(selnode);
                                var currentTestLogin = selnode.Tag as TestLogin;
                                var testLogin = new TestLogin
                                {
                                    LoginCode = currentTestLogin.LoginCode,
                                    IsMannual = currentTestLogin.IsMannual,
                                    SiteId = currentTestLogin.SiteId,
                                    Url = currentTestLogin.Url,
                                    ValidCode = currentTestLogin.ValidCode,
                                    //AccountInfo=currentTestLogin.AccountInfo
                                };
                                var dlg = new AddTestLoginDlg(testSite.Id, testLogin);
                                if (dlg.ShowDialog() == DialogResult.OK)
                                {
                                    ReLoadDBObj(selnode.Parent);
                                }
                            }
                            break;
                        }
                    default:
                        {
                            MessageBox.Show(e.ClickedItem.Text);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "发生错误", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private string GetDBName(TreeNode node)
        {
            if (node == null)
                return null;
            if (node.Level < 1)
                return null;
            if (node.Level == 2)
                return node.Text;
            return GetDBName(node.Parent);
        }

        void tv_DBServers_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Nodes.Count == 0)
            {

                ReLoadDBObj(e.Node);
            }
        }


        private void Tv_DBServers_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is TestCase)
            {
                RunTest(e.Node);
            }
        }

        public void Bind()
        {
            ReLoadDBObj(tv_DBServers.Nodes[0]);
        }

        private void DBServerView_Load(object sender, EventArgs e)
        {
            ReLoadDBObj(tv_DBServers.Nodes[0], true);
        }

        public string DisConnectSelectDBServer()
        {
            if (tv_DBServers.SelectedNode == null || this.tv_DBServers.SelectedNode.Level != 1)
                return null;
            var server = tv_DBServers.SelectedNode.Text;
            DisConnectServer(server);
            return server;
        }

        private void DisConnectServer(string serverName)
        {

        }

        private void tv_DBServers_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var node = tv_DBServers.SelectedNode;
                this.tv_DBServers.ContextMenuStrip = this.DBServerviewContextMenuStrip;

                添加测试资源ToolStripMenuItem.Visible = node.Level == 0;

                添加测试站点ToolStripMenuItem.Visible = node.Tag is TestSource;

                添加APIToolStripMenuItem.Visible = (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.APIPARENT;
                添加WCF接口ToolStripMenuItem.Visible = (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.APIPARENT;

                添加环境ToolStripMenuItem.Visible = (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.ENVPARENT;
                添加环境变量ToolStripMenuItem.Visible = node.Tag is TestEnv;

                参数定义ToolStripMenuItem.Visible = (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.DOC;

                新增逻辑关系图ToolStripMenuItem.Visible = (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.LOGICMAPParent;
                删除逻辑关系图ToolStripMenuItem.Visible = (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.LOGICMAP;

                if ((node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.ENV)
                {
                    批量复制引用ToolStripMenuItem.Visible = true;
                }
                else
                {
                    批量复制引用ToolStripMenuItem.Visible = false;
                }

                切换ToolStripMenuItem.Visible = node.Tag is TestEnv
                    || node.Tag is TestLogin;

                if (node.Tag is TestSite)
                {
                    添加测试页面ToolStripMenuItem.Visible = true;
                }
                else
                {
                    添加测试页面ToolStripMenuItem.Visible = false;
                }

                添加登陆页ToolStripMenuItem.Visible = (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.LOGINACCOUNTS;

                添加测试用例ToolStripMenuItem.Visible = node.Tag is TestPage;

                运行测试ToolStripMenuItem.Visible = node.Tag is TestCase
                    || node.Tag is TestPage
                    || node.Tag is TestSite
                    || node.Tag is TestSource
                    || node.Tag is TestTaskBag;

                登陆ToolStripMenuItem.Visible = 退出ToolStripMenuItem.Visible = node.Tag is TestLogin;

                添加脚本ToolStripMenuItem.Visible = (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.SCRIPTPARENT;

                复制ToolStripMenuItem.Visible = node.Tag is TestCase || node.Tag is TestLogin;

                添加测试包ToolStripMenuItem.Visible = (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.TESTBAG;
            }

        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {
            string serchkey = ts_serchKey.Text;

            bool matchall = serchkey.StartsWith("'");
            if (matchall)
            {
                serchkey = serchkey.Trim('\'');
            }
            if (!ts_serchKey.Items.Contains(serchkey))
            {
                ts_serchKey.Items.Add(serchkey);
            }
            if (tv_DBServers.SelectedNode == null)
            {
                tv_DBServers.SelectedNode = tv_DBServers.Nodes[0];
            }

            if (!tv_DBServers.Focused)
            {
                this.tv_DBServers.Focus();
            }

            bool boo = false;
            if (tv_DBServers.SelectedNode.Nodes.Count > 0)
                boo = SearchNode(tv_DBServers.SelectedNode.Nodes[0], serchkey, matchall, true);
            else if (tv_DBServers.SelectedNode.NextNode != null)
                boo = SearchNode(tv_DBServers.SelectedNode.NextNode, serchkey, matchall, true);
            else
            {
                var parent = tv_DBServers.SelectedNode.Parent;
                while (parent != null && parent.NextNode == null)
                {
                    parent = parent.Parent;
                }
                if (parent != null)
                {
                    if (parent.NextNode != null)
                    {
                        boo = SearchNode(parent.NextNode, serchkey, matchall, true);
                    }
                }
            }

            if (!boo)
            {
                tv_DBServers.SelectedNode = tv_DBServers.Nodes[0];
            }

        }

        private bool SearchNode(TreeNode nodeStart, string txt, bool matchall, bool maxsearch)
        {
            if (nodeStart == null)
            {
                return false;
            }
            var text = nodeStart.Text;
            var find = matchall ? text.Equals(txt, StringComparison.OrdinalIgnoreCase) : text.IndexOf(txt, StringComparison.OrdinalIgnoreCase) > -1;
            if (!find)
            {
                if ((nodeStart.Tag as ISearch) != null)
                {
                    find = (nodeStart.Tag as ISearch).Search(txt);
                }
            }
            if (!find)
            {

            }
            if (find)
            {
                tv_DBServers.SelectedNode = nodeStart;
                return true;
            }
            if (nodeStart.Nodes.Count > 0)
            {
                foreach (TreeNode node in nodeStart.Nodes)
                {
                    if (SearchNode(node, txt, matchall, false))
                        return true;
                }
            }

            if (maxsearch)
            {
                if (nodeStart.NextNode != null)
                {
                    return SearchNode(nodeStart.NextNode, txt, matchall, true);
                }
                else
                {
                    if (maxsearch)
                    {
                        var parent = nodeStart.Parent;
                        while (parent != null && parent.NextNode == null)
                        {
                            parent = parent.Parent;
                        }
                        if (parent != null)
                        {
                            return SearchNode(parent.NextNode, txt, matchall, true);
                        }
                    }
                }
            }

            return false;
        }

        private void ts_serchKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                tv_DBServers.Focus();
                toolStripDropDownButton1_Click(null, null);
            }
        }

        public TreeNode FindNode(string serverName, string dbName = null, string tbName = null)
        {
            foreach (TreeNode node in tv_DBServers.Nodes[0].Nodes)
            {
                if (node.Text.Equals(serverName, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(dbName))
                        return node;
                    foreach (TreeNode subNode in node.Nodes)
                    {
                        if (subNode.Text.Equals(dbName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.IsNullOrWhiteSpace(tbName))
                                return subNode;
                            foreach (TreeNode subSubNode in subNode.Nodes)
                            {
                                if (subSubNode.Text.Equals(tbName, StringComparison.OrdinalIgnoreCase))
                                    return subSubNode;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void SubMenuItem_Insert_Click(object sender, EventArgs e)
        {

        }

        private void Mark_Local()
        {


        }

        private void 备注本地ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mark_Local();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            EventBus.SelectTestCaseAction += SelectTestCase;
            EventBus.NotifyTestResultAction += NotifyTestResult;
            EventBus.NotifyTestStartAction += NotifyTestStart;
            EventBus.NotifyTestThingChangeAction += NotifyTestThingChange;

            autoTimer = TaskHelper.SetInterval(60 * 1000, () =>
              {
                  var allList = new AutoTaskBiz().GetNextRunTaskBagList();
                  foreach (var nextList in allList.GroupBy(p => p.SiteId).Select(p => p.ToList()))
                  {
                      List<TestSource> sources = new List<TestSource>();
                      List<TestSite> testSites = new List<TestSite>();
                      List<TestPage> testPages = new List<TestPage>();
                      List<TestCase> testCases = new List<TestCase>();
                      Dictionary<string, object> cach = new Dictionary<string, object>();
                      var testTaskList = new List<TestTask>();
                      var testLoginList = BigEntityTableRemotingEngine.List<TestLogin>(nameof(TestLogin), 1, int.MaxValue).ToList();

                      foreach (var testTaskBag in nextList)
                      {
                          var testSite = testSites.FirstOrDefault(p => p.Id == testTaskBag.SiteId);
                          if (testSite == null)
                          {
                              testSite = BigEntityTableRemotingEngine.Find<TestSite>(nameof(TestSite), testTaskBag.SiteId);
                              testSites.Add(testSite);
                          }

                          var testSource = sources.FirstOrDefault(p => p.Id == testSite.SourceId);
                          if (testSource == null)
                          {
                              testSource = BigEntityTableRemotingEngine.Find<TestSource>(nameof(TestSource), testSite.SourceId);
                              sources.Add(testSource);
                          }

                          var tempTestCases = BigEntityTableRemotingEngine.FindBatch<TestCase>(nameof(TestCase), testTaskBag.CaseId.Select(p => (object)p));
                          testCases.AddRange(tempTestCases);
                          var tempTestPages = BigEntityTableRemotingEngine.FindBatch<TestPage>(nameof(TestPage), testCases.Select(p => (object)p.PageId).Distinct());
                          testPages.AddRange(tempTestPages);

                          var key = "scripts" + testSite.SourceId;
                          object globalScripts = null;
                          if (!cach.TryGetValue(key, out globalScripts))
                          {
                              globalScripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == 0).ToList();
                              cach.Add(key, globalScripts);
                          }

                          object siteScripts = null;
                          var key2 = "scripts" + testSite.SourceId + "_" + testSite.Id;
                          if (!cach.TryGetValue(key2, out siteScripts))
                          {
                              siteScripts = BigEntityTableRemotingEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == testSite.Id).ToList();
                              cach.Add(key2, siteScripts);
                          }

                          object env = null;
                          var key3 = "env" + testTaskBag.TestEnvId;
                          if (!cach.TryGetValue(key3, out env))
                          {
                              env = BigEntityTableRemotingEngine.Find<TestEnv>(nameof(TestEnv), testTaskBag.TestEnvId);
                              cach.Add(key3, env);
                          }

                          object envParams = null;
                          if (env != null)
                          {
                              var key4 = "env" + testSite.Id + "_" + (env as TestEnv).Id;
                              if (!cach.TryGetValue(key4, out envParams))
                              {
                                  envParams = env == null ? new List<TestEnvParam>() : BigEntityTableRemotingEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_EnvId", new object[] { testSite.Id, (env as TestEnv).Id }).ToList();
                                  cach.Add(key4, envParams);
                              }
                          }
                          foreach (var tc in tempTestCases)
                          {
                              var testLogin = GetTestLogin(tc, testSite);

                              testTaskList.Add(new TestTask
                              {
                                  TestSource = testSource,
                                  SiteTestScripts = (List<TestScript>)siteScripts,
                                  GlobalTestScripts = (List<TestScript>)globalScripts,
                                  TestCase = tc,
                                  TestLogin = testLogin,
                                  TestPage = tempTestPages.First(x => x.Id == tc.PageId),
                                  TestSite = testSite,
                                  TestEnv = env == null ? null : (TestEnv)env,
                                  Bag = testTaskBag,
                                  TestEnvParams = envParams == null ? null : (List<TestEnvParam>)envParams
                              });
                          }

                          testTaskList = TestTaskBagBiz.Order(testTaskList, testTaskBag);
                      }

                      foreach(var kv in testTaskList.GroupBy(x => x.Bag))
                      {
                          //
                          var testPanel = (TestPanel)Util.TryAddToMainTab(this, $"({testSites.First().Name}.{kv.Key.BagName})执行定时测试", () =>
                          {
                              var panel = new TestPanel($"[{kv.Key.BagName}]执行定时测试");

                              panel.OnTaskStart += t =>
                              {
                                  var rt = t as RunTestTask;
                                  if (rt != null && rt.TestLogin != null && (currentTestLogin == null || currentTestLogin.Id != rt.TestLogin.Id))
                                  {
                                      currentTestLogin = rt.TestLogin;
                                      panel.ClearCookie(rt.TestLogin.Url);

                                      var cookies = TestCookieContainerBiz.GetCookies(rt.TestLogin.SiteId, rt.TestEnv?.Id, rt.TestLogin.Id);
                                      if (cookies?.Count > 0)
                                      {
                                          panel.SetCookie(rt.GetStartPageUrl(), cookies);
                                      }
                                  }
                              };

                              panel.Load();

                              return panel;
                          }, null);

                          var runTaskList = kv.Select(task => new RunTestTask(task.GetTaskName(), false, task.TestSite, task.TestLogin, task.TestPage, task.TestCase, task.TestEnv, task.TestEnvParams, task.GlobalTestScripts, task.SiteTestScripts, task.ResultNotify));
                          BeginInvoke(new Action(() => testPanel.RunTest(runTaskList)));
                      }               

                      TestLogin GetTestLogin(TestCase testCase, TestSite testSite)
                      {
                          TestLogin testLogin = null;
                          if (testCase.OnlyUserId > 0)
                          {
                              testLogin = testLoginList.FirstOrDefault(p => p.Id == testCase.OnlyUserId && p.SiteId == testSite.Id);
                              if (testLogin == null)
                              {
                                  Util.SendMsg(this, $"账号不存在：{testCase.OnlyUserId}");
                                  return testLogin;
                              }
                          }
                          else
                          {
                              if (testLoginList.Where(p => p.SiteId == testSite.Id).Count() == 1)
                              {
                                  testLogin = testLoginList.FirstOrDefault(p => p.SiteId == testSite.Id);
                              }
                              else if (testLoginList.Where(p => p.SiteId == testSite.Id).Count() > 1)
                              {
                                  testLogin = testLoginList.FirstOrDefault(p => p.SiteId == testSite.Id && p.Used);
                                  if (testLogin == null)
                                  {
                                      MessageBox.Show($"{testSite.Name}:请选择一个测试帐号");
                                      return testLogin;
                                  }
                              }

                          }

                          return testLogin;
                      }
                  }
                  return false;
              }, runintime: false);

            AutoTest.Biz.SimulateServer.ApiTaskTrigger.NewTaskRecived += ApiTaskTrigger_NewTaskRecived;
        }

        private void ApiTaskTrigger_NewTaskRecived(TestTask task,APITaskRequest apiTaskRequest)
        {
            //
            var testPanel = (TestPanel)Util.TryAddToMainTab(this, $"执行API请求", () =>
            {
                var panel = new TestPanel("执行API请求");
                panel.Load();

                return panel;
            }, null);
            //if (testPanel.IsRunning())
            //{
            //    Util.SendMsg(this, "正在执行测试，请稍后再试");
            //    return false;
            //}

            //if (!testPanel.Reset())
            //{
            //    Util.SendMsg(this, "任务未开始，有测试在执行");
            //    return false;
            //}

            var runTaskList = new RunTestTask(task.GetTaskName(), false, task.TestSite, task.TestLogin, task.TestPage, task.TestCase, task.TestEnv, task.TestEnvParams, task.GlobalTestScripts, task.SiteTestScripts, task.ResultNotify, apiTaskRequest);
            BeginInvoke(new Action(() => testPanel.RunTest(runTaskList)));
        }
    }
}
