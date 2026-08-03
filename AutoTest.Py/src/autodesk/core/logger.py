"""
日志模块
对应现有 C# 项目中的 NLogger / LogHelper
"""
import logging
import logging.config
from pathlib import Path


def get_logger(name: str) -> logging.Logger:
    """获取预配置的 logger"""
    return logging.getLogger(name)


def setup_logging(log_file: str = "results/autodesk.log",
                  level: str = "INFO",
                  max_bytes: int = 10 * 1024 * 1024,
                  backup_count: int = 5):
    """初始化日志配置"""
    log_path = Path(log_file)
    log_path.parent.mkdir(parents=True, exist_ok=True)

    config = {
        "version": 1,
        "disable_existing_loggers": False,
        "formatters": {
            "standard": {
                "format": "%(asctime)s [%(levelname)s] %(name)s: %(message)s",
                "datefmt": "%Y-%m-%d %H:%M:%S",
            },
            "simple": {
                "format": "[%(levelname)s] %(message)s",
            },
        },
        "handlers": {
            "console": {
                "class": "logging.StreamHandler",
                "level": "DEBUG",
                "formatter": "simple",
                "stream": "ext://sys.stdout",
            },
            "file": {
                "class": "logging.handlers.RotatingFileHandler",
                "level": "DEBUG",
                "formatter": "standard",
                "filename": str(log_path),
                "maxBytes": max_bytes,
                "backupCount": backup_count,
                "encoding": "utf-8",
            },
        },
        "root": {
            "level": level,
            "handlers": ["console", "file"],
        },
    }

    logging.config.dictConfig(config)
    return logging.getLogger("autodesk")
