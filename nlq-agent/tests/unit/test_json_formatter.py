"""
JSONFormatter 与 setup_structured_logging 单元测试

测试覆盖：dev 模式文本输出、prod 模式 JSON 输出、correlation_id 注入。
"""

from __future__ import annotations

import json
import logging
import os
from unittest.mock import patch

from src.core.logging_config import (
    JSONFormatter,
    bind_correlation_id,
    get_correlation_id,
    setup_structured_logging,
)


class TestJSONFormatter:
    def test_dev_mode_text_output(self):
        """默认（非 JSON）模式下应输出人类可读的文本行。"""
        setup_structured_logging(level="INFO", json_output=False)
        logger = logging.getLogger("test_dev")

        # 使用 MemoryHandler 捕获输出较复杂，直接检查 formatter
        handler = logging.getLogger().handlers[0]
        assert isinstance(handler.formatter, logging.Formatter)
        assert handler.formatter._fmt == "%(asctime)s | %(levelname)-7s | %(name)s | %(message)s"

    def test_prod_mode_json_output(self):
        """json_output=True 时，日志应输出 JSON 行。"""
        setup_structured_logging(level="INFO", json_output=True)
        logger = logging.getLogger("test_json")

        handler = logging.getLogger().handlers[0]
        assert isinstance(handler.formatter, JSONFormatter)

        record = logger.makeRecord(
            name="test_json",
            level=logging.INFO,
            fn="",
            lno=0,
            msg="hello json",
            args=(),
            exc_info=None,
        )
        line = handler.formatter.format(record)
        payload = json.loads(line)
        assert payload["level"] == "INFO"
        assert payload["logger"] == "test_json"
        assert payload["msg"] == "hello json"
        assert "ts" in payload

    def test_json_with_correlation_id(self):
        """当 contextvar 中存在 correlation_id 时，JSON 应包含该字段。"""
        setup_structured_logging(level="INFO", json_output=True)
        logger = logging.getLogger("test_cid")

        bind_correlation_id("abc-123")
        try:
            record = logger.makeRecord(
                name="test_cid",
                level=logging.WARNING,
                fn="",
                lno=0,
                msg="warn message",
                args=(),
                exc_info=None,
            )
            handler = logging.getLogger().handlers[0]
            line = handler.formatter.format(record)
            payload = json.loads(line)
            assert payload["correlation_id"] == "abc-123"
            assert payload["level"] == "WARNING"
        finally:
            bind_correlation_id(None)

    def test_env_log_format_json_triggers_json(self):
        """环境变量 LOG_FORMAT=json 应自动启用 JSON 输出。"""
        with patch.dict(os.environ, {"LOG_FORMAT": "json"}):
            setup_structured_logging(level="INFO", json_output=None)
            handler = logging.getLogger().handlers[0]
            assert isinstance(handler.formatter, JSONFormatter)
