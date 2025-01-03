﻿using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Comm;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.SubForm
{
    public partial class ServerSettingDlg : SubBaseDlg
    {
        public ServerSettingDlg()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var serverCfg = SerializerHelper.DeSerializerFile<RemotingServiceConfig>(RemotingConsts.RemotingServiceConfigFileName);
            var localCfg= SerializerHelper.DeSerializerFile<RemotingClientConfig>(RemotingConsts.RemotingClientConfigFileName)?.RemotingClientConfigItems?.FirstOrDefault();
            if (serverCfg != null)
            {
                TBLocalPort.Text = serverCfg.Port.ToString();
                TBLocalPassword.Text = serverCfg.Password;
                TBLocalUserName.Text = serverCfg.UserName;
            }
            if (localCfg != null)
            {
                TBRemotingIP.Text = localCfg.Ip;
                TBRemotingPort.Text = localCfg.Port.ToString();
                TBRemotingUserName.Text = localCfg.UserName;
                TBRemotingPassword.Text = localCfg.Password;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            bool isOk = false;
            if (!string.IsNullOrWhiteSpace(TBLocalPort.Text))
            {
                int port = 0;
                if(!int.TryParse(TBLocalPort.Text,out port))
                {
                    MessageBox.Show("端口必须是数字");
                    return;
                }

                if (port <= 1024 || port >= 65535)
                {
                    MessageBox.Show("端口范围必须[1024~65535]之间");
                    return;
                }

                var cfg = new RemotingServiceConfig
                {
                   Password=TBLocalPassword.Text,
                   Port=port,
                   UserName=TBLocalUserName.Text.Trim()
                };
                SerializerHelper.SerializerToXML(cfg, RemotingConsts.RemotingServiceConfigFileName);
                isOk = true;
            }
            else
            {
                if (File.Exists(RemotingConsts.RemotingServiceConfigFileName))
                {
                    File.Delete(RemotingConsts.RemotingServiceConfigFileName);
                }
            }

            if (!string.IsNullOrWhiteSpace(TBRemotingIP.Text) && !string.IsNullOrWhiteSpace(TBRemotingPort.Text))
            {
                if (!Regex.IsMatch(TBRemotingIP.Text, @"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$"))
                {
                    MessageBox.Show("IP格式错误");
                    return;
                }

                int port = 0;
                if (!int.TryParse(TBRemotingPort.Text, out port))
                {
                    MessageBox.Show("端口必须是数字");
                    return;
                }

                if (port <= 1024 || port >= 65535)
                {
                    MessageBox.Show("端口范围必须[1024~65535]之间");
                    return;
                }

                var cfg = new RemotingClientConfig
                {
                    RemotingClientConfigItems=new List<RemotingClientConfigItem>
                    {
                        new RemotingClientConfigItem
                        {
                            Ip=TBRemotingIP.Text.Trim(),
                            Password=TBRemotingPassword.Text.Trim(),
                            Port=port,
                            Used=true,
                            UserName=TBRemotingUserName.Text.Trim(),
                            RemotingTables=new List<string>
                            {
                                nameof(TestSource),
                                nameof(TestSite),
                                nameof(TestLogin),
                                nameof(TestPage),
                                nameof(TestCase),
                                nameof(TestEnv),
                                nameof(TestEnvParam),
                                nameof(TestCaseData),
                                nameof(TestCaseSetting),
                                nameof(TestCaseParam),
                                nameof(TestScript),
                                nameof(FileDB),
                                nameof(Counter)
                            }
                        }
                    }
                };

                SerializerHelper.SerializerToXML(cfg, RemotingConsts.RemotingClientConfigFileName);
                isOk = true;
            }
            else if (File.Exists(RemotingConsts.RemotingClientConfigFileName))
            {
                File.Delete(RemotingConsts.RemotingClientConfigFileName);
            }

            if (isOk)
            {
                MessageBox.Show("保存成功，重启程序生效。");
            }

            DialogResult = DialogResult.OK;
        }
    }
}
