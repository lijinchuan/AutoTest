"""
Notepad 主窗口 Page Object

演示如何将桌面应用的窗口/对话框建模为 Page Object。
每个控件定义为 Locator，业务操作封装为方法并标记 @allure.step。
"""
import time

import allure
import uiautomation as uia

from autodesk.desktop.app import Application
from autodesk.desktop.element import DesktopElement
from autodesk.desktop.locator import Locator
from autodesk.desktop.page import BasePage


class NotepadMainWindow(BasePage):
    """
    Notepad (记事本) 主窗口的 Page Object。

    对应现有 C# 概念中的 TestPage — 一个被测页面的抽象。
    """

    # ---- 元素定位器定义 ----
    EDIT_AREA = Locator.by_control_type("EditControl")
    FILE_MENU = Locator.by_name("文件(F)")
    EDIT_MENU = Locator.by_name("编辑(E)")
    FORMAT_MENU = Locator.by_name("格式(O)")
    VIEW_MENU = Locator.by_name("查看(V)")
    HELP_MENU = Locator.by_name("帮助(H)")

    def __init__(self, app: Application):
        super().__init__(app)
        # 确保窗口就绪后再操作
        self.window = app.get_main_window()

    # ------------------------------------------------------------------
    # 元素访问器 (lazy resolution)
    # ------------------------------------------------------------------

    @property
    def edit_area(self) -> DesktopElement:
        """编辑区域"""
        el = self._find_element(self.EDIT_AREA)
        return DesktopElement(el, self.EDIT_AREA)

    # ------------------------------------------------------------------
    # 业务方法
    # ------------------------------------------------------------------

    @allure.step("输入文本: {text[:50]}...")
    def type_text(self, text: str) -> "NotepadMainWindow":
        """
        在编辑区中输入文本。
        对应现有 JS TestCode: $('#editor').val(text)
        """
        self.edit_area.set_text(text)
        return self

    @allure.step("获取文本内容")
    def get_text(self) -> str:
        """获取编辑区全部文本内容"""
        return self.edit_area.get_text()

    @allure.step("清空文本")
    def clear_text(self) -> "NotepadMainWindow":
        """清空编辑区所有文本"""
        self.edit_area.send_keys("{Ctrl}A{Delete}")
        time.sleep(0.2)
        return self

    @allure.step("替换所有文本: {text[:50]}...")
    def replace_text(self, text: str) -> "NotepadMainWindow":
        """替换编辑区全部内容"""
        self.clear_text()
        self.type_text(text)
        return self

    @allure.step("快捷键操作: Ctrl+A → Delete")
    def select_all_and_delete(self) -> "NotepadMainWindow":
        """全选并删除"""
        self.edit_area.send_keys("{Ctrl}A{Delete}")
        time.sleep(0.2)
        return self

    @allure.step("获取窗口标题")
    def get_title(self) -> str:
        """获取窗口标题"""
        self.window = self.app.get_main_window()
        return self.window.Name or ""

    @allure.step("关闭记事本")
    def close_notepad(self) -> None:
        """关闭记事本窗口"""
        self.app.close()
