using AutoTest.Biz;
using AutoTest.Domain;
using AutoTest.Domain.Entity;
using AutoTest.UI.SubForm;
using AutoTest.UI.WebTask;
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
                var envData= GetCurrEnvData(selNode);
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

        }

        private (TestEnv env, List<TestEnvParam> envParams, bool hasEvn) GetCurrEnvData(TreeNode node)
        {
            var testSite = FindParentNode<TestSite>(node);
            if (testSite == null)
            {
                return (null, null, false);
            }

            var testEnvs = BigEntityTableEngine.LocalEngine.Find<TestEnv>(nameof(TestEnv), nameof(TestEnv.SiteId), new object[] { testSite.Id });
            var currentEnv = testEnvs.FirstOrDefault(p => p.Used);
            List<TestEnvParam> testEnvParams = null;
            if (currentEnv != null)
            {
                testEnvParams = BigEntityTableEngine.LocalEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_EnvId", new object[] { testSite.Id, currentEnv.Id }).ToList();
            }
            return (currentEnv, testEnvParams, true);
        }

        private List<TreeNode> GetTestCaseTaskList(TreeNode node)
        {
            var list = new List<TreeNode>();

            if(node.Tag is TestCase)
            {
                list.Add(node);
            }

            if (node.Nodes.Count > 0)
            {
                foreach(TreeNode n in node.Nodes)
                {
                    list.AddRange(GetTestCaseTaskList(n));
                }
            }

            return list;
        }

        private TreeNode FindNode(TreeNodeCollection topNodes,object tag)
        {
            foreach(TreeNode node in topNodes)
            {
                if (node.Tag != null)
                {
                    if (node.Tag.GetType() == tag.GetType()
                        && (node.Tag as IComparable)!=null)
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

        private void FindParentAndReLoad(TreeNode treeNode)
        {
            if (treeNode.Parent != null)
            {
                ReLoadDBObj(treeNode.Parent,false);
            }
            else
            {
                if (treeNode.Tag == null)
                {
                    return;
                }

                var node = FindNode(tv_DBServers.Nodes, treeNode.Tag);
                if (node != null)
                {
                    ReLoadDBObj(node.Parent,false);
                }
            }
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

                                var tab= (UCAddCaseParam)Util.TryAddToMainTab(this, $"[{testSite.Name}]-{testPage.Name}-{testCase.CaseName}",()=> new UCAddCaseParam(testSite, testPage, testCase,
                                    ()=> FindParentAndReLoad(selnode)));

                                if (tab.CallBack == null)
                                {
                                    tab.CallBack = () => FindParentAndReLoad(selnode);
                                }
                            }
                            else if (selnode.Tag is TestPage)
                            {
                                var testSite = FindParentNode<TestSite>(selnode);
                                var dlg = new AddTestPageDlg(testSite.Id, (selnode.Tag as TestPage).Id);
                                if (dlg.ShowDialog() == DialogResult.OK)
                                {
                                    FindParentAndReLoad(selnode);
                                }
                            }
                            else if (selnode.Tag is TestSite)
                            {
                                var testSource = FindParentNode<TestSource>(selnode);
                                AddTestSiteDlg dlg = new AddTestSiteDlg(testSource.Id, (selnode.Tag as TestSite).Id);
                                if (dlg.ShowDialog() == DialogResult.OK)
                                {
                                    FindParentAndReLoad(selnode);
                                }
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
                                if (dlg.ShowDialog() == DialogResult.OK)
                                {
                                    FindParentAndReLoad(selnode);
                                }
                            }
                            else if (selnode.Tag is TestScript)
                            {
                                var testScript = selnode.Tag as TestScript;
                                var testResource = FindParentNode<TestSource>(selnode);
                                var testSite = FindParentNode<TestSite>(selnode);
                                var scripts = BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testResource.Id && s.SiteId == 0).ToList();

                                Util.AddToMainTab(this, $"{testResource.SourceName}_{testSite?.Name}_{testScript.ScriptName}(脚本)",
                                        new UC.UCTestScript(testScript,()=> FindParentAndReLoad(selnode), scripts));
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
                                    BigEntityTableEngine.LocalEngine.Delete<TestCase>(nameof(TestCase), (selnode.Tag as TestCase).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestPage)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableEngine.LocalEngine.Delete<TestPage>(nameof(TestPage), (selnode.Tag as TestPage).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestSite)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableEngine.LocalEngine.Delete<TestSite>(nameof(TestSite), (selnode.Tag as TestSite).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestSource)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableEngine.LocalEngine.Delete<TestSource>(nameof(TestSource), (selnode.Tag as TestSource).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestEnv)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableEngine.LocalEngine.Delete<TestEnv>(nameof(TestEnv), (selnode.Tag as TestEnv).Id);
                                    return true;
                                };
                            }
                            else if (selnode.Tag is TestScript)
                            {
                                delFunc = () =>
                                {
                                    BigEntityTableEngine.LocalEngine.Delete<TestScript>(nameof(TestScript), (selnode.Tag as TestScript).Id);
                                    return true;
                                };
                            }

                            if (delFunc!=null)
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
                            var testSite =FindParentNode<TestSite>(selnode);
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
                            AddTestSiteDlg dlg = new AddTestSiteDlg(testSource.Id,0);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                ReLoadDBObj(selnode);
                            }
                            break;
                        }
                    case "添加测试页面":
                        {
                            var testSite = FindParentNode<TestSite>(selnode);
                            var dlg = new AddTestPageDlg(testSite.Id,0);
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
                            var scripts = BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == 0).ToList();
                            var siteScripts = BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == testSite.Id).ToList();
                            scripts.AddRange(siteScripts);
                            var step1dlg = new AddTestCaseDlg(testPage.Id, 0, scripts);
                            
                            if (step1dlg.ShowDialog() == DialogResult.OK)
                            {
                                ReLoadDBObj(selnode);
                                Util.AddToMainTab(this, $"[{testSite.Name}]-{testPage.Name}-{step1dlg.TestCase.CaseName}", new UCAddCaseParam(testSite, testPage, step1dlg.TestCase,
                                    () => FindParentAndReLoad(selnode)));
                            }
                            break;
                        }
                    case "运行测试":
                        {
                            var testCaseNodeList = GetTestCaseTaskList(selnode);
                            var testTaskList = new List<TestTask>();
                            var scriptsDic = new Dictionary<object, List<TestScript>>();
                            var loginDic = new Dictionary<object, TestLogin>();
                            var envDic = new Dictionary<object, (TestEnv env, List<TestEnvParam> envParams, bool hasEvn)>();

                            List<TestSource> sources = new List<TestSource>();
                            List<TestSite> testSites = new List<TestSite>();
                            List<TestPage> testPages = new List<TestPage>();
                            List<TestCase> testCases = new List<TestCase>();

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

                                TestLogin testLogin;
                                var key = "testLogin_" + testSource.Id + "_" + testSite.Id;
                                if (!loginDic.TryGetValue(key, out testLogin))
                                {
                                    var testLoginList = BigEntityTableEngine.LocalEngine.Find<TestLogin>(nameof(TestLogin), nameof(TestLogin.SiteId), new object[] { testSite.Id });
                                    if (testLoginList.Any())
                                    {
                                        if(!testLoginList.Any(p => p.Used))
                                        {
                                            MessageBox.Show("请选择一个测试帐号");
                                            return;
                                        }
                                        testLogin = testLoginList.First(p => p.Used);
                                    }
                                    
                                    loginDic.Add(key, testLogin);
                                }

                                key = "globalScripts_" + testSource.Id;
                                List<TestScript> globalScripts = null;
                                if (!scriptsDic.TryGetValue(key, out globalScripts))
                                {
                                    globalScripts = BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == 0).ToList();
                                    scriptsDic.Add(key, globalScripts);
                                }

                                key = "siteScripts_" + testSource.Id + "_" + testSite.Id;
                                List<TestScript> siteScripts = null;
                                if (!scriptsDic.TryGetValue(key, out siteScripts))
                                {
                                    siteScripts = BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == testSite.Id).ToList();
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
                                    TestEnvParams = ep.envParams,
                                    ResultNotify = r =>
                                    {
                                        BeginInvoke(new Action(() =>
                                        {
                                            var lastNode = node;
                                            if (lastNode.Parent == null)
                                            {
                                                lastNode = FindNode(tv_DBServers.Nodes, lastNode.Tag);
                                            }
                                            if (lastNode == null)
                                            {
                                                return;
                                            }
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
                                });
                            }

                            if (new SubForm.TestCaseSelectorDlg().Init(sources, testSites, testPages, testCases).ShowDialog() == DialogResult.Cancel)
                            {
                                return;
                            }

                            if (new ConfirmDlg("询问", "执行测试吗？").ShowDialog() == DialogResult.OK)
                            {
                                foreach(var n in testCaseNodeList)
                                {
                                    var nodeEx = n as TreeNodeEx;
                                    nodeEx.SelectedImageIndex = nodeEx.ImageIndex = nodeEx.ExpandImgIndex = nodeEx.CollapseImgIndex = 18;
                                }

                                var testPanel = (TestPanel)Util.TryAddToMainTab(this, $"执行测试", () =>
                                {
                                    var panel = new UC.TestPanel("执行测试");
                                    panel.Load();

                                    return panel;
                                }, typeof(TestPanel));

                                testPanel.OnTaskStart += t =>
                                {
                                    var rt = t as RunTestTask;
                                    if (rt != null && rt.TestLogin != null && (currentTestLogin == null || currentTestLogin.Id != rt.TestLogin.Id))
                                    {
                                        currentTestLogin = rt.TestLogin;
                                        testPanel.ClearCookie(rt.TestLogin.Url);
                                    }
                                };

                                LJC.FrameWorkV3.Comm.TaskHelper.SetInterval(1000, () =>
                                {
                                    var runTaskList = testTaskList.Select(task => new RunTestTask(task.GetTaskName(), false, task.TestSite, task.TestLogin, task.TestPage, task.TestCase, task.TestEnv, task.TestEnvParams, task.GlobalTestScripts, task.SiteTestScripts, task.ResultNotify));
                                    this.BeginInvoke(new Action(() => testPanel.RunTest(runTaskList)));
                                    return true;
                                }, runintime: false);
                            }

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
                    case "登陆":
                        {
                            var testSite = FindParentNode<TestSite>(selnode);
                            var testLogin = FindParentNode<TestLogin>(selnode);
                            var ep = GetCurrEnvData(selnode);

                            var testPanel = (TestPanel)Util.TryAddToMainTab(this, $"执行测试", () =>
                            {
                                var panel = new UC.TestPanel(testSite.Name);
                                panel.Load();

                                return panel;
                            }, typeof(TestPanel));

                            LJC.FrameWorkV3.Comm.TaskHelper.SetInterval(1000, () =>
                            {
                                this.BeginInvoke(new Action(() => testPanel.RunTest(new RunTestLoginTask(testLogin.Url, false, testSite, testLogin, ep.env, ep.envParams))));
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
                                var loginTask = new RunTestLoginTask(testLogin.Url, false, testSite, testLogin, ep.env, ep.envParams);
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

                                    var envList = BigEntityTableEngine.LocalEngine.Find<TestEnv>(nameof(TestEnv), nameof(TestEnv.SiteId), new object[] { testSite.Id });
                                    foreach (var envUse in envList.Where(p => p.Used))
                                    {
                                        envUse.Used = false;
                                        BigEntityTableEngine.LocalEngine.Update(nameof(TestEnv), envUse);
                                    }

                                    currEnv.Used = true;
                                    BigEntityTableEngine.LocalEngine.Update(nameof(TestEnv), currEnv);

                                    ReLoadDBObj(selnode.Parent);
                                }
                            }
                            else if (selnode.Tag is TestLogin)
                            {
                                var currTestLogin = selnode.Tag as TestLogin;
                                if (!currTestLogin.Used)
                                {
                                    var testSite = FindParentNode<TestSite>(selnode);

                                    var testLoginList = BigEntityTableEngine.LocalEngine.Find<TestLogin>(nameof(TestLogin), nameof(TestLogin.SiteId), new object[] { testSite.Id });
                                    foreach (var testLoginUse in testLoginList.Where(p => p.Used))
                                    {
                                        testLoginUse.Used = false;
                                        BigEntityTableEngine.LocalEngine.Update(nameof(TestLogin), testLoginUse);
                                    }

                                    currTestLogin.Used = true;
                                    BigEntityTableEngine.LocalEngine.Update(nameof(TestLogin), currTestLogin);

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
                                if(BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), 
                                    $"{nameof(TestScript.SourceId)}_{nameof(TestScript.SiteId)}_{nameof(TestScript.ScriptName)}", 
                                    new object[] { testResource.Id, testSite == null ? 0 : testSite.Id, scriptNameDlg.InputString }).Count() > 0)
                                {
                                    Util.SendMsg(this, $"{scriptNameDlg.InputString}已经存在，不能重复添加。");
                                }
                                else
                                {
                                    var testScript = new TestScript
                                    {
                                        ScriptName=scriptNameDlg.InputString,
                                        Enable=true,
                                        SiteId = testSite == null ? 0 : testSite.Id,
                                        SourceId=testResource.Id
                                    };

                                    BigEntityTableEngine.LocalEngine.Insert(nameof(TestScript), testScript);
                                    var scripts = BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testResource.Id && s.SiteId == 0).ToList();

                                    Util.AddToMainTab(this, $"{testResource.SourceName}_{testSite?.Name}_{testScript.ScriptName}(脚本)",
                                        new UC.UCTestScript(testScript, () => FindParentAndReLoad(selnode), scripts));
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
                                var scripts = BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == 0).ToList();
                                var siteScripts = BigEntityTableEngine.LocalEngine.Find<TestScript>(nameof(TestScript), s => s.Enable && s.SourceId == testSource.Id && s.SiteId == testSite.Id).ToList();
                                scripts.AddRange(siteScripts);

                                var step1dlg = new AddTestCaseDlg(testPage.Id, 0, scripts, selnode.Tag as TestCase);
                                if (step1dlg.ShowDialog() == DialogResult.OK)
                                {
                                    ReLoadDBObj(selnode);
                                    Util.AddToMainTab(this, $"[{testSite.Name}]-{testPage.Name}-{step1dlg.TestCase.CaseName}", new UC.UCAddCaseParam(testSite, testPage, step1dlg.TestCase,
                                        () => FindParentAndReLoad(selnode)));
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
                                    FindParentAndReLoad(selnode);
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
            if (e.Node.Tag is TestEnvParam)
            {
                var testSite = FindParentNode<TestSite>(e.Node);
                var envparam = e.Node.Tag as TestEnvParam;
                if (envparam.Id == 0)
                {
                    envparam = BigEntityTableEngine.LocalEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId_Name", new object[] { testSite.Id, envparam.Name }).FirstOrDefault();
                }
                var dlg = new SubForm.AddTestEnvParamDlg(testSite.Id, envparam.Id);
                if (dlg.ShowDialog() == DialogResult.OK)
                {

                }
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
            if (this.tv_DBServers.SelectedNode == null || this.tv_DBServers.SelectedNode.Level != 1)
                return null;
            var server = this.tv_DBServers.SelectedNode.Text;
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

                添加登陆页ToolStripMenuItem.Visible = (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.LOGINACCOUNTS; ;

                添加测试用例ToolStripMenuItem.Visible = node.Tag is TestPage;

                运行测试ToolStripMenuItem.Visible = node.Tag is TestCase
                    ||node.Tag is TestPage
                    ||node.Tag is TestSite
                    ||node.Tag is TestSource;

                登陆ToolStripMenuItem.Visible = 退出ToolStripMenuItem.Visible = node.Tag is TestLogin;

                添加脚本ToolStripMenuItem.Visible= (node.Tag as INodeContents)?.GetNodeContentType() == NodeContentType.SCRIPTPARENT;

                复制ToolStripMenuItem.Visible = node.Tag is TestCase || node.Tag is TestLogin;
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
                if((nodeStart.Tag as ISearch)!=null)
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

        private void MarkResource()
        {

        }

        private void ClearMarkResource()
        {

        }

        private void TTSM_CreateIndex_Click(object sender, EventArgs e)
        {
            var _node = tv_DBServers.SelectedNode;
        }

        private void TTSM_DelIndex_Click(object sender, EventArgs e)
        {
        }
    }
}
