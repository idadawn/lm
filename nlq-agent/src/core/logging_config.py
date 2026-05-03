"""
结构化日志配置

基于 stdlib logging 实现，无需引入第三方包：
- JSONFormatter: 生产环境输出机器可读的 JSON
- 文本 Formatter: 开发环境输出人类可读的格式
- correlation_id 通过 contextvars 在异步请求树中传递
"""

from __future__ import annotations

import json
import logging
import os
import sys
from contextvars import ContextVar
from datetime import datetime, timezone
from typing import Any

# 当前请求的 correlation_id（async-safe）
_correlation_id: ContextVar[str | None] = ContextVar("correlation_id", default=None)


def bind_correlation_id(cid: str | None) -> None:
    """绑定 correlation_id 到当前执行上下文。"""
    _correlation_id.set(cid)


def get_correlation_id() -> str | None:
    """获取当前执行上下文的 correlation_id。"""
    return _correlation_id.get()


class JSONFormatter(logging.Formatter):
    """将日志记录序列化为 JSON 行。

    输出字段：
        ts        - ISO8601 时间戳
        level     - 日志级别（大写）
        logger    - logger 名称
        msg       - 格式化后的消息
        correlation_id - 当前上下文中的追踪 ID（如存在）
        <extra>   - 通过 extra={...} 传入的自定义字段
    """

    def format(self, record: logging.LogRecord) -> str:
        payload: dict[str, Any] = {
            "ts": datetime.fromtimestamp(record.created, tz=timezone.utc).isoformat(),
            "level": record.levelname,
            "logger": record.name,
            "msg": record.getMessage(),
        }

        cid = get_correlation_id()
        if cid is not None:
            payload["correlation_id"] = cid

        # 将 LogRecord 中通过 extra={...} 传入的自定义字段加入 JSON
        # 跳过标准内置属性
        _skip = frozenset(
            {
                "name",
                "msg",
                "args",
                "levelname",
                "levelno",
                "pathname",
                "filename",
                "module",
                "exc_info",
                "exc_text",
                "stack_info",
                "lineno",
                "funcName",
                "created",
                "msecs",
                "relativeCreated",
                "thread",
                "threadName",
                "processName",
                "process",
                "asctime",
                "message",
                "taskName",
            }
        )
        for key, value in record.__dict__.items():
            if key not in _skip and not key.startswith("_"):
                payload[key] = value

        if record.exc_info:
            payload["exc_info"] = self.formatException(record.exc_info)

        return json.dumps(payload, ensure_ascii=False, default=str)


def setup_structured_logging(
    level: str | int = "INFO",
    json_output: bool | None = None,
) -> None:
    """配置全局日志。

    Args:
        level: 日志级别，字符串或 logging 常量。
        json_output: 是否输出 JSON；None 时自动根据环境变量 LOG_FORMAT=json 判定。
    """
    if isinstance(level, str):
        level = getattr(logging, level.upper(), logging.INFO)

    if json_output is None:
        json_output = os.getenv("LOG_FORMAT", "").lower().strip() == "json"

    if json_output:
        formatter: logging.Formatter = JSONFormatter()
    else:
        formatter = logging.Formatter(
            fmt="%(asctime)s | %(levelname)-7s | %(name)s | %(message)s",
            datefmt="%Y-%m-%d %H:%M:%S",
        )

    handler = logging.StreamHandler(sys.stdout)
    handler.setFormatter(formatter)

    root_logger = logging.getLogger()
    root_logger.setLevel(level)

    # 清除已有 handler，防止重复添加
    for h in list(root_logger.handlers):
        root_logger.removeHandler(h)
    root_logger.addHandler(handler)
