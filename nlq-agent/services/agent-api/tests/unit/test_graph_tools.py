"""Unit tests for app.tools.graph_tools.

Covers the @tool ``traverse_judgment_path`` and the underlying helpers.

5 required scenarios from plan AC-3:
  1. normal path (record + spec + rule + conditions + grade)
  2. furnace_no not found in DB
  3. target_grade missing AND record has no F_LABELING
  4. KG returns None (manager unavailable)
  5. Cypher exception during rule fetch
"""

from __future__ import annotations

from typing import Any
from unittest.mock import AsyncMock, MagicMock, patch

import pytest

from app.tools.graph_tools import (
    _evaluate_rule_conditions,
    _humanize_field,
    traverse_judgment_path,
)


# --------------------------------------------------------------------------- #
# Helper-level tests
# --------------------------------------------------------------------------- #


class TestEvaluateRuleConditions:
    """Static evaluation of rule conditions (migrated from root_cause_agent)."""

    def test_separates_pass_and_fail(self) -> None:
        record = {
            "F_BATCH_NO": "BATCH-001",
            "F_FURNACE_NO": "1丙20260110-1",
            "F_WIDTH": 119.8,
            "F_PERF_PS_LOSS": 1.46,
        }
        rule = {
            "name": "C级",
            "conditions": [
                {"field": "F_WIDTH", "operator": ">=", "value": 119.5},
                {"field": "F_PERF_PS_LOSS", "operator": "<=", "value": 1.30},
            ],
        }
        result = _evaluate_rule_conditions(record, rule)
        assert len(result["satisfied"]) == 1
        assert len(result["failed"]) == 1
        assert result["satisfied"][0]["actual"] == 119.8
        assert result["failed"][0]["field"] == "F_PERF_PS_LOSS"

    def test_humanize_field_chinese_label(self) -> None:
        assert _humanize_field("F_PERF_PS_LOSS") == "Ps铁损"
        assert _humanize_field("F_UNKNOWN") == "F_UNKNOWN"
        assert _humanize_field("") == "未知字段"


# --------------------------------------------------------------------------- #
# traverse_judgment_path @tool tests
# --------------------------------------------------------------------------- #


_NORMAL_RECORD = {
    "furnace_no": "1丙20260110-1",
    "batch_no": "BATCH-001",
    "spec_code": "120",
    "grade": "C",
    "F_WIDTH": 119.8,
    "F_PERF_PS_LOSS": 1.46,
}

_NORMAL_RULE_QUERY_RESULT = [
    {
        "rule": {
            "id": "r-1",
            "name": "C级",
            "priority": 1,
            "qualityStatus": "不合格",
            "color": "#f97316",
            "isDefault": False,
            "conditionJson": (
                '[{"field": "F_WIDTH", "operator": ">=", "value": 119.5},'
                ' {"field": "F_PERF_PS_LOSS", "operator": "<=", "value": 1.30}]'
            ),
        }
    }
]


def _build_kg(query_result: list[dict[str, Any]]) -> MagicMock:
    graph = MagicMock()
    graph.query_async = AsyncMock(return_value=query_result)
    return graph


@pytest.mark.asyncio
async def test_traverse_returns_minimum_path() -> None:
    """AC-3 hard assertion: non-empty list with record + rule + ordering."""
    with (
        patch(
            "app.tools.graph_tools.execute_safe_sql",
            new=AsyncMock(return_value=[_NORMAL_RECORD]),
        ),
        patch(
            "app.tools.graph_tools.get_knowledge_graph",
            return_value=_build_kg(_NORMAL_RULE_QUERY_RESULT),
        ),
    ):
        result = await traverse_judgment_path.ainvoke(
            {
                "furnace_no": "1丙20260110-1",
                "batch_no": None,
                "target_grade": "C",
            }
        )

    assert len(result) > 0
    assert any(s["kind"] == "record" for s in result)
    assert any(s["kind"] == "rule" for s in result)

    record_idx = next(i for i, s in enumerate(result) if s["kind"] == "record")
    rule_idx = next(i for i, s in enumerate(result) if s["kind"] == "rule")
    condition_indices = [i for i, s in enumerate(result) if s["kind"] == "condition"]
    grade_idx = next(
        (i for i, s in enumerate(result) if s["kind"] == "grade"), len(result)
    )

    assert record_idx < rule_idx, "record must precede rule"
    if condition_indices:
        assert min(condition_indices) > rule_idx, (
            "condition steps must come after rule"
        )
    assert grade_idx >= max(condition_indices) if condition_indices else True


