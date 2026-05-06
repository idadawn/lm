"""SSE streaming tests for the chat endpoint.

Two layers of coverage:

1. **Gating spike** (`test_adispatch_custom_event_emits_in_astream_events_v2`):
   验证 LangChain `adispatch_custom_event` 在 `astream_events(version="v2")` 下
   能够透出 `on_custom_event` 事件。这是 plan 里 Step 2.0 的 gating spike——
   该测试 PASS 才允许 RootCauseAgent 在节点内 dispatch reasoning_step。
   FAIL 则触发 Plan B（state-only fallback）。

2. **SSE forwarding 单测**：mock graph，验证 chat.py 把 `on_custom_event` 翻译成
   `event:reasoning_step` SSE 行 + 顺序保持。

PASS gate semantics:
- 两个测试都通过 → adispatch + 转发都 OK，可继续后续 RootCause 改造。
- 第一个 FAIL → Plan B 启动（参考 plan Step 2.0 Plan B）。
- 第二个 FAIL → chat.py:71-157 on_custom_event 分支需要修复。
"""

from __future__ import annotations

import json
from collections.abc import AsyncIterator
from typing import Any
from unittest.mock import AsyncMock, patch

import pytest


# --------------------------------------------------------------------------- #
# Layer 1: Gating spike — exercises real LangGraph + LangChain primitives.
# --------------------------------------------------------------------------- #


@pytest.mark.asyncio
async def test_adispatch_custom_event_emits_in_astream_events_v2() -> None:
    """Spike: confirm `adispatch_custom_event` surfaces in `astream_events(v2)`.

    This is the gating test for plan Step 2.0. If it fails, RootCauseAgent
    cannot rely on streaming custom events and must fall back to state-only
    Plan B. nlq-agent has zero prior call sites for `adispatch_custom_event`
    (grep confirmed at planning time), so this is a real unknown.
    """
    from langchain_core.callbacks import adispatch_custom_event
    from langgraph.graph import END, START, StateGraph

    async def emit_step_node(state: dict[str, Any]) -> dict[str, Any]:
        await adispatch_custom_event(
            "reasoning_step",
            {"kind": "record", "label": "测试 record"},
        )
        await adispatch_custom_event(
            "reasoning_step",
            {"kind": "rule", "label": "测试 rule"},
        )
        return {"done": True}

    workflow = StateGraph(dict)
    workflow.add_node("emit", emit_step_node)
    workflow.add_edge(START, "emit")
    workflow.add_edge("emit", END)
    graph = workflow.compile()

    custom_events: list[dict[str, Any]] = []
    async for event in graph.astream_events({}, version="v2"):
        if event.get("event") == "on_custom_event":
            custom_events.append(event)

    assert len(custom_events) >= 2, (
        f"Expected at least 2 on_custom_event from astream_events v2, got "
        f"{len(custom_events)}. adispatch_custom_event may not be wired in this "
        f"LangChain/LangGraph version — Plan B (state-only) must activate."
    )

    names = [e.get("name") for e in custom_events]
    assert names == ["reasoning_step", "reasoning_step"], (
        f"Expected name='reasoning_step' for both events, got {names}"
    )

    payloads = [e.get("data") for e in custom_events]
    kinds = [p.get("kind") for p in payloads if isinstance(p, dict)]
    assert kinds == ["record", "rule"], (
        f"Custom event payloads lost ordering or kind field; got kinds={kinds}"
    )


# --------------------------------------------------------------------------- #
# Layer 2: chat.py SSE forwarding — mocks the graph, validates chat.py logic.
# --------------------------------------------------------------------------- #


def _scripted_astream_events(
    events: list[dict[str, Any]],
) -> AsyncIterator[dict[str, Any]]:
    async def _gen() -> AsyncIterator[dict[str, Any]]:
        for event in events:
            yield event

    return _gen()


def _parse_sse_lines(body: str) -> list[dict[str, Any]]:
    """Parse SSE ``data:`` lines into payload dicts (skipping [DONE])."""
    parsed: list[dict[str, Any]] = []
    for line in body.splitlines():
        line = line.strip()
        if not line.startswith("data:"):
            continue
        payload_text = line[len("data:"):].strip()
        if not payload_text or payload_text == "[DONE]":
            continue
        try:
            parsed.append(json.loads(payload_text))
        except json.JSONDecodeError:
            continue
    return parsed


