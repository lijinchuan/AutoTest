"""
Allure 报告辅助工具
将 uiautomation 截图和测试详情集成到 Allure 报告中。
"""
import io
import time
from typing import Optional

import allure
from PIL import Image

from autodesk.core.logger import get_logger

logger = get_logger(__name__)


class AllureReporter:
    """Allure 报告工具集"""

    @staticmethod
    def attach_screenshot(image: Image.Image, name: str = "screenshot"):
        """
        将 PIL Image 截图附加到当前 Allure 步骤。

        :param image: PIL Image 对象
        :param name: 附件名称
        """
        buf = io.BytesIO()
        image.save(buf, format="PNG")
        buf.seek(0)
        allure.attach(
            buf.getvalue(),
            name=name,
            attachment_type=allure.attachment_type.PNG,
        )

    @staticmethod
    def attach_bytes(data: bytes, name: str, attachment_type=None):
        """
        附加二进制数据到报告。

        :param data: 二进制数据
        :param name: 附件名称
        :param attachment_type: allure 附件类型
        """
        if attachment_type is None:
            attachment_type = allure.attachment_type.TEXT
        allure.attach(data, name=name, attachment_type=attachment_type)

    @staticmethod
    def attach_text(text: str, name: str = "detail"):
        """
        附加文本到报告。

        :param text: 文本内容
        :param name: 附件名称
        """
        allure.attach(
            text,
            name=name,
            attachment_type=allure.attachment_type.TEXT,
        )

    @staticmethod
    def attach_json(data: dict, name: str = "data"):
        """附加 JSON 数据到报告"""
        import json
        AllureReporter.attach_text(
            json.dumps(data, ensure_ascii=False, indent=2),
            name=name,
        )

    @staticmethod
    def step(title: str):
        """
        装饰器/上下文：在 Allure 报告中生成一个步骤。

        用法1 (装饰器):
            @AllureReporter.step("登录应用")
            def login(self): ...

        用法2 (上下文管理):
            with allure.step("正在查找元素"):
                element = ...
        """
        return allure.step(title)

    @staticmethod
    def attach_element_state(element, include_screenshot: bool = True):
        """
        附加元素状态详情到报告。
        包含：名称、矩形区域、可用状态、可见状态。

        :param element: DesktopElement 实例
        :param include_screenshot: 是否包含元素截图
        """
        info_lines = [
            f"Locator: {element.locator}",
            f"Name: {element.name}",
            f"Enabled: {element.is_enabled}",
            f"Visible: {element.is_visible}",
            f"BoundingRect: {element.bounding_rect}",
        ]
        AllureReporter.attach_text("\n".join(info_lines), name="element_info")
