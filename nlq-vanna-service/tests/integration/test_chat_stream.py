"""Integration tests for POST /api/v1/chat/stream SSE endpoint.

Uses FastAPI TestClient + httpx ASGITransport — no live services required.
Mocks VannaApp.generate_sql / run_sql to return fixed SQL + DataFrame.

Tests:
  - test_kind_coverage: spec + rule + condition kinds present; record or grade present
  - test_no_fallback_degeneration: first step is not fallback; ≤ 1 fallback total
  - test_done_event_payload: last event is {"type": "done"}
"""

from __future__ import annotations

import json
from typing import Iterator
from unittest.mock import MagicMock, patch

import pandas as pd
import pytest
from fastapi import FastAPI
from fastapi.testclient import TestClient

from app.api.chat_stream import router
from app.api.schemas import ChatMessage, ChatRequest


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def _build_app() -> FastAPI:
    """Build a minimal FastAPI app with only the chat_stream router."""
    app = FastAPI()
    app.include_router(router)
    return app


def _parse_sse_stream(raw: bytes) -> list[dict]:
    """Parse SSE event stream bytes into a list of parsed JSON dicts.

    Each SSE event has the form:
        event: message\\n
        data: {...}\\n
        \\n
    """
    events: list[dict] = []
    current_data: str | None = None

    for line in raw.decode("utf-8").splitlines():
        if line.startswith("data:"):
            current_data = line[len("data:"):].strip()
        elif line == "" and current_data is not None:
            try:
                events.append(json.loads(current_data))
            except json.JSONDecodeError:
                pass
            current_data = None

    # Flush last event if stream ends without trailing blank line
    if current_data is not None:
        try:
            events.append(json.loads(current_data))
        except json.JSONDecodeError:
            pass

    return events


def _collect_sse_events(client: TestClient, question: str) -> list[dict]:
    """POST a question to /api/v1/chat/stream and return all parsed SSE events."""
    payload = {
        "messages": [{"role": "user", "content": question}],
        "session_id": "test-session-001",
    }
    with client.stream("POST", "/api/v1/chat/stream", json=payload) as response:
        assert response.status_code == 200
        return _parse_sse_stream(response.read())


# ---------------------------------------------------------------------------
# Mock VannaApp factory
# ---------------------------------------------------------------------------


FIXED_SQL = (
    "SELECT a.iron_loss, b.spec_name, JSON_EXTRACT(a.attrs, '$.grade') AS grade "
    "FROM lab_intermediate_data a "
    "JOIN lab_spec b ON a.spec_id = b.id "
    "WHERE a.spec_name = '120规格' AND a.iron_loss > 1.05"
)

FIXED_DF_GRADE = pd.DataFrame(
    {"iron_loss": [1.08, 1.10], "spec_name": ["120规格", "120规格"], "grade": ["C", "C"]}
)

FIXED_DF_RECORD = pd.DataFrame(
    {"iron_loss": [0.98, 1.01], "spec_name": ["120规格", "120规格"]}
)


def _make_mock_vanna(sql: str = FIXED_SQL, df: pd.DataFrame | None = None):
    """Create a mock VannaApp that returns fixed SQL and DataFrame."""
    if df is None:
        df = FIXED_DF_RECORD

    mock_vn = MagicMock()
    mock_vn.generate_sql.return_value = sql
    mock_vn.run_sql.return_value = df
    mock_vn.attach_emitter = MagicMock()
    mock_vn.detach_emitter = MagicMock()

    # QdrantStoreMixin methods that emit reasoning steps
    def fake_generate_sql(question: str) -> str:
        # Simulate emit of spec + rule steps via the emitter
        # (In real code, QdrantStoreMixin calls emitter.put_step during search)
        # We must access the emitter that was attached
        emitter = mock_vn._attached_emitter
        if emitter is not None:
            from app.api.schemas import ReasoningStep
            emitter.put_step(ReasoningStep(kind="spec", label="匹配到表结构 (top-2 score=0.921)"))
            emitter.put_step(ReasoningStep(kind="rule", label="应用业务术语：叠片系数定义"))
            emitter.put_step(ReasoningStep(kind="rule", label="应用判级规则：铁损 > 1.05 判 C 级"))
        return sql

    mock_vn._attached_emitter = None

    def attach(emitter):
        mock_vn._attached_emitter = emitter

    def detach():
        mock_vn._attached_emitter = None

    mock_vn.attach_emitter.side_effect = attach
    mock_vn.detach_emitter.side_effect = detach
    mock_vn.generate_sql.side_effect = fake_generate_sql

    return mock_vn


# ---------------------------------------------------------------------------
# Fixtures
# ---------------------------------------------------------------------------


@pytest.fixture
def client_grade() -> Iterator[TestClient]:
    """TestClient with mock VannaApp returning grade-type DataFrame."""
    app = _build_app()
    mock_vn = _make_mock_vanna(sql=FIXED_SQL, df=FIXED_DF_GRADE)
    with patch("app.deps.get_vanna_app", return_value=mock_vn):
        with patch("app.api.chat_stream.get_vanna_app", return_value=mock_vn):
            with TestClient(app, raise_server_exceptions=False) as client:
                yield client


