"""
桌面应用生命周期管理
对应现有 C# 项目中 WebTask 的生命周期管理模式：
  ready → execute → complete
但适配为桌面应用：launch/attach → execute → close/kill

同时对应 Guard 的进程监控模式（FindWindow, Kill, restart）
"""
import subprocess
import time
from pathlib import Path
from typing import Optional

import uiautomation as uia
from PIL import Image

from autodesk.core.config import AppEntry
from autodesk.core.exceptions import AppLaunchError, AppNotRespondingError
from autodesk.core.logger import get_logger

logger = get_logger(__name__)


class Application:
    """
    管理被测桌面应用的生命周期。

    支持两种模式：
    1. launch()  - 启动应用并等待主窗口就绪
    2. attach()  - 附加到已运行的窗口

    使用示例:
        config = ConfigLoader.get_app("notepad")
        app = Application(config)
        app.launch()
        app.wait_ready()
        # ... 执行测试 ...
        app.close()
    """

    def __init__(self, app_config: AppEntry = None):
        self.config: AppEntry = app_config
        self._process: Optional[subprocess.Popen] = None
        self._window: Optional[uia.WindowControl] = None
        self._is_ready: bool = False

    @classmethod
    def from_exe(cls, executable: str, window_title: str = "", window_class: str = "",
                 args: list = None, startup_timeout: int = 30) -> "Application":
        """
        便捷方法：直接指定 exe 路径和窗口标题，不需要配置文件。

        :param executable: 可执行文件路径或名称（如 "notepad.exe"）
        :param window_title: 主窗口标题（部分匹配）
        :param window_class: 窗口类名（可选）
        :param args: 启动参数
        :param startup_timeout: 启动超时秒数
        """
        from autodesk.core.config import AppEntry
        config = AppEntry(
            name=executable,
            executable=executable,
            window_title=window_title,
            window_class=window_class,
            args=args or [],
            startup_timeout=startup_timeout,
        )
        return cls(config)

    # ------------------------------------------------------------------
    # 生命周期方法
    # ------------------------------------------------------------------

    def launch(self) -> "Application":
        """
        启动应用并等待主窗口出现。
        对应现有 TaskBiz 构建 TestTask 后的执行环节。
        """
        logger.info(f"正在启动应用: {self.config.name} ({self.config.executable})")

        cmd = [self.config.executable]
        if self.config.args:
            cmd.extend(self.config.args)

        kwargs = {}
        if self.config.working_dir:
            kwargs["cwd"] = self.config.working_dir

        try:
            self._process = subprocess.Popen(
                cmd,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                **kwargs
            )
        except FileNotFoundError:
            raise AppLaunchError(
                f"找不到可执行文件: {self.config.executable}\n"
                f"请确认路径正确或该程序已在系统 PATH 中。"
            )
        except Exception as e:
            raise AppLaunchError(f"启动应用失败: {e}")

        # 等待窗口出现
        try:
            self.wait_ready(timeout=self.config.startup_timeout)
        except AppLaunchError:
            # 窗口未出现，尝试清理
            self.kill()
            raise

        return self

    def attach(self, title: str = "", class_name: str = "") -> "Application":
        """
        附加到已运行的窗口。
        对应现有 Guard 的 FindWindow 模式。

        :param title: 窗口标题（支持部分匹配）
        :param class_name: 窗口类名
        """
        search_title = title or self.config.window_title
        search_class = class_name or self.config.window_class

        logger.info(f"正在附加到窗口: title='{search_title}', class='{search_class}'")

        if search_title:
            self._window = uia.WindowControl(searchDepth=1, Name=search_title)
        elif search_class:
            self._window = uia.WindowControl(searchDepth=1, ClassName=search_class)
        else:
            # 用 Name 属性模糊匹配
            self._window = uia.WindowControl(
                searchDepth=1,
                Name=self.config.window_title
            )

        if not self._window.Exists(0, 0.1):
            raise AppLaunchError(f"未找到匹配的窗口: title='{search_title}'")

        self._is_ready = True
        logger.info(f"已附加到窗口: {self._window.Name}")
        return self

    def wait_ready(self, timeout: int = None) -> None:
        """
        等待应用窗口就绪并可交互。
        """
        if timeout is None:
            timeout = self.config.startup_timeout

        title = self.config.window_title
        class_name = self.config.window_class

        deadline = time.time() + timeout

        while time.time() < deadline:
            # 用 class_name 或 title 在顶层窗口搜索（searchDepth=1 只搜顶层）
            if class_name:
                self._window = uia.WindowControl(searchDepth=1, ClassName=class_name)
            elif title:
                self._window = uia.WindowControl(searchDepth=1, Name=title)
            else:
                time.sleep(0.5)
                continue

            if self._window.Exists(0, 0.5):
                # 获取 NativeWindowHandle 重建引用，避免 COM stale 问题
                try:
                    hwnd = self._window.NativeWindowHandle
                    self._window = uia.WindowControl(searchDepth=1, ClassName=class_name)
                    self._window.Exists(0, 0.3)
                except Exception:
                    pass

                self._is_ready = True
                logger.info(f"应用窗口已就绪: {self._window.Name}")
                return

            time.sleep(0.5)

        raise AppLaunchError(
            f"等待窗口超时 ({timeout}s): title='{title}', class='{class_name}'"
        )

    def close(self) -> None:
        """
        正常关闭应用窗口。
        """
        logger.info(f"正在关闭应用: {self.config.name}")
        try:
            if self._window and self._window.Exists(0, 0.1):
                self._window.Close()
                time.sleep(0.5)
        except Exception as e:
            logger.warning(f"关闭窗口异常: {e}")

        # 如果仍未退出，强制杀进程
        if self.is_running():
            self.kill()

        self._is_ready = False
        self._window = None

    def kill(self) -> None:
        """
        强制终止进程。对应 Guard.Kill() 模式。
        """
        if self._process is None:
            return
        try:
            logger.info(f"正在强制终止进程 (PID={self._process.pid})")
            self._process.kill()
            self._process.wait(timeout=5)
        except Exception as e:
            logger.warning(f"终止进程异常: {e}")
        finally:
            self._process = None
            self._is_ready = False

    def restart_if_unresponsive(self, timeout: int = 3) -> bool:
        """
        检查应用是否响应，不响应则重启。
        对应 Guard 看门狗模式。
        返回 True 表示已重启。
        """
        if not self.is_running():
            logger.warning(f"应用 {self.config.name} 已退出，正在重启...")
            self.launch()
            return True
        return False

    # ------------------------------------------------------------------
    # 状态查询
    # ------------------------------------------------------------------

    def is_running(self) -> bool:
        """检查进程是否存活"""
        if self._process is None:
            return False
        return self._process.poll() is None

    @property
    def is_ready(self) -> bool:
        return self._is_ready

    def get_main_window(self) -> uia.WindowControl:
        """
        获取主窗口控件，必要时刷新引用。
        """
        if self._window is None or not self._window.Exists(0, 0.1):
            self._window = uia.WindowControl(
                searchDepth=1,
                Name=self.config.window_title
            )
        return self._window

    def refresh(self) -> None:
        """
        刷新窗口引用（对话框切换后很有用）。
        """
        self._window = None

    # ------------------------------------------------------------------
    # 截图
    # ------------------------------------------------------------------

    def screenshot(self) -> Image.Image:
        """
        截取应用窗口的截图。
        对应现有的屏幕捕获功能。
        """
        try:
            import io
            win = self.get_main_window()
            if win.Exists(0, 0.1):
                # 使用 uiautomation 内置截图
                bitmap = win.ToBitmap()
                buf = io.BytesIO()
                bitmap.Save(buf, "png")
                buf.seek(0)
                return Image.open(buf)
        except Exception as e:
            logger.warning(f"截图失败: {e}")

        # 回退：全屏截图
        from PIL import ImageGrab
        return ImageGrab.grab()
