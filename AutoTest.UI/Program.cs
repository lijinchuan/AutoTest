using AutoTest.Biz;
using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern ErrorModes SetErrorMode(ErrorModes uMode);

        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // initialize data store switcher (default: BIGENTITY). Can be switched via App.config DataStore key.
            try
            {
                AutoTest.Data.DataStoreSwitcher.Init();
            }
            catch { }
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

            LJC.FrameWorkV3.Comm.ThreadPoolHelper.CheckSetMinThreads(1000, 1000);

            // Data store initialization is handled by the selected IDataStore implementation.
            // Ensure DI container initialization always runs.
            AutofacBuilder.init();

            var simulateServerPort = System.Configuration.ConfigurationManager.AppSettings["SimulateServerPort"];
            if (!string.IsNullOrWhiteSpace(simulateServerPort))
            {
                Biz.SimulateServer.SimulateServerManager.StartServer(int.Parse(simulateServerPort));
            }

            //处理未捕获的异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainFrm());

            if (!string.IsNullOrWhiteSpace(simulateServerPort))
            {
                Biz.SimulateServer.SimulateServerManager.Stop();
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LJC.FrameWorkV3.LogManager.LogHelper.Instance.Error("Application_ThreadException", e.Exception);

            Environment.Exit(-1);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LJC.FrameWorkV3.LogManager.LogHelper.Instance.Error("UnhandledException", new ApplicationException("程序中止:" + e));

            Environment.Exit(-1);
        }
    }
}
