"""Chat API unit tests for session-scoped context memory."""

from unittest.mock import AsyncMock, patch

import pytest
from fastapi import Request

from app.api import chat as chat_api
from app.models.schemas import ChatMessage, ChatRequest


def _http_request() -> Request:
    """构造无请求头的最小 HTTP Request（非流式端点签名需要）。"""
    return Request(scope={"type": "http", "headers": []})


def _patch_auth():
    """跳过上游登录态校验，鉴权上下文原样透传。

    避免测试受本地 .env 的 AUTH_REQUIRED / AUTH_VALIDATE_UPSTREAM 配置影响。
    """
    return patch(
        "app.api.chat.validate_chat_auth",
        new=AsyncMock(side_effect=lambda ctx: ctx),
    )


class TestChatApi:
    """Test chat API context persistence."""

    @pytest.mark.asyncio
    async def test_chat_reuses_context_by_session_id(self) -> None:
        """Load stored context into the graph and persist updated context after reply."""
        graph = AsyncMock()
        graph.ainvoke = AsyncMock(
            side_effect=[
                {
                    "response": "上个月 Ps铁损平均值为 1.20 W/kg。",
                    "chart_config": None,
                    "intent": "query",
                    "entities": {"metric": "psironloss"},
                    "context": {
                        "metric": "psironloss",
                        "time_range": {"type": "last_month"},
                        "spec_code": "120",
                    },
                },
                {
                    "response": "本月 Ps铁损平均值为 1.18 W/kg。",
                    "chart_config": None,
                    "intent": "query",
                    "entities": {"metric": "psironloss"},
                    "context": {
                        "metric": "psironloss",
                        "time_range": {"type": "current_month"},
                        "spec_code": "120",
                    },
                },
            ]
        )

        with patch("app.api.chat.create_agent_graph", return_value=graph), _patch_auth():
            chat_api._SESSION_CONTEXTS.clear()

            first_response = await chat_api.chat(
                ChatRequest(
                    session_id="session-ctx-1",
                    messages=[ChatMessage(role="user", content="上个月 Ps铁损")],
                ),
                _http_request(),
            )
            second_response = await chat_api.chat(
                ChatRequest(
                    session_id="session-ctx-1",
                    messages=[ChatMessage(role="user", content="那这个月呢")],
                ),
                _http_request(),
            )

        assert first_response["session_id"] == "session-ctx-1"
        assert second_response["session_id"] == "session-ctx-1"
        first_call = graph.ainvoke.await_args_list[0].args[0]
        second_call = graph.ainvoke.await_args_list[1].args[0]
        assert first_call["context"] == {}
        assert second_call["context"] == {
            "metric": "psironloss",
            "time_range": {"type": "last_month"},
            "spec_code": "120",
        }
        assert chat_api._SESSION_CONTEXTS["session-ctx-1"]["context"]["time_range"] == {
            "type": "current_month"
        }

    @pytest.mark.asyncio
    async def test_chat_resolves_model_name_per_request(self) -> None:
        """Explicit model wins; omitted model falls back to the .env default.

        不再回退到 _SESSION_CONTEXTS 缓存的历史 model_name——避免同一会话
        曾用过某个失效模型后，后续请求被永久锁死在该模型上。
        """
        graph = AsyncMock()
        graph.ainvoke = AsyncMock(
            side_effect=[
                {
                    "response": "第一次响应",
                    "chart_config": None,
                    "intent": "query",
                    "entities": {},
                    "context": {"metric": "psironloss"},
                },
                {
                    "response": "第二次响应",
                    "chart_config": None,
                    "intent": "query",
                    "entities": {},
                    "context": {"metric": "psironloss", "time_range": {"type": "current_month"}},
                },
            ]
        )

        with (
            patch("app.api.chat.create_agent_graph", return_value=graph),
            patch("app.api.chat._default_model_name", return_value="env-default-model"),
            _patch_auth(),
        ):
            chat_api._SESSION_CONTEXTS.clear()

            await chat_api.chat(
                ChatRequest(
                    session_id="session-model-1",
                    model_name="gemini-2.5-flash",
                    messages=[ChatMessage(role="user", content="查一下 Ps 铁损")],
                ),
                _http_request(),
            )
            await chat_api.chat(
                ChatRequest(
                    session_id="session-model-1",
                    model_name=None,
                    messages=[ChatMessage(role="user", content="那这个月呢")],
                ),
                _http_request(),
            )

        first_call = graph.ainvoke.await_args_list[0].args[0]
        second_call = graph.ainvoke.await_args_list[1].args[0]

        assert first_call["model_name"] == "gemini-2.5-flash"
        assert second_call["model_name"] == "env-default-model"
        assert chat_api._SESSION_CONTEXTS["session-model-1"]["model_name"] == "env-default-model"

    @pytest.mark.asyncio
    async def test_chat_returns_root_cause_response(self) -> None:
        """Pass RootCauseAgent response fields through the non-stream endpoint."""
        graph = AsyncMock()
        graph.ainvoke = AsyncMock(
            return_value={
                "response": "炉号 1丙20260110-1 被判为 C 级。",
                "chart_config": None,
                "intent": "root_cause",
                "entities": {"furnace_no": "1丙20260110-1", "grade": "C"},
                "context": {
                    "furnace_no": "1丙20260110-1",
                    "batch_no": "BATCH-001",
                    "grade": "C",
                },
            }
        )

        with patch("app.api.chat.create_agent_graph", return_value=graph), _patch_auth():
            response = await chat_api.chat(
                ChatRequest(
                    session_id="session-root-1",
                    messages=[
                        ChatMessage(role="user", content="为什么炉号 1丙20260110-1 是 C 级？")
                    ],
                ),
                _http_request(),
            )

        assert response["session_id"] == "session-root-1"
        assert response["intent"] == "root_cause"
        assert response["entities"]["grade"] == "C"
        assert response["context"]["batch_no"] == "BATCH-001"
