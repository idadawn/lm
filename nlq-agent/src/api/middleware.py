"""
请求安全中间件

提供四层请求防护：
- CorrelationIDMiddleware: 生成/复用请求追踪 ID（注入 response header 与日志上下文）
- RequestSizeLimit: 请求体大小限制（仅检查 Content-Length 头）
- QueryLengthGuard: 查询文本长度守卫（读取 body，检查 messages[-1].content）
- RateLimitInMem: 内存速率限制（每 IP token bucket）
"""

from __future__ import annotations

import json
import logging
import time
import uuid
from collections import defaultdict
from dataclasses import dataclass, field

from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request
from starlette.responses import JSONResponse

from src.core.logging_config import bind_correlation_id, get_correlation_id

logger = logging.getLogger(__name__)

EXEMPT_PATHS = frozenset({"/health", "/docs", "/openapi.json", "/redoc"})


def _error_json(status_code: int, code: str, message: str) -> JSONResponse:
    """统一错误响应格式。"""
    return JSONResponse(
        status_code=status_code,
        content={"error": {"code": code, "message": message}},
    )


class CorrelationIDMiddleware(BaseHTTPMiddleware):
    """为每个请求生成或复用 correlation_id，注入 response header 与日志上下文。

    优先级：
    1. 客户端传入 X-Correlation-ID header → 复用
    2. 服务端生成 UUID4 → 新建
    """

    async def dispatch(self, request: Request, call_next):
        cid = request.headers.get("x-correlation-id")
        if not cid:
            cid = str(uuid.uuid4())

        request.state.correlation_id = cid
        bind_correlation_id(cid)

        try:
            response = await call_next(request)
        except Exception:
            # 异常时也需要重置 contextvar，避免污染后续请求
            bind_correlation_id(None)
            raise

        response.headers["X-Correlation-ID"] = cid
        bind_correlation_id(None)
        return response


class RequestSizeLimit(BaseHTTPMiddleware):
    """拒绝超大请求体（默认 100 KB）。仅检查 Content-Length 头。"""

    def __init__(self, app, max_bytes: int = 100 * 1024):
        super().__init__(app)
        self.max_bytes = max_bytes

    async def dispatch(self, request: Request, call_next):
        if request.method in ("POST", "PUT", "PATCH"):
            cl = request.headers.get("content-length")
            if cl is not None:
                try:
                    if int(cl) > self.max_bytes:
                        return _error_json(
                            413,
                            "PAYLOAD_TOO_LARGE",
                            f"Request body exceeds {self.max_bytes} byte limit",
                        )
                except (ValueError, AttributeError):
                    pass
        return await call_next(request)


class QueryLengthGuard(BaseHTTPMiddleware):
    """拒绝超长查询文本（默认 2000 字符）。

    仅对含 messages 字段的 POST 请求生效；
    /health 等路径自动豁免。
    """

    def __init__(self, app, max_chars: int = 2000):
        super().__init__(app)
        self.max_chars = max_chars

    async def dispatch(self, request: Request, call_next):
        if request.url.path in EXEMPT_PATHS or request.method != "POST":
            return await call_next(request)

        try:
            body = await request.body()
            if not body:
                return await call_next(request)
            data = json.loads(body)
        except (json.JSONDecodeError, UnicodeDecodeError):
            return await call_next(request)

        messages = data.get("messages")
        if isinstance(messages, list) and messages:
            last = messages[-1]
            content = last.get("content", "") if isinstance(last, dict) else ""
            if len(content) > self.max_chars:
                logger.warning(
                    "Query rejected: %d chars > %d limit (path=%s)",
                    len(content),
                    self.max_chars,
                    request.url.path,
                )
                return _error_json(
                    422,
                    "QUERY_TOO_LONG",
                    f"Query text exceeds {self.max_chars} character limit",
                )

        return await call_next(request)


@dataclass
class _TokenBucket:
    tokens: float
    last_refill: float = field(default_factory=time.monotonic)


class RateLimitInMem(BaseHTTPMiddleware):
    """每 IP 每分钟请求限制（in-memory token bucket）。

    /health 等路径自动豁免。
    进程重启后令牌桶清空（in-memory，不持久化）。
    """

    def __init__(self, app, max_requests: int = 30, window_seconds: int = 60):
        super().__init__(app)
        self.max_requests = max_requests
        self.window_seconds = window_seconds
        self._buckets: dict[str, _TokenBucket] = defaultdict(
            lambda: _TokenBucket(tokens=float(max_requests))
        )

    def _refill(self, bucket: _TokenBucket) -> None:
        now = time.monotonic()
        elapsed = now - bucket.last_refill
        refill = (elapsed / self.window_seconds) * self.max_requests
        bucket.tokens = min(bucket.tokens + refill, float(self.max_requests))
        bucket.last_refill = now

    async def dispatch(self, request: Request, call_next):
        if request.url.path in EXEMPT_PATHS:
            return await call_next(request)

        client_ip = request.client.host if request.client else "unknown"
        bucket = self._buckets[client_ip]
        self._refill(bucket)

        if bucket.tokens < 1.0:
            return _error_json(
                429,
                "RATE_LIMITED",
                f"Rate limit exceeded: max {self.max_requests} requests"
                f" per {self.window_seconds}s",
            )

        bucket.tokens -= 1.0
        return await call_next(request)
