"""RootCauseAgent unit tests for state-first dual-emit architecture.

Helpers that used to live here (e.g. _evaluate_rule_conditions) moved to
``app.tools.graph_tools`` — those are exercised by ``test_graph_tools.py``.

This file focuses on the agent-level behavior:
  - reasoning_steps written into returned state
  - adispatch_custom_event fired for each step (state-first + stream-second)
  - Markdown response built from the steps
  - Five branch scenarios (no identifier, normal, KG missing, record missing,
    rule missing)
"""

from __future__ import annotations

from typing import Any
from unittest.mock import AsyncMock, patch

import pytest
from langchain_core.messages import HumanMessage

from app.agents.root_cause_agent import root_cause_agent_node


# --------------------------------------------------------------------------- #
# Helpers
# --------------------------------------------------------------------------- #


def _capture_dispatch_calls() -> tuple[Any, list[dict[str, Any]]]:
    """Return an (AsyncMock, captured_payloads) pair.

    The mock signature mirrors ``adispatch_custom_event(name, data)`` and
    appends the payload to ``captured`` so tests can assert what was streamed.
    """
    captured: list[dict[str, Any]] = []

    async def _record(name: str, data: dict[str, Any]) -> None:
        if name == "reasoning_step":
            captured.append(data)

    return AsyncMock(side_effect=_record), captured


# --------------------------------------------------------------------------- #
# Scenario: no identifier (early-return fallback)
# --------------------------------------------------------------------------- #


@pytest.mark.asyncio
async def test_no_identifier_emits_single_fallback() -> None:
    dispatch_mock, captured = _capture_dispatch_calls()
    with patch(
        "app.agents.root_cause_agent.adispatch_custom_event", new=dispatch_mock
    ):
        result = await root_cause_agent_node(
            {
                "messages": [HumanMessage(content="为什么它是 C 级？")],
                "entities": {"grade": "C"},
                "context": {},
            }
        )

    assert result["intent"] == "root_cause"
    assert "炉号或批次号" in result["response"]
    assert result["reasoning_steps"] == [
        {"kind": "fallback", "label": "请提供要归因的炉号或批次号"}
    ]
    assert len(captured) == 1
    assert captured[0]["kind"] == "fallback"


# --------------------------------------------------------------------------- #
# Scenario: normal path
# --------------------------------------------------------------------------- #


@pytest.mark.asyncio
async def test_normal_path_emits_full_chain() -> None:
    dispatch_mock, captured = _capture_dispatch_calls()
    fake_steps = [
        {"kind": "record", "label": "命中"},
        {"kind": "spec", "label": "规格 120", "meta": {"spec_code": "120"}},
        {"kind": "rule", "label": "C 级规则"},
        {
            "kind": "condition",
            "label": "Ps铁损 ≤ 1.30",
            "field": "F_PERF_PS_LOSS",
            "expected": "<= 1.30",
            "actual": 1.46,
            "satisfied": False,
        },
        {"kind": "grade", "label": "判定为 C 级", "meta": {"grade": "C"}},
    ]
    traverse_mock = AsyncMock(return_value=fake_steps)

    with (
        patch(
            "app.agents.root_cause_agent.adispatch_custom_event", new=dispatch_mock
        ),
        patch(
            "app.agents.root_cause_agent.traverse_judgment_path"
        ) as tool_patch,
    ):
        tool_patch.ainvoke = traverse_mock
        result = await root_cause_agent_node(
            {
                "messages": [
                    HumanMessage(content="为什么炉号 1丙20260110-1 是 C 级？")
                ],
                "entities": {
                    "furnace_no": "1丙20260110-1",
                    "grade": "C",
                },
                "context": {},
            }
        )

    assert result["intent"] == "root_cause"
    assert result["reasoning_steps"] == fake_steps
    assert len(captured) == 5
    assert [c["kind"] for c in captured] == [
        "record",
        "spec",
        "rule",
        "condition",
        "grade",
    ]
    assert "判定为 C 级" in result["response"]
    assert "Ps铁损" in result["response"]


