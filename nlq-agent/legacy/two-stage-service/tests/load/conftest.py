"""
Load test fixtures.

Provides ``mock_full_stack`` — an httpx.AsyncClient pointed at the real FastAPI
app with all external services (LLM, Qdrant, DB) mocked out via a
MockOrchestrator that produces deterministic SSE events.
"""

from __future__ import annotations

import json
from typing import AsyncIterator
from unittest.mock import AsyncMock, patch

import httpx
import pytest

from src.api.dependencies import get_orchestrator
from src.main import create_app
from src.models.schemas import ChatRequest


# ── Helpers ──────────────────────────────────────────────────


def _sse_event(data: dict) -> str:
    return f"data: {json.dumps(data, ensure_ascii=False)}\n\n"


def parse_sse_events(body: str) -> list[dict]:
    """Parse concatenated SSE ``data: {...}`` blocks into a list of dicts."""
    events: list[dict] = []
    for block in body.strip().split("\n\n"):
        block = block.strip()
        if block.startswith("data: "):
            events.append(json.loads(block[6:]))
    return events


# ── Mock orchestrator ────────────────────────────────────────


class MockOrchestrator:
    """Deterministic orchestrator that emits *n* reasoning steps + text + done."""

    def __init__(self, n_reasoning_steps: int = 2, text_content: str = "模拟回复") -> None:
        self.n_reasoning_steps = n_reasoning_steps
        self.text_content = text_content

    async def stream_chat(self, request: ChatRequest) -> AsyncIterator[str]:
        for i in range(self.n_reasoning_steps):
            yield _sse_event({
                "type": "reasoning_step",
                "reasoning_step": {
                    "kind": "spec",
                    "label": f"推理步骤 {i + 1}",
                    "detail": f"详情 {i + 1}",
                },
            })
        yield _sse_event({"type": "text", "content": self.text_content})
        yield _sse_event({
            "type": "response_metadata",
            "response_payload": {"sql": "SELECT 1", "row_count": 0},
        })
        yield _sse_event({"type": "done"})


def _build_test_client(orchestrator: MockOrchestrator) -> httpx.AsyncClient:
    """Create an ``AsyncClient`` whose app has mocked services and the given orchestrator."""
    app = create_app()
    app.dependency_overrides[get_orchestrator] = lambda: orchestrator
    transport = httpx.ASGITransport(app=app)
    return httpx.AsyncClient(transport=transport, base_url="http://test")


# ── Fixtures ─────────────────────────────────────────────────


@pytest.fixture
async def mock_full_stack():
    """httpx client with full mock stack (mock LLM + Qdrant + DB)."""
    with (
        patch("src.main.init_services", new_callable=AsyncMock),
        patch("src.main.shutdown_services", new_callable=AsyncMock),
    ):
        client = _build_test_client(MockOrchestrator())
        async with client:
            yield client
