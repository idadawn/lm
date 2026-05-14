"""
Prometheus metrics for nlq-agent.

Counters, histograms, gauges, and helper utilities for monitoring
chat stream throughput, latency, and error rates.
"""

from __future__ import annotations

import functools
import time
from typing import AsyncIterator, Callable

from prometheus_client import Counter, Gauge, Histogram

# ── Metrics ──────────────────────────────────────────────────────

CHAT_STREAM_TOTAL = Counter(
    "chat_stream_total",
    "Total number of chat stream requests",
    ["intent_type", "status"],
)

CHAT_STREAM_ERRORS_TOTAL = Counter(
    "chat_stream_errors_total",
    "Total number of chat stream errors by error code",
    ["error_code"],
)

STAGE1_DURATION_SECONDS = Histogram(
    "stage1_duration_seconds",
    "Stage 1 (semantic KG) duration in seconds",
    ["intent_type"],
    buckets=(0.1, 0.25, 0.5, 1.0, 2.5, 5.0, 10.0, 30.0),
)

STAGE2_SQL_DURATION_SECONDS = Histogram(
    "stage2_sql_duration_seconds",
    "Stage 2 (SQL generation + execution) duration in seconds",
    buckets=(0.1, 0.25, 0.5, 1.0, 2.5, 5.0, 10.0, 30.0),
)

CHAT_STREAM_DURATION_SECONDS = Histogram(
    "chat_stream_duration_seconds",
    "End-to-end chat stream duration in seconds",
    buckets=(0.5, 1.0, 2.5, 5.0, 10.0, 30.0, 60.0, 120.0),
)

ACTIVE_CHAT_STREAMS = Gauge(
    "active_chat_streams",
    "Number of currently active chat streams",
)


# ── Helpers ──────────────────────────────────────────────────────

def inc_intent_count(intent: str, status: str) -> None:
    """Increment chat_stream_total counter."""
    CHAT_STREAM_TOTAL.labels(intent_type=intent, status=status).inc()


def inc_error_count(error_code: str) -> None:
    """Increment chat_stream_errors_total counter."""
    CHAT_STREAM_ERRORS_TOTAL.labels(error_code=error_code).inc()


def observe_stage_duration(stage: str, duration: float, **labels: str) -> None:
    """Observe a stage duration on the appropriate histogram."""
    if stage == "stage1":
        STAGE1_DURATION_SECONDS.labels(**labels).observe(duration)
    elif stage == "stage2":
        STAGE2_SQL_DURATION_SECONDS.observe(duration)
    elif stage == "total":
        CHAT_STREAM_DURATION_SECONDS.observe(duration)


def track_chat_stream_duration(func: Callable) -> Callable:
    """
    Async decorator that wraps a stream_chat-like coroutine.

    Tracks active concurrency, end-to-end duration, and error counts.
    The wrapped function must be an async generator.
    """

    @functools.wraps(func)
    async def wrapper(*args, **kwargs):
        ACTIVE_CHAT_STREAMS.inc()
        start = time.monotonic()
        intent_type = "unknown"
        try:
            async for event in func(*args, **kwargs):
                yield event
            status = "ok"
        except Exception as exc:
            status = "error"
            error_code = type(exc).__name__
            inc_error_count(error_code)
            raise
        finally:
            elapsed = time.monotonic() - start
            ACTIVE_CHAT_STREAMS.dec()
            inc_intent_count(intent_type, status)
            observe_stage_duration("total", elapsed)

    return wrapper
