using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            BigEntityTableEngine.LocalEngine.CreateTable<TestSource>(p => p.Id, b => b.AddIndex(nameof(TestSource.Id), c => c.Asc(m => m.Id)));

            BigEntityTableEngine.LocalEngine.CreateTable<TestSite>(p => p.Id, b => b.AddIndex(nameof(TestSite.Name), c => c.Asc(m => m.Name))
            .AddIndex(nameof(TestSite.SourceId), c => c.Asc(m => m.SourceId)));

            BigEntityTableEngine.LocalEngine.CreateTable<TestLogin>(p => p.Id, b => b.AddIndex(nameof(TestLogin.SiteId), c => c.Asc(m => m.SiteId)));

            BigEntityTableEngine.LocalEngine.CreateTable<TestPage>(p => p.Id, b => b.AddIndex(nameof(TestPage.SiteId), c => c.Asc(m => m.SiteId)));

            BigEntityTableEngine.LocalEngine.CreateTable<TestCase>(p => p.Id, b => b.AddIndex(nameof(TestCase.PageId), c => c.Asc(m => m.PageId)));
            
            BigEntityTableEngine.LocalEngine.CreateTable<TestEnv>(p => p.Id, b => b.AddIndex("SourceId", p => p.Asc(q => q.SourceId)));
            BigEntityTableEngine.LocalEngine.CreateTable<TestEnvParam>(p => p.Id, p => p.AddIndex("APISourceId", q => q.Asc(m => m.APISourceId))
            .AddIndex("APISourceId_EnvId", q => q.Asc(m => m.APISourceId).Asc(m => m.EnvId))
            .AddIndex("APISourceId_Name", q => q.Asc(m => m.APISourceId).Asc(m => m.Name)));


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainFrm());
        }
    }
}
