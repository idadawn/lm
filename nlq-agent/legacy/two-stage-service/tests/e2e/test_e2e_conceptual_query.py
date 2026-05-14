"""
C6: E2E test — conceptual query (mocked LLM/Qdrant/DB).

Target query: "什么是铁损 P17/50？"

Verifies:
1. SSE event sequence contains text events (no SQL execution)
2. response_metadata.sql is None
3. response_metadata.row_count is 0
4. Text stream contains retrieved docs content
5. DB validate_sql and execute_query are NOT called
"""

from __future__ import annotations

import json
from unittest.mock import AsyncMock, MagicMock

import pytest

from src.models.schemas import (
    AgentContext,
    IntentClassification,
    IntentType,
)
from src.pipelines.stage2.data_sql_agent import DataSQLAgent
from src.services.database import DatabaseService
from src.services.llm_client import LLMClient
from src.services.sse_emitter import SSEEmitter


# ── Fixtures ───────────────────────────────────────────────


def _make_conceptual_context() -> AgentContext:
    """构建概念解释查询的 AgentContext。"""
    return AgentContext(
        user_question="什么是铁损 P17/50？",
        intent=IntentClassification(
            intent=IntentType.CONCEPTUAL,
            confidence=0.9,
            extracted_entities={},
        ),
        business_explanation="铁损 P17/50 是指在 1.7T 磁通密度、50Hz 频率下的比总损耗",
        filters=[],
        metrics=[],
        retrieved_documents=[
            {
                "doc_id": "spec_ps_loss",
                "text": "铁损（Core Loss）P17/50 是指硅钢片在磁通密度 1.7T、频率 50Hz 条件下的单位重量损耗，单位为 W/kg。",
                "score": 0.95,
                "collection": "specs",
            },
            {
                "doc_id": "rule_ps_loss_threshold",
                "text": "50W470 规格铁损 P17/50 判定标准：≤ 4.70 W/kg 为合格。",
                "score": 0.88,
                "collection": "rules",
            },
        ],
    )


def _mock_db_conceptual() -> DatabaseService:
    """Mock DB — conceptual queries should NOT touch the DB."""
    db = MagicMock(spec=DatabaseService)
    db.validate_sql = MagicMock(return_value=(True, ""))
    db.execute_query = AsyncMock(return_value={
        "columns": [],
        "rows": [],
        "row_count": 0,
        "truncated": False,
    })
    return db


def _mock_llm_conceptual() -> LLMClient:
    """Mock LLM — streams conceptual answer text."""
    llm = MagicMock(spec=LLMClient)
    llm.chat_json = AsyncMock(return_value={"sql": "", "explanation": ""})

    async def _conceptual_stream(*a, **kw):
        chunks = [
            "铁损 P17/50 是指硅钢片在磁通密度 1.7T、频率 50Hz 条件下的单位重量损耗，",
            "单位为 W/kg。",
            "\n\n50W470 规格铁损 P17/50 判定标准：≤ 4.70 W/kg 为合格。",
        ]
        for chunk in chunks:
            yield chunk

    llm.chat_stream = MagicMock(return_value=_conceptual_stream())
    return llm


# ── E2E Test ───────────────────────────────────────────────


class TestE2EConceptualQuery:
    """E2E: conceptual query via no-SQL path."""

    async def test_sse_events_contain_text_no_sql(self) -> None:
        """SSE events contain text; no SQL execution happens."""
        db = _mock_db_conceptual()
        llm = _mock_llm_conceptual()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_conceptual_context())

        # Parse all SSE events
        parsed = []
        for ev in events:
            assert ev.startswith("data: "), f"Bad SSE format: {ev[:60]}"
            data = json.loads(ev[len("data: "):].strip())
            parsed.append(data)

        # Assert text events exist
        text_events = [e for e in parsed if e.get("type") == "text"]
        assert len(text_events) >= 1, "SSE events missing text events"

        # Assert NO condition or grade steps (no SQL path)
        for ev in parsed:
            if ev.get("type") == "reasoning_step":
                kind = ev.get("reasoning_step", {}).get("kind", "")
                assert kind not in ("condition", "grade"), (
                    f"Unexpected {kind} step in conceptual query"
                )

        # DB should NOT have been called
        assert not db.validate_sql.called, (
            "validate_sql should not be called for conceptual query"
        )
        assert not db.execute_query.called, (
            "execute_query should not be called for conceptual query"
        )

    async def test_response_metadata_sql_is_none(self) -> None:
        """Response metadata shows sql=None, row_count=0."""
        db = _mock_db_conceptual()
        llm = _mock_llm_conceptual()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_conceptual_context())

        for ev in events:
            data = json.loads(ev[len("data: "):].strip())
            if data.get("type") == "response_metadata":
                payload = data.get("response_payload", {})
                assert payload.get("sql") is None, (
                    f"Expected sql=None, got {payload.get('sql')}"
                )
                assert payload.get("row_count") == 0, (
                    f"Expected row_count=0, got {payload.get('row_count')}"
                )
                assert payload.get("sql_explanation") == "conceptual query, no SQL", (
                    f"Unexpected sql_explanation: {payload.get('sql_explanation')}"
                )
                return

        pytest.fail("No response_metadata event found")

    async def test_text_stream_contains_retrieved_docs(self) -> None:
        """Text stream contains content from retrieved documents."""
        db = _mock_db_conceptual()
        llm = _mock_llm_conceptual()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_conceptual_context())

        all_text = ""
        for ev in events:
            data = json.loads(ev[len("data: "):].strip())
            if data.get("type") == "text":
                all_text += data.get("content", "")

        # Text should contain concepts from retrieved docs
        assert "铁损" in all_text, "Text missing '铁损'"
        assert "W/kg" in all_text, "Text missing 'W/kg'"

    async def test_no_grade_step_emitted(self) -> None:
        """No grade step is emitted for conceptual queries."""
        db = _mock_db_conceptual()
        llm = _mock_llm_conceptual()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        events = await agent.run(_make_conceptual_context())

        for ev in events:
            data = json.loads(ev[len("data: "):].strip())
            if data.get("type") == "reasoning_step":
                kind = data.get("reasoning_step", {}).get("kind", "")
                assert kind != "grade", "Grade step should not appear in conceptual query"
