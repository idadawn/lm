"""
T8: E2E test — trend query (mocked LLM/Qdrant/DB).

Target query: "最近 6 个月每月的产品合格率变化趋势（按产品规格分组）"

Verifies:
1. SSE event sequence contains ≥1 spec/condition/grade
2. response_metadata.sql contains DATE_FORMAT and ORDER BY month_bucket
3. Template-based SQL is valid (passes validate_sql)
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


def _make_trend_context() -> AgentContext:
    """构建趋势查询的 AgentContext。"""
    return AgentContext(
        user_question="最近6个月每月的产品合格率变化趋势（按产品规格分组）",
        intent=IntentClassification(
            intent=IntentType.TREND,
            confidence=0.9,
            extracted_entities={
                "time_window": 6,
                "metric": "合格率",
                "group_by": "产品规格",
            },
        ),
        business_explanation="分析近6个月各产品规格的合格率月度变化趋势",
        filters=[],
        metrics=[
            MetricDefinition(
                name="合格率",
                formula="SUM(qualified)/COUNT(*)*100",
                description="综合合格率",
            ),
        ],
    )


def _mock_db_trend() -> DatabaseService:
    """Mock DB returning trend query results."""
    db = MagicMock(spec=DatabaseService)
    db.validate_sql = MagicMock(return_value=(True, ""))

    trend_rows = [
        {
            "product_spec_id": "spec_001",
            "month_bucket": "2025-12",
            "sample_count": 100,
            "qualified_count": 80,
            "qualified_rate": 80.0,
        },
        {
            "product_spec_id": "spec_001",
            "month_bucket": "2026-01",
            "sample_count": 110,
            "qualified_count": 88,
            "qualified_rate": 80.0,
        },
        {
            "product_spec_id": "spec_001",
            "month_bucket": "2026-02",
            "sample_count": 105,
            "qualified_count": 89,
            "qualified_rate": 84.76,
        },
        {
            "product_spec_id": "spec_001",
            "month_bucket": "2026-03",
            "sample_count": 120,
            "qualified_count": 108,
            "qualified_rate": 90.0,
        },
        {
            "product_spec_id": "spec_001",
            "month_bucket": "2026-04",
            "sample_count": 115,
            "qualified_count": 104,
            "qualified_rate": 90.43,
        },
        {
            "product_spec_id": "spec_001",
            "month_bucket": "2026-05",
            "sample_count": 130,
            "qualified_count": 117,
            "qualified_rate": 90.0,
        },
        {
            "product_spec_id": "spec_002",
            "month_bucket": "2025-12",
            "sample_count": 90,
            "qualified_count": 72,
            "qualified_rate": 80.0,
        },
        {
            "product_spec_id": "spec_002",
            "month_bucket": "2026-01",
            "sample_count": 95,
            "qualified_count": 81,
            "qualified_rate": 85.26,
        },
        {
            "product_spec_id": "spec_002",
            "month_bucket": "2026-02",
            "sample_count": 100,
            "qualified_count": 88,
            "qualified_rate": 88.0,
        },
        {
            "product_spec_id": "spec_002",
            "month_bucket": "2026-03",
            "sample_count": 105,
            "qualified_count": 89,
            "qualified_rate": 84.76,
        },
        {
            "product_spec_id": "spec_002",
            "month_bucket": "2026-04",
            "sample_count": 110,
            "qualified_count": 99,
            "qualified_rate": 90.0,
        },
        {
            "product_spec_id": "spec_002",
            "month_bucket": "2026-05",
            "sample_count": 115,
            "qualified_count": 104,
            "qualified_rate": 90.43,
        },
    ]
    trend_result = {
        "columns": [
            "product_spec_id", "month_bucket", "sample_count",
            "qualified_count", "qualified_rate",
        ],
        "rows": trend_rows,
        "row_count": len(trend_rows),
        "truncated": False,
    }

    db.execute_query = AsyncMock(return_value=trend_result)
    return db


def _mock_llm() -> LLMClient:
    """Mock LLM — trend uses template, LLM only for final answer stream."""
    llm = MagicMock(spec=LLMClient)
    llm.chat_json = AsyncMock(return_value={"sql": "", "explanation": ""})

    async def _empty_stream(*a, **kw):
        return
        yield  # noqa: unreachable

    llm.chat_stream = MagicMock(return_value=_empty_stream())
    return llm


# ── E2E Test ───────────────────────────────────────────────


class TestE2ETrendQuery:
    """E2E: trend query via template-based SQL generation."""

    async def test_sse_events_contain_grade_and_sql_structure(self) -> None:
        """SSE events contain ≥1 grade step; SQL has DATE_FORMAT + ORDER BY."""
        db = _mock_db_trend()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_trend_context())

        # Parse all SSE events
        parsed = []
        for ev in events:
            assert ev.startswith("data: "), f"Bad SSE format: {ev[:60]}"
            data = json.loads(ev[len("data: "):].strip())
            parsed.append(data)

        # Assert ≥1 grade step in reasoning_steps
        has_spec_or_condition_or_grade = False
        for ev in parsed:
            if ev.get("type") == "reasoning_step":
                kind = ev.get("reasoning_step", {}).get("kind", "")
                if kind in ("spec", "condition", "grade"):
                    has_spec_or_condition_or_grade = True
        assert has_spec_or_condition_or_grade, (
            "SSE events missing spec/condition/grade step"
        )

        # Assert response_metadata.sql contains required structures
        meta_events = [e for e in parsed if e.get("type") == "response_metadata"]
        assert len(meta_events) >= 1, "Missing response_metadata event"
        sql = meta_events[0].get("response_payload", {}).get("sql", "")
        assert "DATE_FORMAT" in sql, f"SQL missing DATE_FORMAT: {sql[:120]}"
        assert "ORDER BY month_bucket" in sql, (
            f"SQL missing ORDER BY month_bucket: {sql[-80:]}"
        )

    async def test_trend_sql_validates(self) -> None:
        """Template-generated SQL passes validate_sql."""
        db = _mock_db_trend()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        await agent.run(_make_trend_context())

        # validate_sql should have been called at least once
        assert db.validate_sql.called
        for call in db.validate_sql.call_args_list:
            sql_str = call.args[0]
            assert "DATE_FORMAT" in sql_str or "SELECT" in sql_str

    async def test_trend_grade_summary_mentions_window(self) -> None:
        """Grade step summary mentions time window and rate change."""
        db = _mock_db_trend()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_trend_context())

        # Find grade steps
        for ev in events:
            data = json.loads(ev[len("data: "):].strip())
            if data.get("type") == "reasoning_step":
                step = data.get("reasoning_step", {})
                if step.get("kind") == "grade":
                    label = step.get("label", "")
                    assert "近6月" in label, f"Grade label missing window: {label}"
                    assert "趋势" in label, f"Grade label missing 趋势: {label}"
                    return

        pytest.fail("No grade step found in SSE events")
