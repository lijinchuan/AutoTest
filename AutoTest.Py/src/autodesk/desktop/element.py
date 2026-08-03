"""
UI 元素包装器
对应现有 C# 项目中 WebTask 通过 JS 操作 DOM 元素的模式，
适配为 uiautomation 控件操作。
"""
import time
from typing import Optional

import uiautomation as uia

from autodesk.core.exceptions import ElementInteractionError
from autodesk.core.logger import get_logger
from autodesk.desktop.locator import Locator

logger = get_logger(__name__)


class DesktopElement:
    """
    包装 uiautomation.Control，提供类型安全的交互方法。
    所有交互方法返回 self，支持链式调用。

    对应现有 JS 脚本中对 DOM 元素的 click/setText/getText 等操作。
    """

    def __init__(self, control: uia.Control, locator: Locator):
        self._control: uia.Control = control
        self.locator: Locator = locator

    # ------------------------------------------------------------------
    # 属性
    # ------------------------------------------------------------------

    @property
    def name(self) -> str:
        """控件名称"""
        self._refresh_if_needed()
        return self._control.Name or ""

    @property
    def is_enabled(self) -> bool:
        """控件是否可用"""
        self._refresh_if_needed()
        return self._control.IsEnabled

    @property
    def is_visible(self) -> bool:
        """控件是否可见"""
        self._refresh_if_needed()
        try:
            return not self._control.IsOffscreen
        except Exception:
            return False

    @property
    def bounding_rect(self) -> tuple:
        """控件边界 (left, top, right, bottom)"""
        self._refresh_if_needed()
        rect = self._control.BoundingRectangle
        return (rect.left, rect.top, rect.right, rect.bottom)

    @property
    def raw_control(self) -> uia.Control:
        """获取原始 uiautomation.Control（高级用法）"""
        return self._control

    # ------------------------------------------------------------------
    # 基础交互
    # ------------------------------------------------------------------

    def click(self, wait_after: float = 0.3) -> "DesktopElement":
        """
        点击控件。
        对应现有 JS: $(selector).click()
        """
        self._ensure_visible_and_enabled()
        try:
            self._control.Click()
            logger.debug(f"点击: {self.locator}")
            if wait_after:
                time.sleep(wait_after)
        except Exception as e:
            raise ElementInteractionError(f"点击失败 [{self.locator}]: {e}")
        return self

    def double_click(self, wait_after: float = 0.3) -> "DesktopElement":
        """双击控件"""
        self._ensure_visible_and_enabled()
        try:
            self._control.DoubleClick()
            logger.debug(f"双击: {self.locator}")
            if wait_after:
                time.sleep(wait_after)
        except Exception as e:
            raise ElementInteractionError(f"双击失败 [{self.locator}]: {e}")
        return self

    def right_click(self, wait_after: float = 0.3) -> "DesktopElement":
        """右键单击控件"""
        self._ensure_visible_and_enabled()
        try:
            self._control.RightClick()
            logger.debug(f"右键点击: {self.locator}")
            if wait_after:
                time.sleep(wait_after)
        except Exception as e:
            raise ElementInteractionError(f"右键点击失败 [{self.locator}]: {e}")
        return self

    def set_text(self, text: str, clear_first: bool = True) -> "DesktopElement":
        """
        设置控件文本。
        对应现有 JS: $(selector).val(text)

        :param text: 要设置的文本
        :param clear_first: 是否先清空已有文本（通过 Ctrl+A → Delete）
        """
        self._ensure_visible_and_enabled()
        try:
            if clear_first:
                # 使用 Ctrl+A 全选，再输入
                self._control.SendKeys("{Ctrl}A{Delete}")
                time.sleep(0.1)
            self._control.SendKeys(text)
            logger.debug(f"设置文本 [{self.locator}]: '{text[:50]}...'")
        except Exception as e:
            # 回退：使用 ValuePattern
            try:
                value_pattern = self._control.GetValuePattern()
                if value_pattern:
                    value_pattern.SetValue(text)
                    logger.debug(f"设置文本(ValuePattern) [{self.locator}]: '{text[:50]}...'")
                    return self
            except Exception:
                pass
            raise ElementInteractionError(f"设置文本失败 [{self.locator}]: {e}")
        return self

    def get_text(self) -> str:
        """
        获取控件文本。
        对应现有 JS: $(selector).text() / $(selector).val()
        """
        self._refresh_if_needed()
        # 优先用 ValuePattern（Edit 控件）
        try:
            value_pattern = self._control.GetValuePattern()
            if value_pattern:
                return value_pattern.Value or ""
        except Exception:
            pass
        # 回退到 Name 属性
        return self._control.Name or ""

    def send_keys(self, keys: str) -> "DesktopElement":
        """
        发送按键字符串。
        对应现有 JS 中的键盘模拟。

        :param keys: SendKeys 格式的按键字符串，如 "{Ctrl}A", "{Enter}", "hello"
        """
        self._ensure_visible_and_enabled()
        try:
            self._control.SendKeys(keys)
        except Exception as e:
            raise ElementInteractionError(f"发送按键失败 [{self.locator}]: {e}")
        return self

    def send_hotkey(self, *keys) -> "DesktopElement":
        """
        发送组合键。
        例: send_hotkey(Keys.CONTROL, Keys.A)  → Ctrl+A

        :param keys: uiautomation.Keys 常量
        """
        try:
            import uiautomation as _uia
            # 使用 uiautomation 的 SendKeys 发送组合键
            key_str = "".join(str(k) for k in keys)
            _uia.SendKeys(key_str)
            logger.debug(f"发送组合键: {key_str}")
        except Exception as e:
            raise ElementInteractionError(f"发送组合键失败 [{self.locator}]: {e}")
        return self

    def scroll_into_view(self) -> "DesktopElement":
        """滚动使控件可见"""
        try:
            self._control.ScrollIntoView()
        except Exception:
            pass
        return self

    # ------------------------------------------------------------------
    # 等待方法
    # ------------------------------------------------------------------

    def wait_until_enabled(self, timeout: float = 10.0) -> "DesktopElement":
        """等待控件变为可用"""
        deadline = time.time() + timeout
        while time.time() < deadline:
            if self.is_enabled:
                return self
            time.sleep(0.3)
        raise ElementInteractionError(
            f"等待控件可用超时 ({timeout}s): {self.locator}"
        )

    def wait_until_visible(self, timeout: float = 10.0) -> "DesktopElement":
        """等待控件变为可见"""
        deadline = time.time() + timeout
        while time.time() < deadline:
            if self.is_visible:
                return self
            time.sleep(0.3)
        raise ElementInteractionError(
            f"等待控件可见超时 ({timeout}s): {self.locator}"
        )

    # ------------------------------------------------------------------
    # 内部辅助
    # ------------------------------------------------------------------

    def _refresh_if_needed(self):
        """刷新控件引用"""
        try:
            # 尝试访问属性来检测引用是否过期
            _ = self._control.Name
        except Exception:
            # 引用已过期，只能让调用方重新查找
            pass

    def _ensure_visible_and_enabled(self):
        """确保控件可见且可用"""
        if not self.is_visible:
            raise ElementInteractionError(f"控件不可见: {self.locator}")
        if not self.is_enabled:
            raise ElementInteractionError(f"控件不可用: {self.locator}")

    def __repr__(self) -> str:
        return f"DesktopElement({self.locator}, name='{self.name}')"
