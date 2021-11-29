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