@pytest.mark.asyncio
async def test_normal_path_response_contains_table_when_conditions_present() -> None:
    dispatch_mock, _ = _capture_dispatch_calls()
    fake_steps = [
        {"kind": "record", "label": "命中"},
        {"kind": "spec", "label": "规格 120", "meta": {"spec_code": "120"}},
        {"kind": "rule", "label": "C 级规则"},
        {
            "kind": "condition",
            "label": "带宽 >= 119.5",
            "field": "F_WIDTH",
            "expected": ">= 119.5",
            "actual": 119.8,
            "satisfied": True,
        },
        {"kind": "grade", "label": "判定为 C 级", "meta": {"grade": "C"}},
    ]
    traverse_mock = AsyncMock(return_value=fake_steps)

    with (
        patch(
            "app.agents.root_cause_agent.adispatch_custom_event", new=dispatch_mock
        ),
        patch(
            "app.agents.root_cause_agent.traverse_judgment_path"
        ) as tool_patch,
    ):
        tool_patch.ainvoke = traverse_mock
        result = await root_cause_agent_node(
            {
                "messages": [HumanMessage(content="为什么 1丙20260110-1 是 C 级？")],
                "entities": {"furnace_no": "1丙20260110-1", "grade": "C"},
                "context": {},
            }
        )

    assert "| 条件 | 结果 | 实际值 | 期望 |" in result["response"]
    assert "满足" in result["response"]


# --------------------------------------------------------------------------- #
# Scenario: KG missing (fallback, no rule reached)
# --------------------------------------------------------------------------- #


@pytest.mark.asyncio
async def test_kg_unavailable_path_falls_back() -> None:
    dispatch_mock, captured = _capture_dispatch_calls()
    fallback_steps = [
        {"kind": "record", "label": "命中"},
        {"kind": "spec", "label": "规格 120"},
        {"kind": "fallback", "label": "知识图谱当前不可用（KG manager 返回 None）"},
    ]
    traverse_mock = AsyncMock(return_value=fallback_steps)

    with (
        patch(
            "app.agents.root_cause_agent.adispatch_custom_event", new=dispatch_mock
        ),
        patch(
            "app.agents.root_cause_agent.traverse_judgment_path"
        ) as tool_patch,
    ):
        tool_patch.ainvoke = traverse_mock
        result = await root_cause_agent_node(
            {
                "messages": [HumanMessage(content="为什么 1丙20260110-1 是 C 级？")],
                "entities": {"furnace_no": "1丙20260110-1", "grade": "C"},
                "context": {},
            }
        )

    assert any(s["kind"] == "fallback" for s in result["reasoning_steps"])
    assert "知识图谱" in result["response"]
    assert any(c["kind"] == "fallback" for c in captured)


# --------------------------------------------------------------------------- #
# Scenario: record missing
# --------------------------------------------------------------------------- #


@pytest.mark.asyncio
async def test_record_missing_path_falls_back() -> None:
    dispatch_mock, _ = _capture_dispatch_calls()
    fallback_steps = [
        {"kind": "fallback", "label": "未找到标识为 ghost 的检测记录"}
    ]
    traverse_mock = AsyncMock(return_value=fallback_steps)

    with (
        patch(
            "app.agents.root_cause_agent.adispatch_custom_event", new=dispatch_mock
        ),
        patch(
            "app.agents.root_cause_agent.traverse_judgment_path"
        ) as tool_patch,
    ):
        tool_patch.ainvoke = traverse_mock
        result = await root_cause_agent_node(
            {
                "messages": [HumanMessage(content="为什么 ghost 是 C 级？")],
                "entities": {"furnace_no": "ghost", "grade": "C"},
                "context": {},
            }
        )

    assert result["reasoning_steps"] == fallback_steps
    assert "未找到" in result["response"]


# --------------------------------------------------------------------------- #
# Scenario: rule missing
# --------------------------------------------------------------------------- #


@pytest.mark.asyncio
async def test_rule_missing_path_falls_back() -> None:
    dispatch_mock, _ = _capture_dispatch_calls()
    fallback_steps = [
        {"kind": "record", "label": "命中"},
        {"kind": "spec", "label": "规格 999"},
        {"kind": "fallback", "label": "未在图谱中找到规格 999 下 C 级的判定规则"},
    ]
    traverse_mock = AsyncMock(return_value=fallback_steps)

    with (
        patch(
            "app.agents.root_cause_agent.adispatch_custom_event", new=dispatch_mock
        ),
        patch(
            "app.agents.root_cause_agent.traverse_judgment_path"
        ) as tool_patch,
    ):
        tool_patch.ainvoke = traverse_mock
        result = await root_cause_agent_node(
            {
                "messages": [HumanMessage(content="为什么 1丙20260110-1 是 C 级？")],
                "entities": {"furnace_no": "1丙20260110-1", "grade": "C"},
                "context": {},
            }
        )

    assert any("未在图谱中找到" in s.get("label", "") for s in result["reasoning_steps"])
    assert "未在图谱中找到" in result["response"]
