#!/usr/bin/env python3
"""
Benchmark script for nlq-agent SSE stream endpoint.

Mocks all external services (LLM / Qdrant / DB) and measures throughput
and latency percentiles under concurrent load.

Usage::

    python -m scripts.benchmark --concurrency 20 --requests 200 --output benchmark.md
"""

from __future__ import annotations

import argparse
import asyncio
import json
import statistics
import time
from datetime import datetime, timezone
from typing import AsyncIterator

import httpx
from unittest.mock import AsyncMock, patch

from src.api.dependencies import get_orchestrator
from src.main import create_app
from src.models.schemas import ChatRequest

# ── Query templates (rotate across requests) ────────────────

QUERY_TEMPLATES = [
    {"messages": [{"role": "user", "content": "本月合格率是多少？"}]},
    {"messages": [{"role": "user", "content": "最近6个月每月的产品合格率变化趋势"}]},
    {"messages": [{"role": "user", "content": "为什么50W470铁损偏高？"}]},
    {"messages": [{"role": "user", "content": "什么是铁损P17/50？"}]},
    {"messages": [{"role": "user", "content": "早班和中班的合格率对比"}]},
]


# ── Mock orchestrator ────────────────────────────────────────


def _sse_event(data: dict) -> str:
    return f"data: {json.dumps(data, ensure_ascii=False)}\n\n"


class BenchmarkOrchestrator:
    """Deterministic mock orchestrator with variable latency simulation."""

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


# ── Benchmark runner ─────────────────────────────────────────


async def _single_request(client: httpx.AsyncClient, payload: dict) -> tuple[int, float]:
    """Send one request, return (status_code, elapsed_seconds)."""
    start = time.monotonic()
    resp = await client.post("/api/v1/chat/stream", json=payload)
    elapsed = time.monotonic() - start
    return resp.status_code, elapsed


async def run_benchmark(concurrency: int, num_requests: int, output: str) -> None:
    sem = asyncio.Semaphore(concurrency)

    async def bounded_request(
        client: httpx.AsyncClient, payload: dict
    ) -> tuple[int, float]:
        async with sem:
            return await _single_request(client, payload)

    with (
        patch("src.main.init_services", new_callable=AsyncMock),
        patch("src.main.shutdown_services", new_callable=AsyncMock),
    ):
        app = create_app()
        app.dependency_overrides[get_orchestrator] = lambda: BenchmarkOrchestrator()
        transport = httpx.ASGITransport(app=app)
        async with httpx.AsyncClient(transport=transport, base_url="http://test") as client:
            wall_start = time.monotonic()
            tasks = [
                bounded_request(client, QUERY_TEMPLATES[i % len(QUERY_TEMPLATES)])
                for i in range(num_requests)
            ]
            results = await asyncio.gather(*tasks)
            wall_elapsed = time.monotonic() - wall_start
        app.dependency_overrides.clear()

    # ── Compute statistics ───────────────────────────────────
    latencies = [r[1] for r in results]
    statuses = [r[0] for r in results]
    ok_count = statuses.count(200)
    error_count = num_requests - ok_count

    sorted_lat = sorted(latencies)
    p50 = sorted_lat[int(len(sorted_lat) * 0.50)]
    p95 = sorted_lat[int(len(sorted_lat) * 0.95)]
    p99 = sorted_lat[int(len(sorted_lat) * 0.99)]
    throughput = num_requests / wall_elapsed if wall_elapsed > 0 else 0

    # ── Generate markdown report ─────────────────────────────
    ts = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    report = (
        f"# nlq-agent Benchmark Report\n"
        f"\n"
        f"| Metric | Value |\n"
        f"|--------|-------|\n"
        f"| Timestamp | {ts} |\n"
        f"| Total requests | {num_requests} |\n"
        f"| Concurrency | {concurrency} |\n"
        f"| Wall time | {wall_elapsed:.3f}s |\n"
        f"| Throughput | {throughput:.1f} req/s |\n"
        f"| OK (200) | {ok_count} |\n"
        f"| Errors | {error_count} |\n"
        f"| Error rate | {error_count / num_requests * 100:.1f}% |\n"
        f"| Latency p50 | {p50 * 1000:.1f} ms |\n"
        f"| Latency p95 | {p95 * 1000:.1f} ms |\n"
        f"| Latency p99 | {p99 * 1000:.1f} ms |\n"
        f"| Latency min | {min(latencies) * 1000:.1f} ms |\n"
        f"| Latency max | {max(latencies) * 1000:.1f} ms |\n"
        f"| Latency mean | {statistics.mean(latencies) * 1000:.1f} ms |\n"
        f"| Latency stdev | {statistics.stdev(latencies) * 1000:.1f} ms |\n"
        f"\n"
        f"## Query Templates\n"
        f"\n"
    )
    for i, tmpl in enumerate(QUERY_TEMPLATES):
        q = tmpl["messages"][0]["content"]
        report += f"{i + 1}. {q}\n"

    report += f"\n> Generated by `python -m scripts.benchmark`\n"

    with open(output, "w") as f:
        f.write(report)

    print(report)
    print(f"\nReport written to {output}")


# ── CLI entry point ──────────────────────────────────────────


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Benchmark nlq-agent SSE stream endpoint (mocked services)"
    )
    parser.add_argument(
        "--concurrency", type=int, default=20, help="Max concurrent requests (default: 20)"
    )
    parser.add_argument(
        "--requests", type=int, default=200, help="Total number of requests (default: 200)"
    )
    parser.add_argument(
        "--output",
        default=None,
        help="Output markdown file (default: benchmark_<timestamp>.md)",
    )
    args = parser.parse_args()

    if args.output is None:
        ts = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
        args.output = f"benchmark_{ts}.md"

    asyncio.run(run_benchmark(args.concurrency, args.requests, args.output))


if __name__ == "__main__":
    main()
