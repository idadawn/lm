"""
Middleware unit tests.

Tests for RequestSizeLimit, QueryLengthGuard, and RateLimitInMem middleware.
Each middleware is tested in isolation with its own FastAPI app instance.
"""

from __future__ import annotations

from fastapi import FastAPI
from fastapi.testclient import TestClient

from src.api.middleware import QueryLengthGuard, RateLimitInMem, RequestSizeLimit


def _make_app(*middleware_configs) -> TestClient:
    """Create a minimal FastAPI app with the given middleware stack."""
    app = FastAPI()

    @app.post("/api/v1/chat/stream")
    async def chat():
        return {"status": "ok"}

    @app.get("/health")
    async def health():
        return {"status": "ok"}

    for mw_cls, kwargs in middleware_configs:
        app.add_middleware(mw_cls, **kwargs)

    return TestClient(app)


# ── RequestSizeLimit ────────────────────────────────────────


class TestRequestSizeLimit:
    def test_small_body_passes(self):
        client = _make_app((RequestSizeLimit, {"max_bytes": 100}))
        resp = client.post(
            "/api/v1/chat/stream",
            json={"messages": [{"role": "user", "content": "hi"}]},
        )
        assert resp.status_code == 200

    def test_oversized_body_rejected(self):
        client = _make_app((RequestSizeLimit, {"max_bytes": 50}))
        resp = client.post(
            "/api/v1/chat/stream",
            json={"messages": [{"role": "user", "content": "x" * 100}]},
        )
        assert resp.status_code == 413
        body = resp.json()
        assert body["error"]["code"] == "PAYLOAD_TOO_LARGE"
        assert "message" in body["error"]

    def test_get_skips_size_check(self):
        """GET 请求不检查 body 大小。"""
        client = _make_app((RequestSizeLimit, {"max_bytes": 1}))
        resp = client.get("/health")
        assert resp.status_code == 200


# ── QueryLengthGuard ────────────────────────────────────────


class TestQueryLengthGuard:
    def test_short_query_passes(self):
        client = _make_app((QueryLengthGuard, {"max_chars": 20}))
        resp = client.post(
            "/api/v1/chat/stream",
            json={"messages": [{"role": "user", "content": "hello"}]},
        )
        assert resp.status_code == 200

    def test_long_query_rejected(self):
        client = _make_app((QueryLengthGuard, {"max_chars": 5}))
        resp = client.post(
            "/api/v1/chat/stream",
            json={"messages": [{"role": "user", "content": "this is way too long"}]},
        )
        assert resp.status_code == 422
        body = resp.json()
        assert body["error"]["code"] == "QUERY_TOO_LONG"

    def test_health_exempt(self):
        """/health 路径豁免查询长度检查。"""
        client = _make_app((QueryLengthGuard, {"max_chars": 1}))
        resp = client.get("/health")
        assert resp.status_code == 200


# ── RateLimitInMem ──────────────────────────────────────────


class TestRateLimitInMem:
    def test_under_limit_passes(self):
        client = _make_app(
            (RateLimitInMem, {"max_requests": 5, "window_seconds": 60})
        )
        for _ in range(5):
            resp = client.post(
                "/api/v1/chat/stream",
                json={"messages": [{"role": "user", "content": "hi"}]},
            )
            assert resp.status_code == 200

    def test_over_limit_rejected(self):
        client = _make_app(
            (RateLimitInMem, {"max_requests": 2, "window_seconds": 60})
        )
        for _ in range(2):
            client.post("/api/v1/chat/stream", json={})

        resp = client.post("/api/v1/chat/stream", json={})
        assert resp.status_code == 429
        assert resp.json()["error"]["code"] == "RATE_LIMITED"

    def test_health_exempt_from_rate_limit(self):
        """/health 不消耗令牌，限流后仍可访问。"""
        client = _make_app(
            (RateLimitInMem, {"max_requests": 1, "window_seconds": 60})
        )
        # 用掉唯一的令牌
        resp = client.post("/api/v1/chat/stream", json={})
        assert resp.status_code == 200

        # /health 豁免，仍可访问
        resp = client.get("/health")
        assert resp.status_code == 200

        # POST 已耗尽，应被限流
        resp = client.post("/api/v1/chat/stream", json={})
        assert resp.status_code == 429
