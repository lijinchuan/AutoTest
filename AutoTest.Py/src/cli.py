"""
CLI 入口点
对应现有 C# 项目的双模式执行（交互式 UI + 定时调度 + API 服务）

命令:
    autodesk run      — 执行测试
    autodesk schedule — 启动 cron 调度器
    autodesk serve    — 启动 HTTP API 服务（预留）
    autodesk allure   — 生成并打开 allure 报告
"""
import subprocess
import sys
from pathlib import Path

import click


PROJECT_ROOT = Path(__file__).parent.parent


@click.group()
def main():
    """AutoDesk - Desktop Automated Testing Framework
    基于 pytest + uiautomation + allure 的桌面程序自动化测试工具
    """
    pass


@main.command()
@click.option("--test-file", "-f", default=None,
              help="指定测试文件路径（不指定则运行全部）")
@click.option("--marker", "-m", default="desktop",
              help="pytest marker 过滤（默认: desktop）")
@click.option("--allure-dir", default="results/allure",
              help="Allure 结果目录（默认: results/allure）")
@click.option("--parallel", "-n", default=0,
              help="并行执行 worker 数（0=串行）")
def run(test_file: str, marker: str, allure_dir: str, parallel: int):
    """运行桌面自动化测试"""
    cmd = [
        sys.executable, "-m", "pytest",
        f"--alluredir={allure_dir}",
        "-v",
        "--tb=short",
    ]

    if marker:
        cmd.extend(["-m", marker])

    if parallel > 0:
        cmd.extend(["-n", str(parallel)])

    if test_file:
        cmd.append(test_file)
    else:
        cmd.append("src/autodesk/tests/")

    # 在项目根目录执行
    click.echo(f"执行: {' '.join(cmd)}")
    result = subprocess.run(cmd, cwd=str(PROJECT_ROOT))
    sys.exit(result.returncode)


@main.command()
@click.option("--results-dir", default="results/allure",
              help="Allure 结果目录")
@click.option("--port", default=0,
              help="Allure 服务端口（0=自动）")
def allure_serve(results_dir: str, port: int):
    """生成并启动 Allure 报告服务"""
    allure_exe = "allure"

    # 检查 allure 是否可用
    check = subprocess.run(
        [allure_exe, "--version"],
        capture_output=True, text=True
    )
    if check.returncode != 0:
        click.echo("错误: 未找到 allure 命令行工具。")
        click.echo("请安装: https://docs.qameta.io/allure-report/#_installing_a_commandline")
        sys.exit(1)

    results_path = PROJECT_ROOT / results_dir
    if not results_path.exists():
        click.echo(f"错误: Allure 结果目录不存在: {results_path}")
        sys.exit(1)

    cmd = [allure_exe, "serve", str(results_path)]
    if port > 0:
        cmd.extend(["-p", str(port)])

    click.echo(f"执行: {' '.join(cmd)}")
    subprocess.run(cmd, cwd=str(PROJECT_ROOT))


@main.command()
@click.option("--port", "-p", default=55555,
              help="API 服务端口（默认 55555，对应现有 SimulateServerPort）")
def serve(port: int):
    """启动 HTTP API 服务（对应现有 SimulateServer）"""
    click.echo(f"API 服务功能预留，计划端口: {port}")
    # TODO: 实现 Flask/FastAPI 服务
    click.echo("此功能尚未实现。")


@main.command()
@click.option("--config", "-c", default="config/schedule.yaml",
              help="调度配置文件")
def schedule(config: str):
    """启动 cron 定时调度器（对应现有 AutoTaskBiz）"""
    click.echo(f"调度功能预留，配置文件: {config}")
    # TODO: 实现 DesktopTaskScheduler
    click.echo("此功能尚未实现。")


if __name__ == "__main__":
    main()
