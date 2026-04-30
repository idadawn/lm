"""
S8: E2E test — by_shift query (mocked LLM/Qdrant/DB).

Target query: "白班和晚班的铁损 P17/50 合格率差异"

Verifies:
1. SSE event sequence contains ≥1 spec/condition/grade
2. response_metadata.sql contains CASE WHEN HOUR(F_CREATORTIME) shift mapping
3. row_count > 0 (mock)
"""

from __future__ import annotations

import json
from unittest.mock import AsyncMock, MagicMock

import pytest

from src.models.schemas import (
    AgentContext,
    FilterCondition,
    IntentClassification,
    IntentType,
    MetricDefinition,
    ReasoningStep,
    ReasoningStepKind,
)
from src.pipelines.stage2.data_sql_agent import DataSQLAgent
from src.services.database import DatabaseService
from src.services.llm_client import LLMClient
from src.services.sse_emitter import SSEEmitter


# ── Fixtures ───────────────────────────────────────────────


def _make_byshift_context() -> AgentContext:
    """构建班次对比查询的 AgentContext。"""
    return AgentContext(
        user_question="白班和晚班的铁损 P17/50 合格率差异",
        intent=IntentClassification(
            intent=IntentType.BY_SHIFT,
            confidence=0.9,
            extracted_entities={
                "time_window": 1,
                "metric": "合格率",
            },
        ),
        business_explanation="按班次（早班/中班/晚班）分组比较合格率",
        filters=[],
        metrics=[
            MetricDefinition(
                name="合格率",
                formula="SUM(qualified)/COUNT(*)*100",
                description="综合合格率",
            ),
        ],
    )


def _mock_db_byshift() -> DatabaseService:
    """Mock DB returning by_shift query results."""
    db = MagicMock(spec=DatabaseService)
    db.validate_sql = MagicMock(return_value=(True, ""))

    shift_rows = [
        {
            "shift": "早班",
            "sample_count": 120,
            "qualified_count": 108,
            "qualified_rate": 90.0,
        },
        {
            "shift": "中班",
            "sample_count": 110,
            "qualified_count": 93,
            "qualified_rate": 84.55,
        },
        {
            "shift": "晚班",
            "sample_count": 100,
            "qualified_count": 80,
            "qualified_rate": 80.0,
        },
    ]
    shift_result = {
        "columns": [
            "shift", "sample_count",
            "qualified_count", "qualified_rate",
        ],
        "rows": shift_rows,
        "row_count": len(shift_rows),
        "truncated": False,
    }

    db.execute_query = AsyncMock(return_value=shift_result)
    return db


def _mock_llm() -> LLMClient:
    """Mock LLM — by_shift uses template, LLM only for final answer stream."""
    llm = MagicMock(spec=LLMClient)
    llm.chat_json = AsyncMock(return_value={"sql": "", "explanation": ""})

    async def _empty_stream(*a, **kw):
        return
        yield  # noqa: unreachable

    llm.chat_stream = MagicMock(return_value=_empty_stream())
    return llm


# ── E2E Test ───────────────────────────────────────────────


class TestE2EByShiftQuery:
    """E2E: by_shift query via template-based SQL generation."""

    async def test_sse_events_contain_grade_and_shift_sql(self) -> None:
        """SSE events contain ≥1 grade step; SQL has CASE WHEN HOUR shift mapping."""
        db = _mock_db_byshift()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_byshift_context())

        # Parse all SSE events
        parsed = []
        for ev in events:
            assert ev.startswith("data: "), f"Bad SSE format: {ev[:60]}"
            data = json.loads(ev[len("data: "):].strip())
            parsed.append(data)

        # Assert ≥1 spec/condition/grade step in reasoning_steps
        has_spec_or_condition_or_grade = False
        for ev in parsed:
            if ev.get("type") == "reasoning_step":
                kind = ev.get("reasoning_step", {}).get("kind", "")
                if kind in ("spec", "condition", "grade"):
                    has_spec_or_condition_or_grade = True
        assert has_spec_or_condition_or_grade, (
            "SSE events missing spec/condition/grade step"
        )

        # Assert response_metadata.sql contains shift mapping structures
        meta_events = [e for e in parsed if e.get("type") == "response_metadata"]
        assert len(meta_events) >= 1, "Missing response_metadata event"
        sql = meta_events[0].get("response_payload", {}).get("sql", "")
        assert "CASE" in sql, f"SQL missing CASE: {sql[:120]}"
        assert "HOUR(F_CREATORTIME)" in sql, (
            f"SQL missing HOUR(F_CREATORTIME): {sql[:120]}"
        )

    async def test_byshift_sql_validates(self) -> None:
        """Template-generated SQL passes validate_sql."""
        db = _mock_db_byshift()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        await agent.run(_make_byshift_context())

        # validate_sql should have been called at least once
        assert db.validate_sql.called
        for call in db.validate_sql.call_args_list:
            sql_str = call.args[0]
            assert "SELECT" in sql_str

    async def test_byshift_grade_summary_mentions_shifts(self) -> None:
        """Grade step summary mentions shift names and rates."""
        db = _mock_db_byshift()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_byshift_context())

        # Find grade steps
        for ev in events:
            data = json.loads(ev[len("data: "):].strip())
            if data.get("type") == "reasoning_step":
                step = data.get("reasoning_step", {})
                if step.get("kind") == "grade":
                    label = step.get("label", "")
                    assert "合格率" in label, f"Grade label missing 合格率: {label}"
                    # Should contain at least one shift name
                    assert any(s in label for s in ("早班", "中班", "晚班")), (
                        f"Grade label missing shift name: {label}"
                    )
                    return

        pytest.fail("No grade step found in SSE events")

    async def test_byshift_row_count_positive(self) -> None:
        """Response metadata shows row_count > 0."""
        db = _mock_db_byshift()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_byshift_context())

        for ev in events:
            data = json.loads(ev[len("data: "):].strip())
            if data.get("type") == "response_metadata":
                row_count = data.get("response_payload", {}).get("row_count", 0)
                assert row_count > 0, f"Expected row_count > 0, got {row_count}"
                return

        pytest.fail("No response_metadata event found")
