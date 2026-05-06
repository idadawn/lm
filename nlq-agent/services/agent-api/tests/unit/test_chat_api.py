"""Chat API unit tests for session-scoped context memory."""

from unittest.mock import AsyncMock, patch

import pytest

from app.api import chat as chat_api
from app.models.schemas import ChatMessage, ChatRequest


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

        with patch("app.api.chat.create_agent_graph", return_value=graph):
            chat_api._SESSION_CONTEXTS.clear()

            first_response = await chat_api.chat(
                ChatRequest(
                    session_id="session-ctx-1",
                    messages=[ChatMessage(role="user", content="上个月 Ps铁损")],
                )
            )
            second_response = await chat_api.chat(
                ChatRequest(
                    session_id="session-ctx-1",
                    messages=[ChatMessage(role="user", content="那这个月呢")],
                )
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
    async def test_chat_persists_model_name_by_session_id(self) -> None:
        """Reuse the last selected model for the same session when omitted later."""
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

        with patch("app.api.chat.create_agent_graph", return_value=graph):
            chat_api._SESSION_CONTEXTS.clear()

            await chat_api.chat(
                ChatRequest(
                    session_id="session-model-1",
                    model_name="gemini-2.5-flash",
                    messages=[ChatMessage(role="user", content="查一下 Ps 铁损")],
                )
            )
            await chat_api.chat(
                ChatRequest(
                    session_id="session-model-1",
                    model_name=None,
                    messages=[ChatMessage(role="user", content="那这个月呢")],
                )
            )

        first_call = graph.ainvoke.await_args_list[0].args[0]
        second_call = graph.ainvoke.await_args_list[1].args[0]

        assert first_call["model_name"] == "gemini-2.5-flash"
        assert second_call["model_name"] == "gemini-2.5-flash"
        assert chat_api._SESSION_CONTEXTS["session-model-1"]["model_name"] == "gemini-2.5-flash"

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

        with patch("app.api.chat.create_agent_graph", return_value=graph):
            response = await chat_api.chat(
                ChatRequest(
                    session_id="session-root-1",
                    messages=[
                        ChatMessage(role="user", content="为什么炉号 1丙20260110-1 是 C 级？")
                    ],
                )
            )

        assert response["session_id"] == "session-root-1"
        assert response["intent"] == "root_cause"
        assert response["entities"]["grade"] == "C"
        assert response["context"]["batch_no"] == "BATCH-001"
