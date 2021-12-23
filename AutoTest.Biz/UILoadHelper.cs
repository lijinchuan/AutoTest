using AutoTest.Domain;
using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.Biz
{
    public static class UILoadHelper
    {
        public static void LoadTestResurceAsync(Form parent, TreeNode tbNode, AsyncCallback callback, object @object)
        {
            tbNode.Nodes.Add(new TreeNode("加载中...", 3, 3));
            tbNode.Expand();

            new Action<Form, TreeNode>(LoadTestResurce).BeginInvoke(parent, tbNode, callback, @object);
        }

        public static void LoadTestResurce(Form parent, TreeNode pnode)
        {
            List<TreeNode> treeNodes = new List<TreeNode>();
            var aPISources = BigEntityTableEngine.LocalEngine.List<TestSource>(nameof(TestSource), 1, int.MaxValue);
            foreach (var s in aPISources)
            {
                TreeNode node = new TreeNodeEx(s.SourceName, 12, 12, 12, 12);
                var serverinfo = s;
                node.Tag = serverinfo;

                treeNodes.Add(node);
            }
            parent.Invoke(new Action(() => { pnode.Nodes.Clear(); pnode.Nodes.AddRange(treeNodes.ToArray()); pnode.Expand(); }));
        }

        public static void LoadTestSiteAsync(Form parent, TreeNode tbNode, int sourceId, AsyncCallback callback, object @object)
        {
            tbNode.Nodes.Add(new TreeNode("加载中...", 3, 3));
            tbNode.Expand();

            new Action<Form, TreeNode,int>(LoadTestSite).BeginInvoke(parent, tbNode, sourceId, callback, @object);
        }
        public static void LoadTestSite(Form parent, TreeNode pnode, int sourceId)
        {
            List<TreeNode> treeNodes = new List<TreeNode>();

            var testSiteList = BigEntityTableEngine.LocalEngine.Find<TestSite>(nameof(TestSite), nameof(TestSite.SourceId), new object[] { sourceId }).ToList();

            foreach (var page in testSiteList)
            {
                var node=new TreeNodeEx
                {
                    Text = page.Name,
                    Tag = page,
                    ImageIndex = 11,
                    SelectedImageIndex = 11,
                    CollapseImgIndex = 11,
                    ExpandImgIndex = 11
                };
                treeNodes.Add(node);
            }

            treeNodes.Add(new TreeNodeEx
            {
                Text = "脚本",
                Tag = new NodeContents(NodeContentType.SCRIPTPARENT),
                ImageIndex = 0,
                SelectedImageIndex = 2,
                CollapseImgIndex = 0,
                ExpandImgIndex = 2
            });

            parent.Invoke(new Action(() => { pnode.Nodes.Clear(); pnode.Nodes.AddRange(treeNodes.ToArray()); pnode.Expand(); }));
        }

        public static void LoadTestPageAsync(Form parent, TreeNode tbNode, int siteId, AsyncCallback callback, object @object)
        {
            tbNode.Nodes.Add(new TreeNode("加载中...", 3, 3));
            tbNode.Expand();

            new Action<Form, TreeNode, int>(LoadTestPage).BeginInvoke(parent, tbNode, siteId, callback, @object);
        }

        public static void LoadTestPage(Form parent, TreeNode pnode, int siteId)
        {
            List<TreeNode> treeNodes = new List<TreeNode>();

            var testLoginList = BigEntityTableEngine.LocalEngine.Find<TestLogin>(nameof(TestLogin), nameof(TestLogin.SiteId), new object[] { siteId });
            foreach(var testLogin in testLoginList)
            {
                treeNodes.Add(new TreeNodeEx
                {
                    Text = testLogin.AccountInfo,
                    Tag = testLogin,
                    ImageIndex = 15,
                    SelectedImageIndex = 15,
                    CollapseImgIndex = 15,
                    ExpandImgIndex = 15
                });
            }

            var testPageList = BigEntityTableEngine.LocalEngine.Find<TestPage>(nameof(TestPage), nameof(TestPage.SiteId), new object[] { siteId })
                .OrderBy(p=>p.Order).ToList();

            foreach (var page in testPageList)
            {
                treeNodes.Add(new TreeNodeEx
                {
                    Text = page.Name,
                    Tag = page,
                    ImageIndex = 10,
                    SelectedImageIndex = 10,
                    CollapseImgIndex = 10,
                    ExpandImgIndex = 10
                });
            }

            treeNodes.Add(new TreeNodeEx
            {
                Text = "脚本",
                Tag = new NodeContents(NodeContentType.SCRIPTPARENT),
                ImageIndex = 0,
                SelectedImageIndex = 2,
                CollapseImgIndex = 0,
                ExpandImgIndex = 2
            });

            treeNodes.Add(new TreeNodeEx
            {
                Text = "环境",
                Tag = new NodeContents(NodeContentType.ENVPARENT),
                ImageIndex = 13,
                SelectedImageIndex = 13,
                CollapseImgIndex = 13,
                ExpandImgIndex = 13
            });

            parent.Invoke(new Action(() => { pnode.Nodes.Clear(); pnode.Nodes.AddRange(treeNodes.ToArray()); pnode.Expand(); }));
        }

        public static void LoadTestCaseAsync(Form parent, TreeNode tbNode, int pageId, int envId, AsyncCallback callback, object @object)
        {
            tbNode.Nodes.Add(new TreeNode("加载中...", 3, 3));
            tbNode.Expand();

            new Action<Form, TreeNode, int, int>(LoadTestCase).BeginInvoke(parent, tbNode, pageId, envId, callback, @object);
        }

        public static void LoadTestCase(Form parent, TreeNode pnode, int pageId,int envId)
        {
            List<TreeNode> treeNodes = new List<TreeNode>();
            var testCaseList = BigEntityTableEngine.LocalEngine.Find<TestCase>(nameof(TestCase), nameof(TestCase.PageId), new object[] { pageId })
                .OrderBy(p => p.Order).ToList();

            foreach (var @case in testCaseList)
            {
                var imageIndex = 18;

                //if (envId != 0)
                {
                    var testResult = BigEntityTableEngine.LocalEngine.Scan<TestResult>(nameof(TestResult), TestResult.Index_TestCaseId_EnvId_TestDate,
                        new object[] { @case.Id, envId, DateTime.Now }, new object[] { @case.Id, envId, DateTime.MinValue }, 1, 1).FirstOrDefault();

                    if (testResult != null)
                    {
                        if (testResult.Success)
                        {
                            if (testResult.HasWarn)
                            {
                                imageIndex = 21;
                            }
                            else
                            {
                                imageIndex = 19;
                            }
                        }
                        else
                        {
                            imageIndex = 20;
                        }
                       
                    }
                }
                treeNodes.Add(new TreeNodeEx
                {
                    Text = @case.CaseName,
                    Tag = @case,
                    ImageIndex = imageIndex,
                    SelectedImageIndex = imageIndex,
                    CollapseImgIndex = imageIndex,
                    ExpandImgIndex = imageIndex
                });
            }

            parent.Invoke(new Action(() => { pnode.Nodes.Clear(); pnode.Nodes.AddRange(treeNodes.ToArray()); pnode.Expand(); }));
        }

        public static void LoadTestEnvAsync(Form parent, TreeNode pnode, int siteId, AsyncCallback callback, object @object)
        {
            pnode.Nodes.Add(new TreeNode("加载中...", 3, 3));
            pnode.Expand();

            new Action<Form, TreeNode, int>(LoadTestEnv).BeginInvoke(parent, pnode, siteId, callback, @object);
        }

        public static void LoadTestEnv(Form parent, TreeNode pnode, int siteId)
        {
            List<TreeNode> treeNodes = new List<TreeNode>();
            var envlist = BigEntityTableEngine.LocalEngine.Find<TestEnv>(nameof(TestEnv), nameof(TestEnv.SiteId), new object[] { siteId }).ToList();
            foreach (var s in envlist)
            {
                int imageIndex = 0, selectedImageIndex = 2;
                if (s.Used)
                {
                    imageIndex = 16;
                    selectedImageIndex = 16;
                }
                TreeNode node = new TreeNodeEx(s.EnvName, imageIndex, selectedImageIndex, imageIndex, selectedImageIndex);
                node.Tag = s;

                treeNodes.Add(node);
            }
            parent.Invoke(new Action(() => { pnode.Nodes.Clear(); pnode.Nodes.AddRange(treeNodes.ToArray()); pnode.Expand(); }));
        }

        public static void LoadTestEnvParamsAsync(Form parent, TreeNode pnode, int sourceid, int envid, AsyncCallback callback, object @object)
        {
            pnode.Nodes.Add(new TreeNode("加载中...", 3, 3));
            pnode.Expand();

            new Action<Form, TreeNode, int, int>(LoadTestEnvParams).BeginInvoke(parent, pnode, sourceid, envid, callback, @object);
        }

        public static void LoadTestEnvParams(Form parent, TreeNode pnode, int siteId, int envid)
        {
            try
            {
                List<TreeNode> treeNodes = new List<TreeNode>();
                var allenvparamslist = BigEntityTableEngine.LocalEngine.Find<TestEnvParam>(nameof(TestEnvParam), "SiteId", new object[] { siteId }).ToList();
                var allparamnames = allenvparamslist.Select(p => p.Name).Distinct();
                foreach (var s in allparamnames)
                {
                    var param = allenvparamslist.Find(p => p.EnvId == envid && p.Name == s);

                    TreeNode node = new TreeNode(s);
                    if (param == null)
                    {
                        node.Tag = new TestEnvParam
                        {
                            EnvId = envid,
                            SiteId = siteId,
                            Name = s
                        };
                        node.ImageKey = node.SelectedImageKey = "COLQ";

                    }
                    else
                    {
                        node.Tag = param;
                        node.ImageKey = node.SelectedImageKey = "COL";
                    }
                    treeNodes.Add(node);

                }
                parent.Invoke(new Action(() => { pnode.Nodes.Clear(); pnode.Nodes.AddRange(treeNodes.ToArray()); pnode.Expand(); }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void LoadTestScriptAsync(Form parent, TreeNode pnode, int sourceid, int siteId, AsyncCallback callback, object @object)
        {
            pnode.Nodes.Add(new TreeNode("加载中...", 3, 3));
            pnode.Expand();

            new Action<Form, TreeNode, int, int>(LoadTestScrip).BeginInvoke(parent, pnode, sourceid, siteId, callback, @object);
        }

        public static void LoadTestScrip(Form parent, TreeNode pnode, int sourceid, int siteId)
        {
            try
            {
                List<TreeNode> treeNodes = new List<TreeNode>();
                var testScriptList = BigEntityTableEngine.LocalEngine.Scan<TestScript>(nameof(TestScript), $"{nameof(TestScript.SourceId)}_{nameof(TestScript.SiteId)}_{nameof(TestScript.ScriptName)}",
                    new object[] { sourceid, siteId, Consts.STRINGCOMPAIRMIN }, new object[] { sourceid, siteId, Consts.STRINGCOMPAIRMAX }, 1, int.MaxValue)
                    .OrderBy(p => p.Order).ToList();

                foreach (var s in testScriptList)
                {
                    TreeNode node = new TreeNodeEx(s.ScriptName, 17, 17, 17, 17);
                    node.Tag = s;

                    treeNodes.Add(node);

                }
                parent.Invoke(new Action(() => { pnode.Nodes.Clear(); pnode.Nodes.AddRange(treeNodes.ToArray()); pnode.Expand(); }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
