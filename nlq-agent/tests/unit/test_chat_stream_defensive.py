"""
LLMClient.chat_stream 防御性测试

验证流式调用在遇到空 choices、空 delta、空 content 等异常 chunk 时
不会抛出 IndexError 且能完整消费整个 stream。
"""

from __future__ import annotations

import pytest
from unittest.mock import AsyncMock, MagicMock, patch


class TestChatStreamDefensive:
    """测试 chat_stream 对异常 chunk 的防御处理。"""

    @pytest.fixture(autouse=True)
    def patch_settings(self):
        """Mock settings so LLMClient can instantiate without env vars."""
        with patch(
            "src.services.llm_client.get_settings",
            return_value=MagicMock(
                llm_base_url="http://mock",
                llm_api_key="mock-key",
                llm_model="mock-model",
                llm_temperature=0.7,
                llm_max_tokens=512,
            ),
        ):
            yield

    def _make_chunk(self, choices):
        """Helper: 构造一个 mock chunk 对象。"""
        chunk = MagicMock()
        chunk.choices = choices
        return chunk

    def _make_choice(self, delta_content):
        """Helper: 构造一个 mock choice（含 delta）。"""
        choice = MagicMock()
        if delta_content is not None:
            choice.delta = MagicMock()
            choice.delta.content = delta_content
        else:
            choice.delta = None
        return choice

    async def _collect_stream(self, stream):
        """Helper: 把 AsyncIterator 的内容收集成 list。"""
        return [item async for item in stream]

    @pytest.mark.asyncio
    async def test_empty_choices_heartbeat_skipped(self):
        """空 choices 的心跳 chunk 应被跳过，不抛 IndexError。"""
        from src.services.llm_client import LLMClient

        client = LLMClient()
        mock_stream = AsyncMock()
        mock_stream.__aiter__.return_value = [
            self._make_chunk([]),
            self._make_chunk([self._make_choice("hello")]),
        ]
        client._client.chat.completions.create = AsyncMock(return_value=mock_stream)

        result = await self._collect_stream(
            client.chat_stream([{"role": "user", "content": "hi"}])
        )

        assert result == ["hello"]

    @pytest.mark.asyncio
    async def test_delta_none_skipped(self):
        """delta 为 None 的 chunk 应被跳过。"""
        from src.services.llm_client import LLMClient

        client = LLMClient()
        mock_stream = AsyncMock()
        mock_stream.__aiter__.return_value = [
            self._make_chunk([self._make_choice(None)]),
            self._make_chunk([self._make_choice("world")]),
        ]
        client._client.chat.completions.create = AsyncMock(return_value=mock_stream)

        result = await self._collect_stream(
            client.chat_stream([{"role": "user", "content": "hi"}])
        )

        assert result == ["world"]

    @pytest.mark.asyncio
    async def test_content_none_skipped(self):
        """delta.content 为 None 的 chunk 应被跳过。"""
        from src.services.llm_client import LLMClient

        client = LLMClient()

        choice_with_none = MagicMock()
        choice_with_none.delta = MagicMock()
        choice_with_none.delta.content = None

        mock_stream = AsyncMock()
        mock_stream.__aiter__.return_value = [
            self._make_chunk([choice_with_none]),
            self._make_chunk([self._make_choice("real")]),
        ]
        client._client.chat.completions.create = AsyncMock(return_value=mock_stream)

        result = await self._collect_stream(
            client.chat_stream([{"role": "user", "content": "hi"}])
        )

        assert result == ["real"]

    @pytest.mark.asyncio
    async def test_content_empty_string_skipped(self):
        """delta.content 为空字符串的 chunk 应被跳过。"""
        from src.services.llm_client import LLMClient

        client = LLMClient()

        choice_empty = MagicMock()
        choice_empty.delta = MagicMock()
        choice_empty.delta.content = ""

        mock_stream = AsyncMock()
        mock_stream.__aiter__.return_value = [
            self._make_chunk([choice_empty]),
            self._make_chunk([self._make_choice("data")]),
        ]
        client._client.chat.completions.create = AsyncMock(return_value=mock_stream)

        result = await self._collect_stream(
            client.chat_stream([{"role": "user", "content": "hi"}])
        )

        assert result == ["data"]

    @pytest.mark.asyncio
    async def test_mixed_chunks_only_yield_valid_content(self):
        """混合多种异常 chunk 和正常 chunk，只 yield 有效 content。"""
        from src.services.llm_client import LLMClient

        client = LLMClient()

        choice_none = MagicMock()
        choice_none.delta = MagicMock()
        choice_none.delta.content = None

        choice_empty = MagicMock()
        choice_empty.delta = MagicMock()
        choice_empty.delta.content = ""

        mock_stream = AsyncMock()
        mock_stream.__aiter__.return_value = [
            self._make_chunk([]),                      # 空 choices
            self._make_chunk([self._make_choice("A")]), # 正常
            self._make_chunk([self._make_choice(None)]), # delta 为 None
            self._make_chunk([choice_none]),            # content 为 None
            self._make_chunk([choice_empty]),           # content 为空字符串
            self._make_chunk([self._make_choice("B")]), # 正常
            self._make_chunk([]),                      # 空 choices
        ]
        client._client.chat.completions.create = AsyncMock(return_value=mock_stream)

        result = await self._collect_stream(
            client.chat_stream([{"role": "user", "content": "hi"}])
        )

        assert result == ["A", "B"]

    @pytest.mark.asyncio
    async def test_all_empty_chunks_yields_nothing(self):
        """全是异常 chunk 时不 yield 任何内容，也不抛异常。"""
        from src.services.llm_client import LLMClient

        client = LLMClient()
        mock_stream = AsyncMock()
        mock_stream.__aiter__.return_value = [
            self._make_chunk([]),
            self._make_chunk([]),
        ]
        client._client.chat.completions.create = AsyncMock(return_value=mock_stream)

        result = await self._collect_stream(
            client.chat_stream([{"role": "user", "content": "hi"}])
        )

        assert result == []
