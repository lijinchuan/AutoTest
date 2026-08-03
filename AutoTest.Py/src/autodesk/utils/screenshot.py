"""
截图工具
"""
import io
import time
from pathlib import Path
from typing import Optional

from PIL import Image

from autodesk.core.config import ConfigLoader
from autodesk.core.logger import get_logger

logger = get_logger(__name__)


def capture_screenshot(name: str = "", save_dir: str = None) -> Optional[str]:
    """
    截取全屏并保存。

    :param name: 截图名称
    :param save_dir: 保存目录，默认使用配置
    :return: 截图文件路径
    """
    config = ConfigLoader.get_config()

    if save_dir is None:
        save_dir = config.framework.screenshot_dir

    ss_dir = Path(save_dir)
    ss_dir.mkdir(parents=True, exist_ok=True)

    if not name:
        name = f"screenshot_{int(time.time() * 1000)}"

    filepath = ss_dir / f"{name}.png"

    try:
        from PIL import ImageGrab
        img = ImageGrab.grab()
        img.save(str(filepath))
        logger.info(f"画面已保存: {filepath}")
        return str(filepath)
    except Exception as e:
        logger.warning(f"画面失败: {e}")
        return None


def screenshot_to_bytes(image: Image.Image) -> bytes:
    """将 PIL Image 转换为 PNG 字节"""
    buf = io.BytesIO()
    image.save(buf, format="PNG")
    buf.seek(0)
    return buf.getvalue()
