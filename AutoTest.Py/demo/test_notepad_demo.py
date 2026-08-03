"""
Notepad 自动化测试 Demo
演示使用 AutoDesk 框架编写桌面测试脚本

在 C# AutoTest.UI 中，将此代码粘贴到 TestCase 的"脚本(Python)"标签页即可运行。
"""
import sys
from pathlib import Path

# 添加框架路径
sys.path.insert(0, r"D:\Git\AutoTest\AutoTest.Py\src")

from autodesk.core.config import ConfigLoader
from autodesk.desktop.app import Application

# 加载配置
ConfigLoader.load(r"D:\Git\AutoTest\AutoTest.Py\config\settings.yaml")

# 启动 Notepad
app_config = ConfigLoader.get_app("notepad")
app = Application(app_config)
app.launch()
app.wait_ready()

# 获取编辑区域
from autodesk.desktop.element import DesktopElement
from autodesk.desktop.locator import Locator
import uiautomation as uia

edit_locator = Locator.by_control_type("EditControl")
window = app.get_main_window()
edit_ctrl = window.ControlType(uia.ControlType.EditControl)
edit_element = DesktopElement(edit_ctrl, edit_locator)

# 输入文本
edit_element.set_text("Hello, AutoDesk! 桌面自动化测试成功！")

# 验证
import time
time.sleep(0.5)
text = edit_element.get_text()
assert "AutoDesk" in text, f"验证失败: 文本中不包含 'AutoDesk'，实际内容: {text}"

print(f"✅ 测试通过！记事本内容: {text}")

# 关闭
app.close()
