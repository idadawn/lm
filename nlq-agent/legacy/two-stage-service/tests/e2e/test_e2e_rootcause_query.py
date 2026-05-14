"""
R8: E2E test — root_cause query (mocked LLM/Qdrant/DB).

Target query: "为什么 50W470 上月铁损合格率下降？"

Verifies:
1. SSE event sequence contains ≥1 spec/condition/grade
2. response_metadata.sql contains GROUP BY F_CREATORUSERID or F_PRODUCT_SPEC_CODE
3. Template-based SQL is valid (passes validate_sql)
4. Grade summary mentions worst performer
"""

from __future__ import annotations

import json
from unittest.mock import AsyncMock, MagicMock

import pytest

from src.models.schemas import (
    AgentContext,
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


def _make_rootcause_context() -> AgentContext:
    """构建根因查询的 AgentContext。"""
    return AgentContext(
        user_question="为什么50W470上月铁损合格率下降",
        intent=IntentClassification(
            intent=IntentType.ROOT_CAUSE,
            confidence=0.9,
            extracted_entities={
                "time_window": 1,
                "metric": "合格率",
                "dimension_keys": ["F_PRODUCT_SPEC_CODE", "F_CREATORUSERID"],
                "product_spec": "50W470",
            },
        ),
        business_explanation="分析50W470产品上月铁损合格率下降的根因",
        filters=[],
        metrics=[
            MetricDefinition(
                name="合格率",
                formula="SUM(qualified)/COUNT(*)*100",
                description="综合合格率",
            ),
        ],
    )


def _mock_db_rootcause() -> DatabaseService:
    """Mock DB returning root cause query results."""
    db = MagicMock(spec=DatabaseService)
    db.validate_sql = MagicMock(return_value=(True, ""))

    rootcause_rows = [
        {
            "dimension_key": "F_PRODUCT_SPEC_CODE",
            "dimension_value": "50W470",
            "sample_count": 120,
            "qualified_rate": 78.5,
            "delta_from_overall": -8.5,
        },
        {
            "dimension_key": "F_PRODUCT_SPEC_CODE",
            "dimension_value": "50W600",
            "sample_count": 95,
            "qualified_rate": 85.0,
            "delta_from_overall": -2.0,
        },
        {
            "dimension_key": "F_PRODUCT_SPEC_CODE",
            "dimension_value": "50W800",
            "sample_count": 110,
            "qualified_rate": 92.0,
            "delta_from_overall": 5.0,
        },
    ]
    rootcause_result = {
        "columns": [
            "dimension_key", "dimension_value", "sample_count",
            "qualified_rate", "delta_from_overall",
        ],
        "rows": rootcause_rows,
        "row_count": len(rootcause_rows),
        "truncated": False,
    }

    db.execute_query = AsyncMock(return_value=rootcause_result)
    return db


def _mock_llm() -> LLMClient:
    """Mock LLM — root cause uses template, LLM only for final answer stream."""
    llm = MagicMock(spec=LLMClient)
    llm.chat_json = AsyncMock(return_value={"sql": "", "explanation": ""})

    async def _empty_stream(*a, **kw):
        return
        yield  # noqa: unreachable

    llm.chat_stream = MagicMock(return_value=_empty_stream())
    return llm


# ── E2E Test ───────────────────────────────────────────────


class TestE2ERootCauseQuery:
    """E2E: root_cause query via template-based SQL generation."""

    async def test_sse_events_contain_grade_and_sql_structure(self) -> None:
        """SSE events contain ≥1 grade step; SQL has GROUP BY dimension."""
        db = _mock_db_rootcause()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_rootcause_context())

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
        assert "GROUP BY" in sql, f"SQL missing GROUP BY: {sql[:120]}"
        assert (
            "F_PRODUCT_SPEC_CODE" in sql or "F_CREATORUSERID" in sql
        ), f"SQL missing dimension column: {sql[-120:]}"

    async def test_rootcause_sql_validates(self) -> None:
        """Template-generated SQL passes validate_sql."""
        db = _mock_db_rootcause()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        await agent.run(_make_rootcause_context())

        # validate_sql should have been called at least once
        assert db.validate_sql.called
        for call in db.validate_sql.call_args_list:
            sql_str = call.args[0]
            assert "SELECT" in sql_str

    async def test_rootcause_grade_summary_mentions_worst(self) -> None:
        """Grade step summary mentions worst performer with rate and delta."""
        db = _mock_db_rootcause()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_rootcause_context())

        # Find grade steps
        for ev in events:
            data = json.loads(ev[len("data: "):].strip())
            if data.get("type") == "reasoning_step":
                step = data.get("reasoning_step", {})
                if step.get("kind") == "grade":
                    label = step.get("label", "")
                    assert "合格率最低" in label, f"Grade label missing 合格率最低: {label}"
                    assert "低于整体" in label, f"Grade label missing 低于整体: {label}"
                    return

        pytest.fail("No grade step found in SSE events")

    async def test_rootcause_row_count_positive(self) -> None:
        """response_metadata contains row_count > 0."""
        db = _mock_db_rootcause()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_rootcause_context())

        for ev in events:
            data = json.loads(ev[len("data: "):].strip())
            if data.get("type") == "response_metadata":
                payload = data.get("response_payload", {})
                assert payload.get("row_count", 0) > 0, "row_count should be > 0"
                return

        pytest.fail("No response_metadata event found")
