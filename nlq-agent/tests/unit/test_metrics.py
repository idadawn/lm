"""
Tests for src/core/metrics.py and /metrics endpoint.

Covers: counter increments, histogram observations,
/metrics endpoint format, and error-path counting.
"""

from __future__ import annotations

import pytest

import src.core.metrics as mod
from src.core.metrics import (
    inc_error_count,
    inc_intent_count,
    observe_stage_duration,
)


class TestCounterIncrements:
    """chat_stream_total counter tracks intent + status."""

    def test_ok_increments_counter(self):
        before = (
            mod.CHAT_STREAM_TOTAL.labels(
                intent_type="statistical", status="ok"
            )._value.get()
        )
        inc_intent_count("statistical", "ok")
        after = (
            mod.CHAT_STREAM_TOTAL.labels(
                intent_type="statistical", status="ok"
            )._value.get()
        )
        assert after == before + 1

    def test_multiple_intents_tracked_separately(self):
        b_stat = mod.CHAT_STREAM_TOTAL.labels(
            intent_type="statistical", status="ok"
        )._value.get()
        b_trend = mod.CHAT_STREAM_TOTAL.labels(
            intent_type="trend", status="ok"
        )._value.get()

        inc_intent_count("statistical", "ok")
        inc_intent_count("trend", "ok")

        assert (
            mod.CHAT_STREAM_TOTAL.labels(
                intent_type="statistical", status="ok"
            )._value.get()
            == b_stat + 1
        )
        assert (
            mod.CHAT_STREAM_TOTAL.labels(
                intent_type="trend", status="ok"
            )._value.get()
            == b_trend + 1
        )


class TestHistogramObserve:
    """observe_stage_duration records on the correct histogram."""

    def test_total_duration_observed(self):
        before_sum = mod.CHAT_STREAM_DURATION_SECONDS._sum.get()

        observe_stage_duration("total", 1.5)

        assert mod.CHAT_STREAM_DURATION_SECONDS._sum.get() == pytest.approx(
            before_sum + 1.5
        )

    def test_multiple_observations_accumulate(self):
        before_sum = mod.CHAT_STREAM_DURATION_SECONDS._sum.get()

        observe_stage_duration("total", 0.5)
        observe_stage_duration("total", 1.0)

        assert mod.CHAT_STREAM_DURATION_SECONDS._sum.get() == pytest.approx(
            before_sum + 1.5
        )


class TestMetricsEndpoint:
    """/metrics endpoint returns Prometheus exposition format."""

    def test_endpoint_returns_prometheus_format(self):
        from prometheus_client import generate_latest

        body = generate_latest().decode()
        assert "chat_stream_total" in body

    def test_content_type_is_prometheus(self):
        from starlette.responses import PlainTextResponse

        resp = PlainTextResponse(
            b"test",
            media_type="text/plain; version=0.0.4; charset=utf-8",
        )
        assert "version=0.0.4" in resp.media_type


class TestErrorPathCounting:
    """Error-path counter tracks error_code labels."""

    def test_error_counter_increments(self):
        b_ve = mod.CHAT_STREAM_ERRORS_TOTAL.labels(
            error_code="ValueError"
        )._value.get()
        b_rt = mod.CHAT_STREAM_ERRORS_TOTAL.labels(
            error_code="RuntimeError"
        )._value.get()

        inc_error_count("ValueError")
        inc_error_count("ValueError")
        inc_error_count("RuntimeError")

        assert (
            mod.CHAT_STREAM_ERRORS_TOTAL.labels(
                error_code="ValueError"
            )._value.get()
            == b_ve + 2
        )
        assert (
            mod.CHAT_STREAM_ERRORS_TOTAL.labels(
                error_code="RuntimeError"
            )._value.get()
            == b_rt + 1
        )

    def test_gauge_inc_dec(self):
        before = mod.ACTIVE_CHAT_STREAMS._value.get()
        mod.ACTIVE_CHAT_STREAMS.inc()
        assert mod.ACTIVE_CHAT_STREAMS._value.get() == before + 1
        mod.ACTIVE_CHAT_STREAMS.inc()
        assert mod.ACTIVE_CHAT_STREAMS._value.get() == before + 2
        mod.ACTIVE_CHAT_STREAMS.dec()
        assert mod.ACTIVE_CHAT_STREAMS._value.get() == before + 1
