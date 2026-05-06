"""Non-streaming /chat endpoint tests for the reasoning_steps dual channel.

The streaming SSE endpoint forwards `reasoning_step` custom events live; this
test verifies the parallel non-streaming path returns the SAME data via
`reasoning_steps` JSON key, ensuring AC-8 (state-first) holds.
"""

from __future__ import annotations

from unittest.mock import AsyncMock, patch

import pytest

from app.api import chat as chat_api
from app.models.schemas import ChatMessage, ChatRequest


@pytest.mark.asyncio
async def test_reasoning_steps_in_nonstream_response() -> None:
    """Non-streaming /chat returns reasoning_steps key as a list."""
    sample_steps = [
        {"kind": "record", "label": "命中检测记录"},
        {"kind": "spec", "label": "产品规格 120"},
        {"kind": "rule", "label": "C 级规则"},
        {
            "kind": "condition",
            "label": "Ps铁损 ≤ 1.30",
            "field": "F_PERF_PS_LOSS",
            "expected": "<= 1.30",
            "actual": 1.45,
            "satisfied": False,
        },
        {"kind": "grade", "label": "判定为 C 级"},
    ]
    graph = AsyncMock()
    graph.ainvoke = AsyncMock(
        return_value={
            "response": "炉号 1丙20260110-1 被判为 C 级。",
            "chart_config": None,
            "intent": "root_cause",
            "entities": {"furnace_no": "1丙20260110-1", "grade": "C"},
            "context": {"furnace_no": "1丙20260110-1"},
            "reasoning_steps": sample_steps,
        }
    )

    fake_request = AsyncMock()
    fake_request.headers = {}

    with patch("app.api.chat.create_agent_graph", return_value=graph), patch(
        "app.api.chat.validate_chat_auth", new_callable=AsyncMock, return_value={}
    ):
        chat_api._SESSION_CONTEXTS.clear()
        response = await chat_api.chat(
            ChatRequest(
                session_id="dual-channel-1",
                messages=[
                    ChatMessage(role="user", content="为什么 1丙20260110-1 是 C 级")
                ],
            ),
            fake_request,
        )

    assert "reasoning_steps" in response, (
        "Non-streaming /chat must return reasoning_steps key (state-first dual "
        "channel — AC-8)"
    )
    steps = response["reasoning_steps"]
    assert isinstance(steps, list)
    assert len(steps) == len(sample_steps)
    assert steps[0]["kind"] == "record"
    assert steps[-1]["kind"] == "grade"
    condition = next(s for s in steps if s["kind"] == "condition")
    assert condition["satisfied"] is False
    assert condition["actual"] == 1.45


@pytest.mark.asyncio
async def test_reasoning_steps_default_empty_list_when_missing() -> None:
    """If state has no reasoning_steps, response defaults to empty list (no KeyError)."""
    graph = AsyncMock()
    graph.ainvoke = AsyncMock(
        return_value={
            "response": "查询结果。",
            "chart_config": None,
            "intent": "query",
            "entities": {},
            "context": {},
        }
    )

    fake_request = AsyncMock()
    fake_request.headers = {}

    with patch("app.api.chat.create_agent_graph", return_value=graph), patch(
        "app.api.chat.validate_chat_auth", new_callable=AsyncMock, return_value={}
    ):
        chat_api._SESSION_CONTEXTS.clear()
        response = await chat_api.chat(
            ChatRequest(
                session_id="dual-channel-2",
                messages=[ChatMessage(role="user", content="正常 query")],
            ),
            fake_request,
        )

    assert response["reasoning_steps"] == []
