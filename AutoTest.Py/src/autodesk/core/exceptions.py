"""
核心异常类定义
对应现有 C# 项目中的 AutoTest.Domain.Exceptions
"""
from pathlib import Path


class AutoDeskError(Exception):
    """框架基础异常"""
    pass


class ConfigError(AutoDeskError):
    """配置相关异常"""
    pass


class AppLaunchError(AutoDeskError):
    """应用启动失败异常"""
    pass


class AppNotRespondingError(AutoDeskError):
    """应用无响应异常"""
    pass


class ElementNotFoundError(AutoDeskError):
    """元素未找到异常"""

    def __init__(self, locator, timeout: float = None):
        self.locator = locator
        self.timeout = timeout
        msg = f"Element not found: {locator}"
        if timeout is not None:
            msg += f" (timeout: {timeout}s)"
        super().__init__(msg)


class TimeoutError(AutoDeskError):
    """超时异常"""
    pass


class ElementInteractionError(AutoDeskError):
    """元素交互异常（如点击被遮挡、元素不可用等）"""
    pass


class ValidationError(AutoDeskError):
    """测试验证失败异常"""
    pass


class ScriptError(AutoDeskError):
    """Python 测试脚本执行异常（对应现有 JSException）"""
    pass
