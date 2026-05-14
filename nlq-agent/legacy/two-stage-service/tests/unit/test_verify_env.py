"""
Unit tests for scripts/verify_env.py

These tests mock filesystem and network so they can run in CI
without actual services.
"""

from __future__ import annotations

import sys
from pathlib import Path
from unittest.mock import AsyncMock, patch

import pytest

from scripts.verify_env import EnvChecker


class TestVerifyEnv:
    def test_all_vars_present_no_network_returns_0(self, tmp_path: Path):
        """所有变量齐全、非 placeholder、跳过网络 → 退出码 0。"""
        env_content = """
LLM_BASE_URL=http://localhost:8000/v1
LLM_API_KEY=sk-real-key-123
LLM_MODEL=Qwen/Qwen2.5-72B-Instruct-AWQ
EMBEDDING_BASE_URL=http://localhost:8001
QDRANT_HOST=localhost
QDRANT_PORT=6333
MYSQL_HOST=localhost
MYSQL_PORT=3306
MYSQL_USER=nlq_readonly
MYSQL_PASSWORD=secret
MYSQL_DATABASE=poxiao_lab
"""
        env_file = tmp_path / ".env"
        env_file.write_text(env_content, encoding="utf-8")

        checker = EnvChecker(env_file=str(env_file), no_network=True)
        assert checker.run() == 0

    def test_missing_llm_api_key_returns_1(self, tmp_path: Path):
        """缺少 LLM_API_KEY → 退出码 1。"""
        env_content = """
LLM_BASE_URL=http://localhost:8000/v1
LLM_MODEL=Qwen/Qwen2.5-72B-Instruct-AWQ
EMBEDDING_BASE_URL=http://localhost:8001
QDRANT_HOST=localhost
QDRANT_PORT=6333
MYSQL_HOST=localhost
MYSQL_PORT=3306
MYSQL_USER=nlq_readonly
MYSQL_PASSWORD=secret
MYSQL_DATABASE=poxiao_lab
"""
        env_file = tmp_path / ".env"
        env_file.write_text(env_content, encoding="utf-8")

        checker = EnvChecker(env_file=str(env_file), no_network=True)
        assert checker.run() == 1

    @pytest.mark.parametrize("bad_key", ["YOUR_API_KEY_HERE", "sk-test-dummy", ""])
    def test_placeholder_key_returns_1(self, tmp_path: Path, bad_key: str):
        """placeholder 形式的 LLM_API_KEY → 退出码 1。"""
        env_content = f"""
LLM_BASE_URL=http://localhost:8000/v1
LLM_API_KEY={bad_key}
LLM_MODEL=Qwen/Qwen2.5-72B-Instruct-AWQ
EMBEDDING_BASE_URL=http://localhost:8001
QDRANT_HOST=localhost
QDRANT_PORT=6333
MYSQL_HOST=localhost
MYSQL_PORT=3306
MYSQL_USER=nlq_readonly
MYSQL_PASSWORD=secret
MYSQL_DATABASE=poxiao_lab
"""
        env_file = tmp_path / ".env"
        env_file.write_text(env_content, encoding="utf-8")

        checker = EnvChecker(env_file=str(env_file), no_network=True)
        assert checker.run() == 1

    def test_network_failures_with_no_network_returns_0(self, tmp_path: Path):
        """网络全失败但传入 --no-network → 跳过网络检查，退出码 0。"""
        env_content = """
LLM_BASE_URL=http://unreachable:8000/v1
LLM_API_KEY=sk-real-key-123
LLM_MODEL=Qwen/Qwen2.5-72B-Instruct-AWQ
EMBEDDING_BASE_URL=http://unreachable:8001
QDRANT_HOST=unreachable
QDRANT_PORT=6333
MYSQL_HOST=unreachable
MYSQL_PORT=3306
MYSQL_USER=nlq_readonly
MYSQL_PASSWORD=secret
MYSQL_DATABASE=poxiao_lab
"""
        env_file = tmp_path / ".env"
        env_file.write_text(env_content, encoding="utf-8")

        # 即使底层网络函数被调用也应被 no_network 跳过
        checker = EnvChecker(env_file=str(env_file), no_network=True)
        assert checker.run() == 0

    def test_network_failures_without_flag_returns_2(self, tmp_path: Path):
        """网络不可达且不传 --no-network → 退出码 2（仅 warning）。"""
        env_content = """
LLM_BASE_URL=http://unreachable:8000/v1
LLM_API_KEY=sk-real-key-123
LLM_MODEL=Qwen/Qwen2.5-72B-Instruct-AWQ
EMBEDDING_BASE_URL=http://unreachable:8001
QDRANT_HOST=unreachable
QDRANT_PORT=6333
MYSQL_HOST=unreachable
MYSQL_PORT=3306
MYSQL_USER=nlq_readonly
MYSQL_PASSWORD=secret
MYSQL_DATABASE=poxiao_lab
"""
        env_file = tmp_path / ".env"
        env_file.write_text(env_content, encoding="utf-8")

        checker = EnvChecker(env_file=str(env_file), no_network=False)
        # mock 所有网络检查返回 False → 产生 warnings
        with patch.object(checker, "_http_check", new_callable=AsyncMock, return_value=False):
            with patch.object(checker, "_tcp_check", new_callable=AsyncMock, return_value=False):
                assert checker.run() == 2

    def test_missing_env_file_returns_1(self):
        """不存在的 env 文件 → 退出码 1。"""
        checker = EnvChecker(env_file="/nonexistent/.env", no_network=True)
        assert checker.run() == 1
