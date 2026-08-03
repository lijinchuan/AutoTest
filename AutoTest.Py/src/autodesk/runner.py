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
import subprocess
import sys
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

    # 解析 pytest 输出
    stdout = result.stdout
    stderr = result.stderr

    # 简单解析：查找 pytest 的 summary line
    total = 0
    passed = 0
    failed = 0
    failures = []

    for line in stdout.split("\n"):
        line = line.strip()
        # pytest 7.x summary: "3 passed", "1 failed"
        if "passed" in line and ("failed" in line or "=" in line):
            import re
            pass_match = re.search(r'(\d+)\s+passed', line)
            fail_match = re.search(r'(\d+)\s+failed', line)
            if pass_match:
                passed = int(pass_match.group(1))
            if fail_match:
                failed = int(fail_match.group(1))
            total = passed + failed

    # 如果没解析到，用退出码判断
    if total == 0:
        if result.returncode == 0:
            passed = 1
            total = 1
        else:
            failed = 1
            total = 1

    # 提取失败详情
    if stderr:
        failures.append(stderr[:2000])  # 截断

    return {
        "success": result.returncode == 0 and failed == 0,
        "exit_code": result.returncode,
        "total": total,
        "passed": passed,
        "failed": failed,
        "duration": round(duration, 2),
        "failures": failures,
        "allure_dir": allure_dir,
        "stdout_tail": stdout[-1000:] if stdout else "",
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

    result = run_pytest(
        test_file=args.test_file,
        allure_dir=args.allure_dir,
        marker=args.marker,
        verbose=not args.quiet,
    )

    # 输出 JSON 结果到 stdout，C# 进程会捕获解析
    print(json.dumps(result, ensure_ascii=False))

    # 退出码: 0=成功, 1=失败
    sys.exit(0 if result.get("success") else 1)


if __name__ == "__main__":
    main()
