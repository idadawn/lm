"""
Unit test: benchmark setup must bypass RateLimitInMem.

Asserts the contract scripts/benchmark.py relies on — patching
src.main.RateLimitInMem swaps it out of the middleware stack and
substitutes the passthrough, so the benchmark measures application
throughput rather than per-IP token-bucket arithmetic.

Real rate-limit behaviour is verified by tests/unit/test_middleware.py;
this test only verifies the bypass mechanism for benchmark contexts.
"""

from __future__ import annotations

from unittest.mock import AsyncMock, patch

from starlette.middleware.base import BaseHTTPMiddleware

from src.api.middleware import RateLimitInMem


class _StubMiddleware(BaseHTTPMiddleware):
    """Passthrough used to verify rate-limit can be swapped at create_app time."""

    async def dispatch(self, request, call_next):
        return await call_next(request)


def test_ratelimit_can_be_replaced_in_create_app() -> None:
    """Patching src.main.RateLimitInMem replaces it in the resulting middleware stack."""
    with (
        patch("src.main.init_services", new_callable=AsyncMock),
        patch("src.main.shutdown_services", new_callable=AsyncMock),
        patch("src.main.RateLimitInMem", _StubMiddleware),
    ):
        from src.main import create_app

        app = create_app()

    middleware_classes = [m.cls for m in app.user_middleware]

    assert RateLimitInMem not in middleware_classes, (
        f"RateLimitInMem must not be present after patch: {middleware_classes}"
    )
    assert _StubMiddleware in middleware_classes, (
        f"Stub middleware must be present after patch: {middleware_classes}"
    )


def test_benchmark_module_uses_passthrough_in_patch_context() -> None:
    """scripts.benchmark must declare _PassthroughMiddleware and reference it in run_benchmark."""
    import scripts.benchmark as bench

    assert hasattr(bench, "_PassthroughMiddleware"), (
        "scripts.benchmark must expose _PassthroughMiddleware"
    )
    assert issubclass(bench._PassthroughMiddleware, BaseHTTPMiddleware)

    import inspect

    source = inspect.getsource(bench.run_benchmark)
    assert 'patch("src.main.RateLimitInMem"' in source, (
        "run_benchmark must patch src.main.RateLimitInMem with the passthrough"
    )
    assert "_PassthroughMiddleware" in source, (
        "run_benchmark must use _PassthroughMiddleware as the patch target"
    )