@pytest.mark.asyncio
async def test_traverse_no_record_returns_fallback() -> None:
    """Missing furnace returns a single fallback step, no exception."""
    with (
        patch(
            "app.tools.graph_tools.execute_safe_sql",
            new=AsyncMock(return_value=[]),
        ),
        patch(
            "app.tools.graph_tools.get_knowledge_graph",
            return_value=_build_kg(_NORMAL_RULE_QUERY_RESULT),
        ),
    ):
        result = await traverse_judgment_path.ainvoke(
            {
                "furnace_no": "ghost-furnace",
                "batch_no": None,
                "target_grade": "C",
            }
        )

    assert len(result) >= 1
    assert result[-1]["kind"] == "fallback"
    assert "未找到" in result[-1]["label"]


@pytest.mark.asyncio
async def test_traverse_no_grade_no_label_returns_fallback() -> None:
    """No target_grade + record without F_LABELING grade → fallback."""
    record_no_grade = dict(_NORMAL_RECORD)
    record_no_grade["grade"] = ""
    with (
        patch(
            "app.tools.graph_tools.execute_safe_sql",
            new=AsyncMock(return_value=[record_no_grade]),
        ),
        patch(
            "app.tools.graph_tools.get_knowledge_graph",
            return_value=_build_kg(_NORMAL_RULE_QUERY_RESULT),
        ),
    ):
        result = await traverse_judgment_path.ainvoke(
            {
                "furnace_no": "1丙20260110-1",
                "batch_no": None,
                "target_grade": "",
            }
        )

    assert any(s["kind"] == "fallback" for s in result)
    fallback = next(s for s in result if s["kind"] == "fallback")
    assert "等级" in fallback["label"]


@pytest.mark.asyncio
async def test_traverse_kg_unavailable_returns_fallback() -> None:
    """get_knowledge_graph returning None must produce a fallback step."""
    with (
        patch(
            "app.tools.graph_tools.execute_safe_sql",
            new=AsyncMock(return_value=[_NORMAL_RECORD]),
        ),
        patch(
            "app.tools.graph_tools.get_knowledge_graph",
            return_value=None,
        ),
    ):
        result = await traverse_judgment_path.ainvoke(
            {
                "furnace_no": "1丙20260110-1",
                "batch_no": None,
                "target_grade": "C",
            }
        )

    assert any(s["kind"] == "fallback" for s in result)
    fallback = next(s for s in result if s["kind"] == "fallback")
    assert "知识图谱" in fallback["label"]


@pytest.mark.asyncio
async def test_traverse_cypher_exception_returns_fallback() -> None:
    """Cypher exception during rule fetch becomes a fallback, no raise."""
    bad_graph = MagicMock()
    bad_graph.query_async = AsyncMock(side_effect=RuntimeError("BOOM Cypher"))

    with (
        patch(
            "app.tools.graph_tools.execute_safe_sql",
            new=AsyncMock(return_value=[_NORMAL_RECORD]),
        ),
        patch(
            "app.tools.graph_tools.get_knowledge_graph",
            return_value=bad_graph,
        ),
    ):
        result = await traverse_judgment_path.ainvoke(
            {
                "furnace_no": "1丙20260110-1",
                "batch_no": None,
                "target_grade": "C",
            }
        )

    assert any(s["kind"] == "fallback" for s in result)
    fallback = next(s for s in result if s["kind"] == "fallback")
    assert "RuntimeError" in fallback["label"] or "查询失败" in fallback["label"]


@pytest.mark.asyncio
async def test_traverse_no_identifier_returns_single_fallback() -> None:
    """Empty furnace AND batch returns a single fallback (no DB call)."""
    result = await traverse_judgment_path.ainvoke(
        {"furnace_no": "", "batch_no": None, "target_grade": "C"}
    )
    assert result == [{"kind": "fallback", "label": "请提供炉号或批次号"}]