@pytest.mark.asyncio
async def test_reasoning_step_event_emitted() -> None:
    """chat.py forwards `on_custom_event` (name=reasoning_step) as SSE."""
    from app.api import chat as chat_api
    from app.models.schemas import ChatMessage, ChatRequest

    scripted = [
        {
            "event": "on_custom_event",
            "name": "reasoning_step",
            "data": {"kind": "record", "label": "炉号 1丙20260110-1 命中"},
        },
        {
            "event": "on_custom_event",
            "name": "reasoning_step",
            "data": {"kind": "rule", "label": "C 级判定规则"},
        },
        {
            "event": "on_custom_event",
            "name": "reasoning_step",
            "data": {
                "kind": "condition",
                "label": "Ps铁损 ≤ 1.30",
                "field": "F_PERF_PS_LOSS",
                "expected": "<= 1.30",
                "actual": 1.45,
                "satisfied": False,
            },
        },
    ]

    graph_mock = AsyncMock()
    graph_mock.astream_events = lambda *args, **kwargs: _scripted_astream_events(scripted)

    fake_request = AsyncMock()
    fake_request.headers = {}

    with patch("app.api.chat.create_agent_graph", return_value=graph_mock), patch(
        "app.api.chat.validate_chat_auth", new_callable=AsyncMock, return_value={}
    ):
        response = await chat_api.chat_stream(
            ChatRequest(
                session_id="spike-1",
                messages=[ChatMessage(role="user", content="为什么 C 级")],
            ),
            fake_request,
        )

        body_chunks: list[str] = []
        async for chunk in response.body_iterator:
            if isinstance(chunk, bytes):
                body_chunks.append(chunk.decode("utf-8"))
            else:
                body_chunks.append(str(chunk))

    body = "".join(body_chunks)
    parsed = _parse_sse_lines(body)
    reasoning_events = [p for p in parsed if p.get("type") == "reasoning_step"]

    assert len(reasoning_events) == 3, (
        f"Expected 3 reasoning_step SSE events forwarded, got {len(reasoning_events)}. "
        f"Body excerpt: {body[:400]}"
    )

    kinds = [evt.get("reasoning_step", {}).get("kind") for evt in reasoning_events]
    assert kinds == ["record", "rule", "condition"], (
        f"reasoning_step ordering not preserved: {kinds}"
    )

    condition_evt = reasoning_events[2]["reasoning_step"]
    assert condition_evt["satisfied"] is False
    assert condition_evt["actual"] == 1.45


@pytest.mark.asyncio
async def test_reasoning_step_ordering_preserved() -> None:
    """Interleaved text + reasoning_step events keep their wire order."""
    from app.api import chat as chat_api
    from app.models.schemas import ChatMessage, ChatRequest

    class _Chunk:
        def __init__(self, content: str) -> None:
            self.content = content

    scripted = [
        {
            "event": "on_chat_model_stream",
            "name": "model",
            "data": {"chunk": _Chunk("炉号")},
        },
        {
            "event": "on_custom_event",
            "name": "reasoning_step",
            "data": {"kind": "record", "label": "命中记录"},
        },
        {
            "event": "on_chat_model_stream",
            "name": "model",
            "data": {"chunk": _Chunk(" 1丙")},
        },
        {
            "event": "on_custom_event",
            "name": "reasoning_step",
            "data": {"kind": "rule", "label": "B 级"},
        },
    ]

    graph_mock = AsyncMock()
    graph_mock.astream_events = lambda *args, **kwargs: _scripted_astream_events(scripted)

    fake_request = AsyncMock()
    fake_request.headers = {}

    with patch("app.api.chat.create_agent_graph", return_value=graph_mock), patch(
        "app.api.chat.validate_chat_auth", new_callable=AsyncMock, return_value={}
    ):
        response = await chat_api.chat_stream(
            ChatRequest(
                session_id="spike-2",
                messages=[ChatMessage(role="user", content="顺序")],
            ),
            fake_request,
        )
        body_chunks: list[str] = []
        async for chunk in response.body_iterator:
            if isinstance(chunk, bytes):
                body_chunks.append(chunk.decode("utf-8"))
            else:
                body_chunks.append(str(chunk))

    body = "".join(body_chunks)
    parsed = _parse_sse_lines(body)
    types = [p.get("type") for p in parsed]

    assert types == ["text", "reasoning_step", "text", "reasoning_step"], (
        f"Expected interleaved order text/reasoning_step/text/reasoning_step, got {types}"
    )
