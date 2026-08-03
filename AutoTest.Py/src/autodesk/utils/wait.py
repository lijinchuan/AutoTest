"""
等待工具集
对应现有 C# 中的超时重试逻辑：

WebTask 中的重试模式：
  while(tryCount++ < maxCount) { ... await Task.Delay(sleepMills); }

本模块提供 Python 风格的重试和等待工具。
"""
import time
import functools
from typing import Callable, Any, TypeVar

from autodesk.core.exceptions import TimeoutError

T = TypeVar("T")


def wait_until(
    condition: Callable[[], bool],
    timeout: float = 10.0,
    poll_interval: float = 0.5,
    error_msg: str = "条件未满足",
) -> None:
    """
    阻塞等待直到条件成立。

    :param condition: 返回 bool 的可调用对象
    :param timeout: 超时秒数
    :param poll_interval: 轮询间隔秒数
    :param error_msg: 超时时的错误消息
    :raises TimeoutError: 超时
    """
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            if condition():
                return
        except Exception:
            pass
        time.sleep(poll_interval)
    raise TimeoutError(f"{error_msg} (timeout={timeout}s)")


def wait_until_returns(
    func: Callable[[], T],
    timeout: float = 10.0,
    poll_interval: float = 0.5,
    error_msg: str = "操作超时",
) -> T:
    """
    阻塞等待直到函数返回非 None 值。

    :param func: 返回 T 的可调用对象
    :param timeout: 超时秒数
    :param poll_interval: 轮询间隔
    :param error_msg: 超时错误消息
    :return: func 的返回值
    :raises TimeoutError: 超时
    """
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            result = func()
            if result is not None:
                return result
        except Exception:
            pass
        time.sleep(poll_interval)
    raise TimeoutError(f"{error_msg} (timeout={timeout}s)")


def retry_on_failure(
    max_attempts: int = 3,
    delay: float = 1.0,
    exceptions: tuple = (Exception,),
):
    """
    装饰器：失败时自动重试。

    对应现有 RunTestCode 中的重试逻辑：
        while(tryCount++ < maxCount) { try { ... } catch { await Task.Delay(sleepMills); } }

    使用方式:
        @retry_on_failure(max_attempts=3, delay=1.0)
        def flaky_operation():
            ...
    """
    def decorator(func: Callable):
        @functools.wraps(func)
        def wrapper(*args, **kwargs):
            last_exc = None
            for attempt in range(1, max_attempts + 1):
                try:
                    return func(*args, **kwargs)
                except exceptions as e:
                    last_exc = e
                    if attempt < max_attempts:
                        time.sleep(delay)
            raise last_exc  # type: ignore
        return wrapper
    return decorator


class Wait:
    """
    流式等待 API，类似 Selenium 的 WebDriverWait。

    使用方式:
        wait = Wait(app, timeout=10)
        element = wait.until(lambda: page._find_element(locator))
    """

    def __init__(self, timeout: float = 10.0, poll_interval: float = 0.5):
        self._timeout = timeout
        self._poll_interval = poll_interval

    def until(self, condition: Callable[[], Any], error_msg: str = "等待条件超时") -> Any:
        """等待条件成立并返回结果"""
        return wait_until_returns(
            condition,
            timeout=self._timeout,
            poll_interval=self._poll_interval,
            error_msg=error_msg,
        )

    def until_not(self, condition: Callable[[], bool], error_msg: str = "等待条件消失超时") -> None:
        """等待条件不成立"""
        wait_until(
            lambda: not condition(),
            timeout=self._timeout,
            poll_interval=self._poll_interval,
            error_msg=error_msg,
        )
