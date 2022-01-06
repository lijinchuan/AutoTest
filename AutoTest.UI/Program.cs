using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI
{
    static class Program
    {
        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Process[] pa = Process.GetProcesses();//获取当前进程数组。
            var currprocess = Process.GetCurrentProcess();
            foreach (Process p in pa)
            {
                if (p.ProcessName == currprocess.ProcessName && p.Id != currprocess.Id && p.MainModule.FileName == currprocess.MainModule.FileName)
                {
                    //MessageBox.Show("另一个进程正在运行，无法启动。");
                    SwitchToThisWindow(p.MainWindowHandle, true);
                    return;
                }
            }

            BigEntityTableEngine.LocalEngine.CreateTable<TestSource>(p => p.Id, b => b.AddIndex(nameof(TestSource.Id), c => c.Asc(m => m.Id)));

            BigEntityTableEngine.LocalEngine.CreateTable<TestSite>(p => p.Id, b => b.AddIndex(nameof(TestSite.Name), c => c.Asc(m => m.Name))
            .AddIndex(nameof(TestSite.SourceId), c => c.Asc(m => m.SourceId)));

            //BigEntityTableEngine.LocalEngine.CreateTable<TestLogin>(p => p.Id, b => b.AddIndex(nameof(TestLogin.SiteId), c => c.Asc(m => m.SiteId)));
            BigEntityTableEngine.LocalEngine.Upgrade<Domain.Entity.OldVerion.TestLogin, TestLogin>(nameof(TestLogin), f => new TestLogin
            {
                AccountInfo = "默认帐号",
                IsMannual = f.IsMannual,
                LoginCode = f.LoginCode,
                SiteId = f.SiteId,
                Url = f.Url,
                Used = false,
                ValidCode = f.ValidCode
            },nameof(TestLogin.Id),true,
            new IndexBuilder<TestLogin>().AddIndex(nameof(TestLogin.SiteId), c => c.Asc(m => m.SiteId)).Build());

            BigEntityTableEngine.LocalEngine.CreateTable<TestPage>(p => p.Id, b => b.AddIndex(nameof(TestPage.SiteId), c => c.Asc(m => m.SiteId)));

            //BigEntityTableEngine.LocalEngine.CreateTable<TestCase>(p => p.Id, b => b.AddIndex(nameof(TestCase.PageId), c => c.Asc(m => m.PageId)));
            BigEntityTableEngine.LocalEngine.Upgrade<Domain.Entity.OldVerion.TestCaseV2, TestCase>(nameof(TestCase),
                f => new TestCase
                {
                    BodyDataType=f.BodyDataType,
                    AuthType=f.AuthType,
                    ApiEnvId=f.ApiEnvId,
                    ApplicationType=f.ApplicationType,
                    CaseName=f.CaseName,
                    Desc=f.Desc,
                    Order=f.Order,
                    PageId=f.PageId,
                    TestCode=f.TestCode,
                    Url=f.Url,
                    ValidCode=f.ValidCode,
                    WebMethod=f.WebMethod,
                    OnlyUserId=0
                }, nameof(TestCase.Id), true, new IndexBuilder<TestCase>().AddIndex(nameof(TestCase.PageId), c => c.Asc(m => m.PageId)).Build());

            BigEntityTableEngine.LocalEngine.CreateTable<TestEnv>(p => p.Id, b => b.AddIndex(nameof(TestEnv.SiteId), p => p.Asc(q => q.SiteId)));
            BigEntityTableEngine.LocalEngine.CreateTable<TestEnvParam>(p => p.Id, p => p.AddIndex(nameof(TestEnvParam.SiteId), q => q.Asc(m => m.SiteId))
            .AddIndex("SiteId_EnvId", q => q.Asc(m => m.SiteId).Asc(m => m.EnvId))
            .AddIndex("SiteId_Name", q => q.Asc(m => m.SiteId).Asc(m => m.Name)));

            BigEntityTableEngine.LocalEngine.CreateTable<TestCaseData>(p => p.Id, p => p.AddIndex(nameof(TestCaseData.TestCaseId), q => q.Asc(m => m.TestCaseId)));

            BigEntityTableEngine.LocalEngine.CreateTable<TestCaseInvokeLog>(p => p.Id, p => p.AddIndex("TestCaseId_CDate", m => m.Asc(s => s.TestCaseId).Desc(s => s.CDate))
            .AddIndex("TestCaseId_EnvId_CDate", m => m.Asc(s => s.TestCaseId).Asc(s => s.EnvId).Desc(s => s.CDate)));

            BigEntityTableEngine.LocalEngine.CreateTable<TestCaseSetting>(p => p.Id, b => b.AddIndex(nameof(TestCaseSetting.TestCaseId), c => c.Asc(d => d.TestCaseId)));

            //参数
            BigEntityTableEngine.LocalEngine.CreateTable<TestCaseParam>(p => p.Id, p => p.AddIndex(nameof(TestCaseParam.TestCaseId), m => m.Asc(s => s.TestCaseId)));
            //文档
            BigEntityTableEngine.LocalEngine.CreateTable<APIDoc>(p => p.Id, p => p.AddIndex(nameof(APIDoc.TestSourceId), m => m.Asc(s => s.TestSourceId)).AddIndex(nameof(APIDoc.TestCaseId), m => m.Asc(s => s.TestCaseId)));
            //文档示例
            BigEntityTableEngine.LocalEngine.CreateTable<TestCaseDocExample>(p => p.Id, p => p.AddIndex(nameof(TestCaseDocExample.TestCaseId), m => m.Asc(s => s.TestCaseId)));
            BigEntityTableEngine.LocalEngine.CreateTable<ProxyServer>(p => p.Id, null);


            BigEntityTableEngine.LocalEngine.CreateTable<TestScript>(p => p.Id, a => a.AddIndex(TestScript.Index3, b => b.Asc(m => m.SourceId).Asc(m => m.SiteId).Asc(m => m.ScriptName)));

            BigEntityTableEngine.LocalEngine.CreateTable<TestResult>(p => p.Id, a => a.AddIndex(TestResult.Index_TestCaseId_EnvId_TestDate,
                   b => b.Asc(m => m.TestCaseId).Asc(m => m.EnvId).Desc(m => m.TestStartDate)));

            BigEntityTableEngine.LocalEngine.CreateTable<FileDB>(p => p.Id, a => a.AddIndex(nameof(FileDB.FileName), m => m.Asc(s => s.FileName)));

            AutofacBuilder.init();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainFrm());
        }
    }
}
