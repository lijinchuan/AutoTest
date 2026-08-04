"""
C# ↔ Python 测试执行桥梁

C# 调用方式:
    python -m autodesk.runner --test-file test_xxx.py [--allure-dir results/allure]

输出:
    JSON 格式的测试结果到 stdout
    退出码: 0=全部通过, 1=存在失败

这个模块对应现有 C# 项目中 RunTestTask.ExecuteInner() 的角色 —
不过是桌面测试版本：启动被测应用 → 执行 Python 脚本 → 验证结果。
"""
import argparse
import json
import re
import subprocess
import sys
import traceback
from datetime import datetime
from pathlib import Path
from typing import Optional


def run_pytest(test_file: str, allure_dir: str = "results/allure",
               marker: str = None, verbose: bool = True) -> dict:
    """
    执行 pytest 测试并返回结构化结果。

    :param test_file: 测试文件路径
    :param allure_dir: Allure 结果输出目录
    :param marker: pytest marker 过滤
    :param verbose: 是否详细输出
    :return: {"success": bool, "total": int, "passed": int, "failed": int,
              "duration": float, "failures": [...], "allure_dir": str}
    """
    test_path = Path(test_file)
    if not test_path.exists():
        return {
            "success": False,
            "error": f"测试文件不存在: {test_file}",
            "total": 0, "passed": 0, "failed": 1,
            "duration": 0, "failures": [f"File not found: {test_file}"],
        }

    # 构建 pytest 命令
    cmd = [
        sys.executable, "-m", "pytest",
        str(test_path),
        f"--alluredir={allure_dir}",
        "-v" if verbose else "",
        "--tb=short",
    ]
    if marker:
        cmd.extend(["-m", marker])

    # 过滤空字符串
    cmd = [c for c in cmd if c]

    start_time = datetime.now()

    try:
        result = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            timeout=600,  # 10 分钟超时，对应现有 RunTestCodeTimeOut
            cwd=str(Path(__file__).parent.parent.parent),  # AutoTest.Py 根目录
        )
    except subprocess.TimeoutExpired:
        duration = (datetime.now() - start_time).total_seconds()
        return {
            "success": False,
            "error": "测试执行超时 (600s)",
            "total": 0, "passed": 0, "failed": 1,
            "duration": duration,
            "failures": ["Timeout: Test execution exceeded 600 seconds"],
            "allure_dir": allure_dir,
        }

    duration = (datetime.now() - start_time).total_seconds()

    # ── 解析 pytest 输出 ──
    stdout = result.stdout
    stderr = result.stderr

    total = 0
    passed = 0
    failed = 0
    skipped = 0
    errors = 0
    failures = []

    # 收集 FAILED/ERROR 详情
    for line in (stdout + "\n" + stderr).split("\n"):
        stripped = line.strip()
        if stripped and (stripped.startswith("FAILED") or stripped.startswith("ERROR")):
            failures.append(stripped[:500])

    # 解析 pytest summary line，支持多种格式:
    #   "3 passed in 1.23s"
    #   "1 failed in 0.5s"
    #   "1 failed, 2 passed in 1.23s"
    #   "2 passed, 1 failed in 1.23s"
    #   "1 skipped in 0.1s"
    #   "no tests ran in 0.01s"
    #   "========== 3 passed in 1.23s =========="
    #   "========== 1 failed, 2 passed in 1.23s =========="
    for line in stdout.split("\n"):
        line = line.strip()

        # 尝试匹配数字 + 状态
        pass_m = re.search(r'(\d+)\s+passed', line)
        fail_m = re.search(r'(\d+)\s+failed', line)
        skip_m = re.search(r'(\d+)\s+skipped', line)
        err_m = re.search(r'(\d+)\s+errors?', line)

        if pass_m or fail_m or skip_m or err_m:
            if pass_m:
                passed = max(passed, int(pass_m.group(1)))
            if fail_m:
                failed = max(failed, int(fail_m.group(1)))
            if skip_m:
                skipped = max(skipped, int(skip_m.group(1)))
            if err_m:
                errors = max(errors, int(err_m.group(1)))

    total = passed + failed + skipped + errors

    # 如果 summary 没解析到（比如 "no tests ran"），用 pytest exit code
    if total == 0:
        # pytest exit codes: 0=all passed, 1=tests failed, 2=interrupted,
        #                     3=internal error, 4=usage error, 5=no tests collected
        if result.returncode == 0:
            passed = 0
            total = 0
        elif result.returncode == 5:
            failed = 1
            total = 1
            failures.insert(0, "pytest: no tests collected (exit code 5)")
        else:
            failed = 1
            total = 1

    # 如果 stderr 有内容但没被收集到 failures 中
    if stderr and not failures:
        failures.append(stderr[:2000])

    success = (result.returncode == 0 and failed == 0 and errors == 0)

    return {
        "success": success,
        "exit_code": result.returncode,
        "total": total,
        "passed": passed,
        "failed": failed,
        "skipped": skipped,
        "errors": errors,
        "duration": round(duration, 2),
        "failures": failures[:50],  # 最多保留50条
        "allure_dir": allure_dir,
        "stdout_tail": stdout[-1000:] if stdout else "",
        "stderr_tail": stderr[-500:] if stderr else "",
    }


def main():
    """CLI 入口 — 被 C# DesktopTestRunner 通过 Process.Start 调用"""
    parser = argparse.ArgumentParser(
        description="AutoDesk Test Runner - C# Bridge"
    )
    parser.add_argument(
        "--test-file", "-f",
        required=True,
        help="Python 测试文件路径"
    )
    parser.add_argument(
        "--allure-dir",
        default="results/allure",
        help="Allure 结果输出目录 (默认: results/allure)"
    )
    parser.add_argument(
        "--marker", "-m",
        default=None,
        help="pytest marker 过滤器"
    )
    parser.add_argument(
        "--quiet", "-q",
        action="store_true",
        help="简洁输出模式"
    )

    args = parser.parse_args()

    try:
        result = run_pytest(
            test_file=args.test_file,
            allure_dir=args.allure_dir,
            marker=args.marker,
            verbose=not args.quiet,
        )
    except Exception as ex:
        # runner 自身异常也以 JSON 形式返回
        result = {
            "success": False,
            "error": f"Runner 异常: {ex}",
            "total": 0, "passed": 0, "failed": 1,
            "duration": 0,
            "failures": [traceback.format_exc()],
        }

    # ★ 用特殊分隔符包裹 JSON，C# 解析时不会混淆
    print("<<<AUTODESK_RESULT>>>")
    print(json.dumps(result, ensure_ascii=False))
    print("<<<END_AUTODESK_RESULT>>>")

    # 退出码: 0=成功, 1=失败
    sys.exit(0 if result.get("success") else 1)


if __name__ == "__main__":
    main()
