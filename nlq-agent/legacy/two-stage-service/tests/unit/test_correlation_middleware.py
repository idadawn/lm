"""
CorrelationIDMiddleware 单元测试

测试覆盖：生成新 ID、复用客户端 ID、response header 注入、异常路径。
"""

from __future__ import annotations

from fastapi import FastAPI
from fastapi.testclient import TestClient
from starlette.requests import Request
from starlette.responses import PlainTextResponse

from src.api.middleware import CorrelationIDMiddleware
from src.core.logging_config import get_correlation_id


def _make_app() -> TestClient:
    """创建带 CorrelationIDMiddleware 的最小 FastAPI 应用。"""
    app = FastAPI()
    app.add_middleware(CorrelationIDMiddleware)

    @app.get("/health")
    async def health():
        return {"status": "ok"}

    @app.get("/echo-cid")
    async def echo_cid(request: Request):
        return {"correlation_id": request.state.correlation_id}

    @app.get("/boom")
    async def boom():
        raise RuntimeError("intentional")

    @app.exception_handler(RuntimeError)
    async def runtime_error_handler(request: Request, exc: RuntimeError):
        return PlainTextResponse("boom", status_code=500)

    return TestClient(app)


class TestCorrelationIDMiddleware:
    def test_generates_new_uuid_when_no_header(self):
        """客户端未传 X-Correlation-ID 时，服务端应生成 UUID4。"""
        client = _make_app()
        resp = client.get("/echo-cid")
        assert resp.status_code == 200
        cid = resp.json()["correlation_id"]
        assert len(cid) == 36
        assert cid.count("-") == 4

        # Response header 应包含相同 ID
        assert resp.headers["X-Correlation-ID"] == cid

    def test_reuses_client_provided_id(self):
        """客户端传入 X-Correlation-ID 时，应复用该值。"""
        client = _make_app()
        custom_cid = "my-trace-id-12345"
        resp = client.get("/echo-cid", headers={"X-Correlation-ID": custom_cid})
        assert resp.status_code == 200
        assert resp.json()["correlation_id"] == custom_cid
        assert resp.headers["X-Correlation-ID"] == custom_cid

    def test_response_header_injected_on_all_routes(self):
        """所有路由的响应都应携带 X-Correlation-ID header。"""
        client = _make_app()
        resp = client.get("/health")
        assert resp.status_code == 200
        assert "X-Correlation-ID" in resp.headers
        assert len(resp.headers["X-Correlation-ID"]) == 36

    def test_exception_still_cleans_contextvar(self):
        """中间件内抛异常时，contextvar 应被清理，避免污染后续请求。"""
        client = _make_app()
        # 第一次请求触发异常
        resp = client.get("/boom")
        assert resp.status_code == 500

        # 第二次正常请求应获得新的 correlation_id，而非残留值
        resp = client.get("/echo-cid")
        assert resp.status_code == 200
        cid_after = resp.json()["correlation_id"]
        assert len(cid_after) == 36
        # contextvar 已被清理
        assert get_correlation_id() is None