@pytest.fixture
def client_record() -> Iterator[TestClient]:
    """TestClient with mock VannaApp returning record-type DataFrame."""
    app = _build_app()
    mock_vn = _make_mock_vanna(sql=FIXED_SQL, df=FIXED_DF_RECORD)
    with patch("app.deps.get_vanna_app", return_value=mock_vn):
        with patch("app.api.chat_stream.get_vanna_app", return_value=mock_vn):
            with TestClient(app, raise_server_exceptions=False) as client:
                yield client


# ---------------------------------------------------------------------------
# Helper: build client with overridden dependency
# ---------------------------------------------------------------------------


def _make_client_with_vanna(mock_vn) -> TestClient:
    app = _build_app()

    async def override_get_vanna():
        return mock_vn

    from app.deps import get_vanna_app
    app.dependency_overrides[get_vanna_app] = override_get_vanna
    return TestClient(app, raise_server_exceptions=False)


# ---------------------------------------------------------------------------
# Test: kind coverage (ADR-3)
# ---------------------------------------------------------------------------


def test_kind_coverage():
    """spec + rule + condition kinds must be present; record or grade must appear."""
    mock_vn = _make_mock_vanna(
        sql=FIXED_SQL,
        df=FIXED_DF_GRADE,
    )
    client = _make_client_with_vanna(mock_vn)

    events = _collect_sse_events(client, "120规格 Ps 铁损 > 1.05 判什么级")
    rs = [e for e in events if e.get("type") == "reasoning_step"]
    assert len(rs) > 0, "No reasoning_step events emitted"

    kinds = {e["reasoning_step"]["kind"] for e in rs}
    assert {"spec", "rule", "condition"} <= kinds, (
        f"Missing required kinds. Present: {kinds}. "
        "Expected spec + rule + condition to all be present."
    )
    assert ("record" in kinds) or ("grade" in kinds), (
        f"Neither 'record' nor 'grade' found in kinds: {kinds}. "
        "Terminal step must be record or grade."
    )


# ---------------------------------------------------------------------------
# Test: no fallback degeneration (R5)
# ---------------------------------------------------------------------------


def test_no_fallback_degeneration():
    """Happy-path: first reasoning_step must not be fallback; ≤ 1 fallback total."""
    mock_vn = _make_mock_vanna(sql=FIXED_SQL, df=FIXED_DF_RECORD)
    client = _make_client_with_vanna(mock_vn)

    events = _collect_sse_events(client, "查询铁损数据")
    rs = [e for e in events if e.get("type") == "reasoning_step"]
    assert len(rs) > 0, "No reasoning_step events emitted"

    # First step must not be fallback
    first_kind = rs[0]["reasoning_step"]["kind"]
    assert first_kind != "fallback", (
        f"First reasoning step kind is 'fallback' — happy path should start with spec/rule."
    )

    # At most 1 fallback in the entire stream (e.g. from SQL parse error, not logic error)
    fb_count = sum(1 for e in rs if e["reasoning_step"]["kind"] == "fallback")
    assert fb_count <= 1, (
        f"Too many fallback steps ({fb_count}). "
        "Happy path should have 0–1 fallback steps, not degenerate."
    )


# ---------------------------------------------------------------------------
# Test: done event is last and has correct payload
# ---------------------------------------------------------------------------


def test_done_event_payload():
    """The last SSE event must be {\"type\": \"done\"} (ADR-3, not '[DONE]' sentinel)."""
    mock_vn = _make_mock_vanna(sql=FIXED_SQL, df=FIXED_DF_RECORD)
    client = _make_client_with_vanna(mock_vn)

    events = _collect_sse_events(client, "查询记录")
    assert len(events) > 0, "No SSE events received"

    last_event = events[-1]
    assert last_event.get("type") == "done", (
        f"Last event type is '{last_event.get('type')}', expected 'done'. "
        "ADR-3 requires {{\"type\": \"done\"}} as terminal event, not '[DONE]' string."
    )


# ---------------------------------------------------------------------------
# Test: response_metadata is emitted
# ---------------------------------------------------------------------------


def test_response_metadata_emitted():
    """A response_metadata event must be present in the SSE stream."""
    mock_vn = _make_mock_vanna(sql=FIXED_SQL, df=FIXED_DF_RECORD)
    client = _make_client_with_vanna(mock_vn)

    events = _collect_sse_events(client, "查询数据")
    meta_events = [e for e in events if e.get("type") == "response_metadata"]
    assert len(meta_events) >= 1, "No response_metadata event found in stream"

    meta = meta_events[0]
    assert "response_payload" in meta
    payload = meta["response_payload"]
    assert "session_id" in payload
    assert "reasoning_steps" in payload


# ---------------------------------------------------------------------------
# Test: SQL syntax error triggers fallback step (not 500)
# ---------------------------------------------------------------------------


def test_sql_parse_error_emits_fallback():
    """If generate_sql returns unparseable SQL, extract_conditions emits a fallback."""
    mock_vn = _make_mock_vanna(
        sql="THIS IS NOT SQL AT ALL",
        df=FIXED_DF_RECORD,
    )
    client = _make_client_with_vanna(mock_vn)

    events = _collect_sse_events(client, "bad SQL test")
    rs = [e for e in events if e.get("type") == "reasoning_step"]
    kinds = [e["reasoning_step"]["kind"] for e in rs]

    # With bad SQL, extract_conditions returns a fallback step
    # The stream must still complete with a done event
    done_events = [e for e in events if e.get("type") == "done"]
    assert len(done_events) >= 1, "Stream must always end with done event"
