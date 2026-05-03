"""
Concurrent stream load tests for POST /api/v1/chat/stream.

All tests carry the ``load`` marker and are excluded from the default
``pytest -m "not live_llm and not live_qdrant"`` run.

Run with::

    pytest tests/load/ -m load -v
"""

from __future__ import annotations

import asyncio
import json
from unittest.mock import AsyncMock, patch

import httpx
import pytest

from src.api.dependencies import get_orchestrator
from src.main import create_app

from .conftest import MockOrchestrator, _build_test_client, parse_sse_events

CHAT_PAYLOAD = {
    "messages": [{"role": "user", "content": "本月合格率是多少？"}],
    "session_id": "load-test",
}


# ── Test helpers ─────────────────────────────────────────────


async def _send_stream(client: httpx.AsyncClient, payload: dict | None = None):
    return await client.post("/api/v1/chat/stream", json=payload or CHAT_PAYLOAD)


# ── Tests ────────────────────────────────────────────────────


@pytest.mark.load
async def test_10_concurrent_streams(mock_full_stack) -> None:
    """10 concurrent chat/stream requests all return 200 with a ``done`` event."""
    responses = await asyncio.gather(
        *[_send_stream(mock_full_stack) for _ in range(10)]
    )

    for resp in responses:
        assert resp.status_code == 200
        events = parse_sse_events(resp.text)
        types = [e.get("type") for e in events]
        assert "done" in types, f"Missing done event, got types: {types}"


@pytest.mark.load
async def test_response_metadata_order_under_load(mock_full_stack) -> None:
    """Under 5 concurrent requests ``response_metadata`` arrives before ``done``."""
    responses = await asyncio.gather(
        *[_send_stream(mock_full_stack) for _ in range(5)]
    )

    for resp in responses:
        assert resp.status_code == 200
        events = parse_sse_events(resp.text)
        types = [e.get("type") for e in events]

        assert "done" in types
        assert "response_metadata" in types
        assert types.index("response_metadata") < types.index("done"), (
            "response_metadata must precede done"
        )
        # ``done`` is the very last event
        assert types[-1] == "done", f"Last event is {types[-1]}, expected done"


@pytest.mark.load
async def test_no_event_loss() -> None:
    """A single stream with 100 reasoning_steps delivers all of them."""
    with (
        patch("src.main.init_services", new_callable=AsyncMock),
        patch("src.main.shutdown_services", new_callable=AsyncMock),
    ):
        client = _build_test_client(MockOrchestrator(n_reasoning_steps=100))
        async with client:
            resp = await _send_stream(client)

    assert resp.status_code == 200
    events = parse_sse_events(resp.text)
    reasoning_events = [e for e in events if e.get("type") == "reasoning_step"]
    assert len(reasoning_events) == 100, (
        f"Expected 100 reasoning_step events, got {len(reasoning_events)}"
    )


@pytest.mark.load
async def test_rate_limit_under_burst() -> None:
    """50 concurrent requests from the same IP: >30 should be rejected (429)."""
    with (
        patch("src.main.init_services", new_callable=AsyncMock),
        patch("src.main.shutdown_services", new_callable=AsyncMock),
    ):
        # Fresh app → fresh rate limiter (30 req/min default)
        client = _build_test_client(MockOrchestrator())
        async with client:
            responses = await asyncio.gather(
                *[_send_stream(client) for _ in range(50)]
            )

    status_codes = [r.status_code for r in responses]
    ok_count = status_codes.count(200)
    rejected_count = status_codes.count(429)

    assert ok_count <= 30, f"Expected ≤200 OK, got {ok_count}"
    assert rejected_count >= 20, (
        f"Expected ≥20 rejections (429), got {rejected_count}; "
        f"ok={ok_count}, total={len(status_codes)}"
    )
