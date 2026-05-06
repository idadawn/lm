"""日志模块.

提供统一的日志配置。
"""

import logging
import sys


def setup_logger(name: str = "nlq-agent", level: int = logging.INFO) -> logging.Logger:
    """设置日志记录器.

    Args:
        name: 日志名称
        level: 日志级别

    Returns:
        配置好的日志记录器
    """
    logger = logging.getLogger(name)
    logger.setLevel(level)

    # 避免重复添加处理器
    if not logger.handlers:
        handler = logging.StreamHandler(sys.stdout)
        handler.setLevel(level)

        formatter = logging.Formatter("%(asctime)s - %(name)s - %(levelname)s - %(message)s")
        handler.setFormatter(formatter)
        logger.addHandler(handler)

    return logger


# 全局日志实例
logger = setup_logger()
