using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.Guard
{
    class Program
    {
        public delegate bool ControlCtrlDelegate(int CtrlType);
        //设置关闭处理事件
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);
        private static ControlCtrlDelegate cancelHandler = new ControlCtrlDelegate(HandlerRoutine);

        private static Process FindPidFromIndexedProcessName(string indexedProcessName)
        {
            var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
            return Process.GetProcessById((int)parentId.NextValue());
        }

        /// <summary>
        /// 上次关闭时间
        /// </summary>
        private static DateTime LastCloseDate = DateTime.Now;
        /// <summary>
        /// 最大运行分钟数，超过时间后会启动重置程序
        /// </summary>
        private const int MaxRunMins = 720;

        /// <summary>
        /// 重启检查小时时间
        /// </summary>
        private const int CheckResetHour = 1;

        /// <summary>
        /// 显示一般信息
        /// </summary>
        /// <param name="msg"></param>
        static void PrintInfo(string msg)
        {
            Console.WriteLine($"【{DateTime.Now:yyyy-MM-dd HH:mm:ss}】{msg}");
        }

        /// <summary>
        /// 显示警告信息
        /// </summary>
        /// <param name="msg">信息</param>
        static void PrintWarn(string msg)
        {
            var defaultFontColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"【{DateTime.Now:yyyy-MM-dd HH:mm:ss}】{msg}");

            Console.ForegroundColor = defaultFontColor;
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="msg">信息</param>
        /// <param name="ex">错误</param>
        static void PrintError(string msg, Exception ex)
        {
            var defaultFontColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"【{DateTime.Now:yyyy-MM-dd HH:mm:ss}】{msg}:{ex}");

            Console.ForegroundColor = defaultFontColor;
        }

        /// <summary>
        /// 检查没有其它守护进程
        /// </summary>
        /// <returns></returns>
        static bool CheckNoOtherProcess()
        {
            var currprocess = Process.GetCurrentProcess();
            Process[] pa = Process.GetProcesses();//获取当前进程数组。
            foreach (Process p in pa)
            {
                if (p.ProcessName == currprocess.ProcessName && p.Id != currprocess.Id)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 结束进程
        /// </summary>
        /// <param name="p"></param>
        static void Kill(Process p)
        {
            p.Kill();
        }

        static void Main(string[] args)
        {
            if (!CheckNoOtherProcess())
            {
                PrintWarn("有其它守护进程正在运行，不能启动守护。");

                Thread.Sleep(3000);
                return;
            }

            PrintInfo("守护进程启动......");
            PrintWarn("开始守护，请不要手动关闭......");

            string processName = System.Configuration.ConfigurationManager.AppSettings["GraudeProcessName"];
            if (string.IsNullOrWhiteSpace(processName))
            {
                PrintWarn("没有配置要守护的进程，不能启动守护。");

                Thread.Sleep(3000);
                return;
            }

            if (processName.Equals(Process.GetCurrentProcess().ProcessName, StringComparison.OrdinalIgnoreCase))
            {
                PrintWarn("不能守护自己。");

                Thread.Sleep(3000);
                return;
            }

            _ = SetConsoleCtrlHandler(cancelHandler, true);

            while (true)
            {
                try
                {

                    Process[] pa = Process.GetProcesses();//获取当前进程数组。
                    List<Process> processList = new List<Process>();
                    foreach (Process p in pa)
                    {
                        if (p.ProcessName == processName)
                        {
                            if (p.Responding)
                            {
                                ////凌晨重置下爬虫程序,生产上发现浏览器组件有问题，运行几天后内部进程就会崩溃无法恢复
                                //if (DateTime.Now.Subtract(LastCloseDate).TotalMinutes > MaxRunMins && DateTime.Now.Hour >= CheckResetHour && DateTime.Now.Hour < CheckResetHour + 1)
                                //{
                                //    LastCloseDate = DateTime.Now;
                                //    PrintWarn($"运行超过{MaxRunMins}分钟后到点关闭重启......");
                                //}
                                //else
                                {
                                    processList.Add(p);
                                }
                            }
                            else
                            {
                                PrintWarn("主进程未响应，关闭......");
                                Kill(p);
                            }
                        }
                    }

                    if (processList.Count == 0)
                    {
                        PrintWarn("检查到要守护的进程未启动，启动进程.....");
                        //启动进程，并标识是由守护进程启动的
                        _ = Process.Start($"{processName}.exe", "guarde");
                    }
                    else if (processList.Count > 1)
                    {
                        try
                        {
                            //保留最近一个进程，其它的关闭
                            processList.OrderBy(p => p.StartTime).Skip(1).ToList().ForEach(p => Kill(p));
                        }
                        catch (Exception ex)
                        {
                            PrintError("关闭进程出错", ex);
                        }
                    }
                    else
                    {
                        var parentProcess = processList[0];
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + parentProcess.Id);
                        ManagementObjectCollection moc = searcher.Get();
                        foreach (ManagementObject mo in moc)
                        {
                            var pName = Convert.ToString(mo["Name"]);
                            var pId = Convert.ToInt32(mo["ProcessID"]);
                            if ("CefSharp.BrowserSubprocess.exe".Equals(pName, StringComparison.OrdinalIgnoreCase))
                            {
                                var childProcess = Process.GetProcessById(pId);
                                if (!childProcess.Responding)
                                {
                                    PrintWarn("子进程未响应，关闭......");
                                    Kill(parentProcess);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    PrintError("检查失败", ex);
                }

                Thread.Sleep(30000);
            }
        }

        /// <summary>
        /// 处理关闭事件
        /// </summary>
        /// <param name="CtrlType"></param>
        /// <returns></returns>
        public static bool HandlerRoutine(int CtrlType)
        {
            switch (CtrlType)
            {
                case 0:
                    Console.WriteLine("确认关闭请连续两次回车"); //Ctrl+C关闭  
                    break;
                case 2:
                    Console.WriteLine("确认关闭请连续两次回车");//按控制台关闭按钮关闭  
                    break;
            }
            Console.ReadLine();
            Console.ReadLine();
            return false;
        }
    }
}
