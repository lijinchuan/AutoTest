"""
Pytest 根 conftest — 全局 fixtures 和 hooks

对应现有 C# 项目中的 Program.Main 初始化逻辑:
  - 加载配置
  - 初始化日志
  - 全局异常处理 → 截图
"""
import logging
import sys
from pathlib import Path

import allure
import pytest

# 确保 src 在 sys.path 中
sys.path.insert(0, str(Path(__file__).parent / "src"))


def pytest_configure(config):
    """
    pytest 启动配置
    对应 Program.Main 中的初始化步骤
    """
    from autodesk.core.config import ConfigLoader
    from autodesk.core.logger import setup_logging

    # 加载配置
    cfg = ConfigLoader.load(
        settings_path=str(Path(__file__).parent / "config" / "settings.yaml"),
        envs_path=str(Path(__file__).parent / "config" / "envs.yaml"),
    )

    # 初始化日志
    setup_logging(
        log_file=cfg.logging.file,
        level=cfg.logging.level,
        max_bytes=cfg.logging.max_bytes,
        backup_count=cfg.logging.backup_count,
    )

    # Allure 环境信息
    allure_dir = Path(cfg.framework.allure_dir)
    allure_dir.mkdir(parents=True, exist_ok=True)

    import platform
    env_props = {
        "Framework": "AutoDesk",
        "Framework.Version": "1.0.0",
        "Python.Version": platform.python_version(),
        "OS": f"{platform.system()} {platform.release()}",
        "UI Automation": "uiautomation",
    }

    # 写入 allure 环境文件
    env_file = allure_dir / "environment.properties"
    with open(env_file, "w", encoding="utf-8") as f:
        for k, v in env_props.items():
            f.write(f"{k}={v}\n")


@pytest.fixture(scope="session")
def global_config():
    """Session 级配置 fixture，对应 App.config"""
    from autodesk.core.config import ConfigLoader
    return ConfigLoader.get_config()


@pytest.fixture(scope="session")
def env_config(global_config):
    """当前环境配置 fixture，对应 TestEnv"""
    from autodesk.core.config import ConfigLoader
    return ConfigLoader.get_env()


# ------------------------------------------------------------------
# 失败自动截图 hook
# ------------------------------------------------------------------

@pytest.hookimpl(tryfirst=True, hookwrapper=True)
def pytest_runtest_makereport(item, call):
    """测试结果报告 hook — 失败时自动截图"""
    outcome = yield
    report = outcome.get_result()

    if report.when == "call" and report.failed:
        # 尝试从 fixture 中获取 Application 实例并截图
        try:
            _capture_failure_screenshot(item, report)
        except Exception:
            pass


def _capture_failure_screenshot(item, report):
    """失败时自动截图并附加到 Allure"""
    from autodesk.core.config import ConfigLoader
    from autodesk.report.allure_helper import AllureReporter

    config = ConfigLoader.get_config()

    if not config.framework.screenshot_on_failure:
        return

    # 尝试从 fixture 中获取 app 实例
    app = None
    for fixture_name in ["notepad_app", "calc_app", "app"]:
        try:
            app = item.funcargs.get(fixture_name)
            if app is not None:
                break
        except Exception:
            continue

    if app is None:
        # 尝试查找任何包含 "app" 的 fixture
        for name, val in item.funcargs.items():
            if hasattr(val, "screenshot"):
                app = val
                break

    if app and hasattr(app, "screenshot"):
        try:
            img = app.screenshot()
            AllureReporter.attach_screenshot(img, name="failure_screenshot")
        except Exception:
            pass
