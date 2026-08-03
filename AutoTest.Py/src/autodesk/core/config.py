"""
配置管理模块
对应现有 C# 项目中的 RuntimeConfig + App.config + configfile.json
使用 Pydantic 做类型安全验证，YAML 作为配置格式
"""
from pathlib import Path
from typing import Optional
import yaml

from pydantic import BaseModel, Field


# ---------------------------------------------------------------------------
# 配置模型
# ---------------------------------------------------------------------------

class AppEntry(BaseModel):
    """单个被测桌面应用的定义（对应现有 TestSource 概念）"""
    name: str                                    # 应用名称
    executable: str                              # 可执行文件路径或进程名
    args: list[str] = []                         # 启动参数
    working_dir: str = ""                        # 工作目录
    window_title: str = ""                       # 主窗口标题（用于查找窗口）
    window_class: str = ""                       # Win32 窗口类名
    startup_timeout: int = 30                    # 启动超时（秒）


class FrameworkConfig(BaseModel):
    """框架全局配置（对应 App.config 中的 appSettings）"""
    default_timeout: float = 10.0                # 默认超时（秒）
    implicit_wait: float = 0.5                   # 隐式等待间隔（秒）
    screenshot_on_failure: bool = True           # 失败时自动截图
    screenshot_dir: str = "results/screenshots"  # 截图保存目录
    allure_dir: str = "results/allure"           # Allure 结果目录
    max_retries: int = 2                         # 最大重试次数
    poll_interval: float = 0.5                   # 轮询间隔


class LoggingConfig(BaseModel):
    """日志配置"""
    level: str = "INFO"
    file: str = "results/autodesk.log"
    max_bytes: int = 10 * 1024 * 1024
    backup_count: int = 5


class AppConfig(BaseModel):
    """应用全局配置"""
    apps: dict[str, AppEntry] = {}               # 被测应用注册表
    framework: FrameworkConfig = Field(default_factory=FrameworkConfig)
    logging: LoggingConfig = Field(default_factory=LoggingConfig)


class EnvEntry(BaseModel):
    """环境变量条目（对应 TestEnv + TestEnvParam）"""
    name: str
    description: str = ""
    variables: dict[str, str] = {}


# ---------------------------------------------------------------------------
# 配置加载器（单例模式）
# ---------------------------------------------------------------------------

class ConfigLoader:
    """配置加载器，对应 RuntimeConfig 的概念"""
    _instance: Optional["ConfigLoader"] = None
    _settings: Optional[AppConfig] = None
    _envs: dict[str, EnvEntry] = {}
    _current_env: Optional[EnvEntry] = None

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance

    @classmethod
    def load(cls,
             settings_path: str = "config/settings.yaml",
             envs_path: str = "config/envs.yaml") -> AppConfig:
        """加载配置"""
        # 加载主配置
        settings_file = Path(settings_path)
        if settings_file.exists():
            with open(settings_file, "r", encoding="utf-8") as f:
                data = yaml.safe_load(f) or {}
            cls._settings = AppConfig(**data)
        else:
            cls._settings = AppConfig()

        # 加载环境配置
        envs_file = Path(envs_path)
        if envs_file.exists():
            with open(envs_file, "r", encoding="utf-8") as f:
                data = yaml.safe_load(f) or {}
            cls._envs = {k: EnvEntry(**v) for k, v in data.items()}

        # 默认选择 "default" 环境
        cls._current_env = cls._envs.get("default")
        return cls._settings

    @classmethod
    def get_config(cls) -> AppConfig:
        """获取全局配置"""
        if cls._settings is None:
            cls.load()
        return cls._settings

    @classmethod
    def get_env(cls, name: str = None) -> Optional[EnvEntry]:
        """获取环境配置"""
        if cls._settings is None:
            cls.load()
        if name:
            return cls._envs.get(name)
        return cls._current_env

    @classmethod
    def get_app(cls, name: str) -> AppEntry:
        """获取指定应用的配置"""
        config = cls.get_config()
        if name not in config.apps:
            raise KeyError(f"应用 '{name}' 未在配置中定义。可用: {list(config.apps.keys())}")
        return config.apps[name]

    @classmethod
    def reset(cls):
        """重置配置（测试用）"""
        cls._instance = None
        cls._settings = None
        cls._envs = {}
        cls._current_env = None
