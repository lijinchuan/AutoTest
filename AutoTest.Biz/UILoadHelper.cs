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
                TreeNode node = new TreeNodeEx(s.SourceName, 0, 2, 0, 2);
                var serverinfo = s;
                node.Tag = serverinfo;

                node.Nodes.Add(new TreeNodeEx
                {
                    Text = "登录",
                    Tag = new NodeContents(NodeContentType.LOGINPAGE),
                    ImageIndex = 0,
                    SelectedImageIndex = 2,
                    CollapseImgIndex = 0,
                    ExpandImgIndex = 2
                });

                node.Nodes.Add(new TreeNodeEx
                {
                    Text = "环境",
                    Tag = new NodeContents(NodeContentType.ENVPARENT),
                    ImageIndex = 0,
                    SelectedImageIndex = 2,
                    CollapseImgIndex = 0,
                    ExpandImgIndex = 2
                });

                treeNodes.Add(node);
            }
            parent.Invoke(new Action(() => { pnode.Nodes.Clear(); pnode.Nodes.AddRange(treeNodes.ToArray()); pnode.Expand(); }));
        }

        public static void LoadTestEnvAsync(Form parent, TreeNode pnode, int sourceid, AsyncCallback callback, object @object)
        {
            pnode.Nodes.Add(new TreeNode("加载中...", 3, 3));
            pnode.Expand();

            new Action<Form, TreeNode, int>(LoadTestEnv).BeginInvoke(parent, pnode, sourceid, callback, @object);
        }

        public static void LoadTestEnv(Form parent, TreeNode pnode, int sourceid)
        {
            List<TreeNode> treeNodes = new List<TreeNode>();
            var envlist = BigEntityTableEngine.LocalEngine.Find<TestEnv>(nameof(TestEnv), "SourceId", new object[] { sourceid }).ToList();
            foreach (var s in envlist)
            {
                TreeNode node = new TreeNodeEx(s.EnvName, 0, 2, 0, 2);
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

        public static void LoadTestEnvParams(Form parent, TreeNode pnode, int sourceid, int envid)
        {
            try
            {
                List<TreeNode> treeNodes = new List<TreeNode>();
                var allenvparamslist = BigEntityTableEngine.LocalEngine.Find<TestEnvParam>(nameof(TestEnvParam), "APISourceId", new object[] { sourceid }).ToList();
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
                            APISourceId = sourceid,
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
    }
}
