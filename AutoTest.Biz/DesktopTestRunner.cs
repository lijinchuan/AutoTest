using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.LogManager;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTest.Biz
{
    /// <summary>
    /// 桌面测试执行器
    /// 对应现有 RunTestTask，但不再使用 CefSharp 浏览器 + JS 脚本，
    /// 而是启动 Python 进程执行 pytest + uiautomation 脚本。
    ///
    /// Python = 现在的 JS 测试脚本
    /// </summary>
    public class DesktopTestRunner
    {
        private readonly TestCase _testCase;
        private readonly TestEnv _testEnv;
        private readonly System.Collections.Generic.List<TestEnvParam> _testEnvParams;
        private readonly Action<TestResult> _notify;

        private TestResult _testResult;
        private Process _pythonProcess;
        private CancellationTokenSource _cts;
        private bool _cancelled = false;

        // Python 项目根目录（从 bin/Debug 向上4级到解决方案根目录）
        private static readonly string PyProjectRoot = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\AutoTest.Py"));

        // 超时时间（毫秒），对应现有 RunTestCodeTimeOut
        private const int DefaultTimeoutMs = 10 * 60 * 1000; // 10 分钟

        public DesktopTestRunner(
            TestCase testCase,
            TestEnv testEnv,
            System.Collections.Generic.List<TestEnvParam> testEnvParams,
            Action<TestResult> notify)
        {
            _testCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
            _testEnv = testEnv;
            _testEnvParams = testEnvParams ?? new System.Collections.Generic.List<TestEnvParam>();
            _notify = notify;

            _testResult = new TestResult
            {
                EnvId = testEnv?.Id ?? 0,
                TestCaseId = testCase.Id,
                Success = false,
                TestStartDate = DateTime.Now,
                TestEndDate = DateTime.Now,
                IsTimeOut = false
            };
        }

        /// <summary>
        /// 执行桌面测试
        /// </summary>
        public async Task<TestResult> RunAsync()
        {
            _testResult.TestStartDate = DateTime.Now;
            _cts = new CancellationTokenSource();

            try
            {
                // 1. 准备 Python 测试脚本文件
                string scriptPath = PrepareTestScript();

                // 2. 执行 pytest
                var resultJson = await RunPythonTestAsync(scriptPath);

                // 3. 结果已在 RunPythonTestAsync 中设置

                // 4. 清理临时文件
                CleanupScript(scriptPath);
            }
            catch (OperationCanceledException)
            {
                _testResult.IsTimeOut = true;
                _testResult.FailMsg = "测试超时或被取消";
                _testResult.Success = false;
            }
            catch (Exception ex)
            {
                _testResult.Success = false;
                _testResult.FailMsg = $"桌面测试执行异常: {ex.Message}";
                LogHelper.Instance.Error($"DesktopTestRunner 异常 [{_testCase.CaseName}]", ex);
            }
            finally
            {
                _testResult.TestEndDate = DateTime.Now;
                KillPythonProcess();
                FinishTest();
            }

            return _testResult;
        }

        /// <summary>
        /// 将 TestCode 写入临时 .py 文件
        /// </summary>
        private string PrepareTestScript()
        {
            // 如果 TestCode 本身就是文件路径，则直接使用
            if (!string.IsNullOrWhiteSpace(_testCase.TestCode))
            {
                var code = ReplaceEnvParams(_testCase.TestCode);

                // 判断是脚本文件路径还是内联代码
                if (File.Exists(code))
                {
                    return code;
                }

                // 内联代码 → 写入临时文件
                string tempDir = Path.Combine(Path.GetTempPath(), "AutoDesk");
                Directory.CreateDirectory(tempDir);

                string scriptPath = Path.Combine(tempDir, $"test_{_testCase.Id}_{DateTime.Now:yyyyMMddHHmmss}.py");

                // 预计算路径
                string pyRoot = PyProjectRoot;
                string pySrc = Path.Combine(PyProjectRoot, "src");
                string cfgPath = Path.Combine(PyProjectRoot, "config", "settings.yaml");

                // 构建完整的测试脚本
                var sb = new StringBuilder();
                sb.AppendLine("# Auto-generated test script by AutoDesk");
                sb.AppendLine("# TestCase: " + (_testCase.CaseName ?? ""));

                // 如果代码已经自带框架导入，不重复添加
                if (!code.Contains("from autodesk") && !code.Contains("import autodesk"))
                {
                    sb.AppendLine("import sys");
                    sb.Append("sys.path.insert(0, r\"").Append(pyRoot).AppendLine("\")");
                    sb.Append("sys.path.insert(0, r\"").Append(pySrc).AppendLine("\")");
                    sb.AppendLine("from autodesk.desktop.app import Application");
                    sb.AppendLine("from autodesk.desktop.element import DesktopElement");
                    sb.AppendLine("from autodesk.desktop.locator import Locator");
                    sb.AppendLine("import uiautomation as uia");
                    sb.AppendLine();
                }

                sb.AppendLine(code);

                // 如果有 ValidCode，添加验证逻辑
                if (!string.IsNullOrWhiteSpace(_testCase.ValidCode))
                {
                    sb.AppendLine();
                    sb.AppendLine("# 验证代码");
                    sb.AppendLine(ReplaceEnvParams(_testCase.ValidCode));
                }

                File.WriteAllText(scriptPath, sb.ToString(), Encoding.UTF8);
                LogHelper.Instance.Info($"测试脚本已生成: {scriptPath}");
                return scriptPath;
            }

            throw new InvalidOperationException("TestCase.TestCode 为空");
        }

        /// <summary>
        /// 自动查找 Python 可执行文件路径
        /// 优先找真实安装的 Python，避开 WindowsApps 的 Store stub
        /// </summary>
        private static string FindPythonExe()
        {
            // 常见安装路径（按优先级）
            var searchPaths = new[]
            {
                // Python 3.13+ 用户级安装
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs", "Python", "Python313", "python.exe"),
                // Python 3.12
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs", "Python", "Python312", "python.exe"),
                // Python 3.11
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs", "Python", "Python311", "python.exe"),
                // 系统级安装
                @"C:\Program Files\Python313\python.exe",
                @"C:\Program Files\Python312\python.exe",
                @"C:\Program Files\Python311\python.exe",
                @"C:\Python313\python.exe",
                @"C:\Python312\python.exe",
            };

            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            // 回退：尝试 PATH 中的 python（但可能是 stub）
            return "python";
        }
        private async Task<string> RunPythonTestAsync(string scriptPath)
        {
            // 设置 PYTHONPATH 让 Python 能找到 src/ 下的 autodesk 包
            var envVars = new System.Collections.Generic.Dictionary<string, string>();
            foreach (System.Collections.DictionaryEntry kv in Environment.GetEnvironmentVariables())
            {
                envVars[kv.Key.ToString()] = kv.Value?.ToString() ?? "";
            }
            var pythonPath = Path.GetFullPath(Path.Combine(PyProjectRoot, "src"));
            if (envVars.ContainsKey("PYTHONPATH"))
            {
                envVars["PYTHONPATH"] = pythonPath + ";" + envVars["PYTHONPATH"];
            }
            else
            {
                envVars["PYTHONPATH"] = pythonPath;
            }

            string pythonExe = FindPythonExe();
            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptPath}\"",
                WorkingDirectory = PyProjectRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                CreateNoWindow = true,
            };
            foreach (var kv in envVars)
            {
                psi.EnvironmentVariables[kv.Key] = kv.Value;
            }

            LogHelper.Instance.Info($"启动 Python 测试: {psi.FileName} {psi.Arguments}");

            _pythonProcess = new Process { StartInfo = psi };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            _pythonProcess.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null) outputBuilder.AppendLine(e.Data);
            };
            _pythonProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null) errorBuilder.AppendLine(e.Data);
            };

            _pythonProcess.Start();
            _pythonProcess.BeginOutputReadLine();
            _pythonProcess.BeginErrorReadLine();

            // 等待进程结束，带超时
            var tcs = new TaskCompletionSource<bool>();
            var timeoutTask = Task.Delay(DefaultTimeoutMs, _cts.Token);

            var waitTask = Task.Run(() =>
            {
                _pythonProcess.WaitForExit();
                tcs.TrySetResult(true);
            });

            var completedTask = await Task.WhenAny(waitTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                KillPythonProcess();
                _testResult.IsTimeOut = true;
                throw new TimeoutException($"桌面测试超时 ({DefaultTimeoutMs / 1000}s)");
            }

            await tcs.Task; // 确保 WaitForExit 完成
            _pythonProcess.WaitForExit(); // 二次确认

            string output = outputBuilder.ToString();
            string error = errorBuilder.ToString();

            LogHelper.Instance.Info($"Python 进程退出码: {_pythonProcess.ExitCode}");
            if (!string.IsNullOrWhiteSpace(error))
            {
                LogHelper.Instance.Warn($"Python stderr: {error}");
            }

            // 简单脚本模式：exit code 0 且没有异常 → 成功
            if (_pythonProcess.ExitCode == 0)
            {
                _testResult.Success = true;
                string summary = output?.Trim();
                if (summary?.Length > 500) summary = summary.Substring(0, 500);
                _testResult.FailMsg = summary ?? "执行成功";
                _testResult.ResultContent = output;
                return output ?? "";
            }
            else
            {
                _testResult.Success = false;
                string errMsg = error?.Trim() ?? output?.Trim() ?? "未知错误";
                if (errMsg.Length > 500) errMsg = errMsg.Substring(0, 500);
                _testResult.FailMsg = errMsg;
                _testResult.ResultContent = output;
                return output ?? "";
            }
        }

        /// <summary>
        /// 从 stdout 中提取 JSON 行
        /// </summary>
        private string ExtractJsonLine(string output)
        {
            if (string.IsNullOrWhiteSpace(output)) return null;

            // runner.py 输出的 JSON 在最后一行（或独立一行）
            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                {
                    try
                    {
                        JsonConvert.DeserializeObject(trimmed);
                        return trimmed;
                    }
                    catch { }
                }
            }
            return null;
        }

        /// <summary>
        /// 解析 Python 返回的 JSON 结果
        /// </summary>
        private void ParseResult(string resultJson)
        {
            if (string.IsNullOrWhiteSpace(resultJson)) return;

            try
            {
                var result = JsonConvert.DeserializeObject<dynamic>(resultJson);

                bool success = result.success ?? false;
                int passed = result.passed ?? 0;
                int failed = result.failed ?? 0;
                int total = result.total ?? 0;

                _testResult.Success = success;
                _testResult.ResultContent = resultJson;

                if (!success)
                {
                    string failures = result.failures?.ToString() ?? "";
                    string error = result.error?.ToString() ?? "";
                    _testResult.FailMsg = $"失败 {failed}/{total}。{error} {failures}".Trim();
                }
                else
                {
                    _testResult.FailMsg = $"通过 {passed}/{total}";
                }

                LogHelper.Instance.Info(
                    $"桌面测试结果 [{_testCase.CaseName}]: " +
                    $"Success={success}, Passed={passed}, Failed={failed}, Total={total}");
            }
            catch (Exception ex)
            {
                _testResult.Success = false;
                _testResult.FailMsg = $"解析测试结果失败: {ex.Message}";
                LogHelper.Instance.Error("解析 JSON 结果失败", ex);
            }
        }

        /// <summary>
        /// 替换环境变量参数
        /// 对应现有 Util.ReplaceEvnParams
        /// </summary>
        private string ReplaceEnvParams(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            if (_testEnvParams == null || _testEnvParams.Count == 0) return input;

            foreach (var param in _testEnvParams)
            {
                if (param?.Name == null) continue;
                input = input.Replace("{" + param.Name + "}", param.Val ?? "");
            }
            return input;
        }

        /// <summary>
        /// 强制终止 Python 进程
        /// </summary>
        private void KillPythonProcess()
        {
            try
            {
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    _pythonProcess.Kill();
                    _pythonProcess.WaitForExit(5000);
                    LogHelper.Instance.Info("Python 进程已终止");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Warn($"终止 Python 进程异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理临时脚本文件
        /// </summary>
        private void CleanupScript(string scriptPath)
        {
            try
            {
                // 只清理 Temp 目录下的自动生成文件
                if (scriptPath != null && scriptPath.Contains("AutoDesk") && File.Exists(scriptPath))
                {
                    File.Delete(scriptPath);
                }
            }
            catch { }
        }

        /// <summary>
        /// 取消测试执行
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
            _cts?.Cancel();
            KillPythonProcess();
            _testResult.FailMsg = "测试被取消";
            _testResult.Success = false;
            _testResult.TestEndDate = DateTime.Now;
            FinishTest();
        }

        /// <summary>
        /// 完成测试，保存结果并通知
        /// 对应 RunTestTask.FinishTest()
        /// </summary>
        private void FinishTest()
        {
            try
            {
                AutoTest.Data.DataStoreSwitcher.Current?.Insert(nameof(TestResult), _testResult);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Error("保存 TestResult 失败", ex);
            }

            _notify?.Invoke(_testResult);
        }
    }
}
