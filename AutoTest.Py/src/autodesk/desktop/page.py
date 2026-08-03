"""
Page Object Model 基类
借鉴 Selenium POM 模式，适配桌面应用测试。

对应现有 C# 项目中的测试层级:
  TestSource → TestSite → TestPage → TestCase
映射为:
  Application → Window → Dialog → TestMethod

每个桌面窗口/对话框对应一个 Page Object 类。
"""
import re
import time
from pathlib import Path
from typing import Optional

import uiautomation as uia

from autodesk.core.config import ConfigLoader
from autodesk.core.exceptions import ElementNotFoundError
from autodesk.core.logger import get_logger
from autodesk.desktop.app import Application
from autodesk.desktop.element import DesktopElement
from autodesk.desktop.locator import Locator

logger = get_logger(__name__)


class BasePage:
    """
    所有 Page Object 的基类，代表一个桌面窗口或对话框。

    子类使用方式:
        class MyWindow(BasePage):
            OK_BUTTON = Locator.by_name("确定")
            EDIT_BOX = Locator.by_control_type("EditControl")

            @property
            def ok_button(self) -> DesktopElement:
                return DesktopElement(self._find_element(self.OK_BUTTON), self.OK_BUTTON)

            def click_ok(self) -> "MyWindow":
                self.ok_button.click()
                return self
    """

    def __init__(self, app: Application):
        self.app: Application = app
        self.window: uia.Control = app.get_main_window()

    # ------------------------------------------------------------------
    # 元素查找
    # ------------------------------------------------------------------

    def _find_element(self, locator: Locator, timeout: float = None) -> uia.Control:
        """
        根据定位策略查找单个元素。

        :param locator: 定位器
        :param timeout: 超时时间（秒），默认使用配置中的 default_timeout
        :return: uiautomation Control
        :raises ElementNotFoundError: 未找到元素
        """
        if timeout is None:
            timeout = ConfigLoader.get_config().framework.default_timeout

        deadline = time.time() + timeout
        last_error = None

        while time.time() < deadline:
            try:
                controls = self._search_elements(locator)
                if controls and locator.index < len(controls):
                    ctrl = controls[locator.index]
                    logger.debug(f"找到元素: {locator} → Name='{ctrl.Name}'")
                    return ctrl
            except Exception as e:
                last_error = e

            time.sleep(ConfigLoader.get_config().framework.implicit_wait)

        raise ElementNotFoundError(locator, timeout)

    def _find_elements(self, locator: Locator, timeout: float = None) -> list[uia.Control]:
        """
        查找所有匹配的元素。

        :param locator: 定位器
        :param timeout: 超时时间（秒）
        :return: 匹配的控件列表
        """
        if timeout is None:
            timeout = ConfigLoader.get_config().framework.default_timeout

        deadline = time.time() + timeout

        while time.time() < deadline:
            controls = self._search_elements(locator)
            if controls:
                return controls
            time.sleep(ConfigLoader.get_config().framework.implicit_wait)

        return []

    def _search_elements(self, locator: Locator) -> list[uia.Control]:
        """在窗口中搜索匹配 locator 的控件"""
        # 确保窗口引用有效
        if self.window is None or not self.window.Exists(0, 0.1):
            self.window = self.app.get_main_window()

        conditions: dict = {}

        if locator.strategy == "name":
            conditions["Name"] = locator.value
        elif locator.strategy == "automation_id":
            conditions["AutomationId"] = locator.value
        elif locator.strategy == "class_name":
            conditions["ClassName"] = locator.value
        elif locator.strategy == "control_type":
            conditions["ControlType"] = getattr(uia.ControlType, locator.value, locator.value)

        if conditions:
            controls = self.window.GetChildren()
            # 按条件过滤
            results = []
            for ctrl in controls:
                match = True
                for attr, val in conditions.items():
                    try:
                        ctrl_val = getattr(ctrl, attr, None)
                        if isinstance(ctrl_val, str) and isinstance(val, str):
                            if ctrl_val != val:
                                match = False
                                break
                        elif ctrl_val != val:
                            match = False
                            break
                    except Exception:
                        match = False
                        break
                if match:
                    results.append(ctrl)

            # 如果顶层没找到，尝试深度搜索
            if not results:
                for attr, val in conditions.items():
                    try:
                        child = getattr(self.window, attr)(val)
                        if child and child.Exists(0, 0.1):
                            results.append(child)
                    except Exception:
                        pass

            return results

        elif locator.strategy == "regex":
            controls = self.window.GetChildren()
            pattern = re.compile(locator.value)
            return [
                ctrl for ctrl in controls
                if pattern.search(ctrl.Name or "")
            ]

        return []

    # ------------------------------------------------------------------
    # 等待方法
    # ------------------------------------------------------------------

    def wait_until_visible(self, locator: Locator, timeout: float = None) -> "BasePage":
        """等待元素变为可见"""
        self._find_element(locator, timeout)
        return self

    def wait_until_disappears(self, locator: Locator, timeout: float = None) -> "BasePage":
        """等待元素消失"""
        if timeout is None:
            timeout = ConfigLoader.get_config().framework.default_timeout

        deadline = time.time() + timeout
        while time.time() < deadline:
            elements = self._search_elements(locator)
            if not elements:
                return self
            time.sleep(0.3)

        raise ElementNotFoundError(
            f"等待元素消失超时 ({timeout}s): {locator}"
        )

    def is_visible(self, locator: Locator, timeout: float = 1.0) -> bool:
        """检查元素是否可见"""
        try:
            self._find_element(locator, timeout)
            return True
        except ElementNotFoundError:
            return False

    # ------------------------------------------------------------------
    # 窗口管理
    # ------------------------------------------------------------------

    def refresh(self) -> None:
        """刷新窗口引用（对话框切换后使用）"""
        self.app.refresh()
        self.window = self.app.get_main_window()

    # ------------------------------------------------------------------
    # 截图
    # ------------------------------------------------------------------

    def take_screenshot(self, name: str = "") -> str:
        """
        截图并返回文件路径。

        :param name: 截图名称（不含扩展名）
        :return: 截图文件路径
        """
        config = ConfigLoader.get_config()
        ss_dir = Path(config.framework.screenshot_dir)
        ss_dir.mkdir(parents=True, exist_ok=True)

        if not name:
            name = f"screenshot_{int(time.time() * 1000)}"

        filepath = ss_dir / f"{name}.png"
        img = self.app.screenshot()
        img.save(str(filepath))
        logger.info(f"截图已保存: {filepath}")
        return str(filepath)

    def __repr__(self) -> str:
        cls = self.__class__.__name__
        title = self.window.Name if self.window else "?"
        return f"{cls}(window='{title}')"
