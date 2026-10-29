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
            try
            {
                using (var currprocess = Process.GetCurrentProcess())
                {
                    var currFileName = currprocess.MainModule?.FileName;

                    foreach (Process p in pa)
                    {
                        try
                        {
                            if (p.Id != currprocess.Id
                                && p.ProcessName == currprocess.ProcessName
                                && p.MainModule?.FileName == currFileName)
                            {
                                //MessageBox.Show("另一个进程正在运行，无法启动。");
                                SwitchToThisWindow(p.MainWindowHandle, true);
                                return;
                            }
                        }
                        catch
                        {
                            //个别进程（系统进程）不允许读取 MainModule，跳过
                        }
                    }
                }
            }
            finally
            {
                //Process 持有系统句柄，用完必须释放
                foreach (Process p in pa)
                {
                    try
                    {
                        p.Dispose();
                    }
                    catch
                    {
                    }
                }
            }

            //原来设置成 1000，配合大量阻塞等待会让线程池瞬间膨胀到几百上千个线程，
            //每个线程 1MB 栈空间，本身就是一笔可观的内存开销
            LJC.FrameWorkV3.Comm.ThreadPoolHelper.CheckSetMinThreads(200, 200);

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

            //主窗体及其子控件（所有浏览器）已经释放，这里统一收尾 CEF 资源
            DisposeCefResources();

            if (!string.IsNullOrWhiteSpace(simulateServerPort))
            {
                Biz.SimulateServer.SimulateServerManager.Stop();
            }
        }

        /// <summary>
        /// 释放 CEF 资源。
        /// 顺序：先释放所有窗口（含浏览器）、DevToolsClient、浏览器上下文，再 Cef.Shutdown()，
        /// 最后兜底清理残留子进程。Cef.Shutdown 必须与 Cef.Initialize 在同一线程执行。
        /// </summary>
        private static void DisposeCefResources()
        {
            try
            {
                foreach (Form form in Application.OpenForms.Cast<Form>().ToList())
                {
                    try
                    {
                        form.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            try
            {
                WebBrowser.CefRemoteCaptureBridge.Clear();
            }
            catch
            {
            }

            try
            {
                WebBrowser.WebBrowserTool.DisposeAllDevToolsClients();
            }
            catch
            {
            }

            try
            {
                WebBrowser.BrowserContextCache.DisposeAll();
            }
            catch
            {
            }

            //Cef.Shutdown 因为浏览器/上下文没有完全释放而卡住时，不能让进程永远停在这里
            var shutdownCompleted = false;
            var watchdog = new Thread(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(30));
                if (!shutdownCompleted)
                {
                    try
                    {
                        LJC.FrameWorkV3.LogManager.LogHelper.Instance.Error("Cef.Shutdown 超过30秒未完成，强制退出");
                    }
                    catch
                    {
                    }

                    try
                    {
                        MainFrm.KillBrowserSubprocess();
                    }
                    catch
                    {
                    }

                    Environment.Exit(0);
                }
            });
            watchdog.IsBackground = true;
            watchdog.Start();

            try
            {
                CefSharp.Cef.Shutdown();
            }
            catch (Exception ex)
            {
                LJC.FrameWorkV3.LogManager.LogHelper.Instance.Error("Cef.Shutdown 失败", ex);
            }
            finally
            {
                shutdownCompleted = true;
            }

            try
            {
                MainFrm.KillBrowserSubprocess();
            }
            catch
            {
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LJC.FrameWorkV3.LogManager.LogHelper.Instance.Error("Application_ThreadException", e.Exception);

            Environment.Exit(-1);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //必须记录真实的异常对象，否则日志里只有类型名，排查启动崩溃时什么都看不到
            var ex = e.ExceptionObject as Exception;
            LJC.FrameWorkV3.LogManager.LogHelper.Instance.Error("UnhandledException", ex ?? new ApplicationException("程序中止:" + e));

            Environment.Exit(-1);
        }
    }
}
