"""
测试级 pytest fixtures
管理桌面应用的启动和关闭生命周期。

对应现有 C# 中 TaskBiz 构建 TestTask 的逻辑:
  TaskBiz.CreateTask() → 构建 TestTask → WebTask.Execute()

但使用 pytest fixture 的 yield 模式:
  setup (启动应用) → yield (执行测试) → teardown (关闭应用)
"""
import pytest

from autodesk.core.config import ConfigLoader
from autodesk.desktop.app import Application


# ------------------------------------------------------------------
# Notepad fixtures
# ------------------------------------------------------------------

@pytest.fixture(scope="function")
def notepad_app(global_config):
    """
    Notepad 应用生命周期 fixture。

    scope="function" 意味着每个测试方法都会获得全新的 Notepad 实例。
    如果想在多个测试间共享，可改为 scope="module" 或 "session"。
    """
    # 尝试匹配 Windows 版本
    try:
        app_config = ConfigLoader.get_app("notepad_zh")
    except KeyError:
        app_config = ConfigLoader.get_app("notepad")

    app = Application(app_config)
    app.launch()
    app.wait_ready()

    yield app

    # teardown: 关闭应用
    try:
        app.close()
    except Exception:
        app.kill()


@pytest.fixture(scope="function")
def notepad_main(notepad_app):
    """
    Notepad 主窗口 Page Object fixture。
    测试方法直接使用这个 fixture 即可操作记事本。

    使用方式:
        def test_xxx(notepad_main):
            notepad_main.type_text("hello")
            assert "hello" in notepad_main.get_text()
    """
    from autodesk.pom.notepad.main_window import NotepadMainWindow
    return NotepadMainWindow(notepad_app)
