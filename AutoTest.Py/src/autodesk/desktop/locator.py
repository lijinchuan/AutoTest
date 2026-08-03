"""
元素定位策略
对应现有 C# 项目中 WebTask 通过 jQuery 选择器查找元素的模式，
但适配为 Windows UI Automation 的定位方式。
"""
from dataclasses import dataclass, field
from typing import Literal

# 定位策略类型
LocatorStrategy = Literal[
    "name",            # 按控件 Name 属性
    "automation_id",   # 按 AutomationId
    "class_name",      # 按窗口类名
    "control_type",    # 按控件类型（Button/Edit/ComboBox等）
    "regex",           # 按名称正则匹配
]


@dataclass(frozen=True)
class Locator:
    """
    元素定位器，描述如何找到一个 UI 元素。

    使用示例:
        Locator.by_name("确定")
        Locator.by_automation_id("btnOK")
        Locator.by_class_name("Edit")
        Locator.by_control_type("ButtonControl")
    """
    strategy: LocatorStrategy
    value: str
    index: int = 0  # 匹配多个时取第 N 个（0-based）

    def __repr__(self) -> str:
        return f"Locator({self.strategy}='{self.value}', index={self.index})"

    # ---- 工厂方法 ----

    @staticmethod
    def by_name(name: str, index: int = 0) -> "Locator":
        """按控件名称定位"""
        return Locator(strategy="name", value=name, index=index)

    @staticmethod
    def by_automation_id(aid: str, index: int = 0) -> "Locator":
        """按 AutomationId 定位"""
        return Locator(strategy="automation_id", value=aid, index=index)

    @staticmethod
    def by_class_name(class_name: str, index: int = 0) -> "Locator":
        """按窗口类名定位"""
        return Locator(strategy="class_name", value=class_name, index=index)

    @staticmethod
    def by_control_type(control_type: str, index: int = 0) -> "Locator":
        """按控件类型名称定位，如 'ButtonControl', 'EditControl', 'ComboBoxControl'"""
        return Locator(strategy="control_type", value=control_type, index=index)

    @staticmethod
    def by_regex(pattern: str, index: int = 0) -> "Locator":
        """按名称正则匹配定位"""
        return Locator(strategy="regex", value=pattern, index=index)
