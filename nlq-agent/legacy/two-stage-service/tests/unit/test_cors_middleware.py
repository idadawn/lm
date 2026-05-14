"""Tests for CORS allow_origins configuration via settings."""

from __future__ import annotations

import os
import sys
from types import ModuleType
from unittest.mock import MagicMock

import pytest

# Stub heavy deps so src.main can be imported without real services
for _mod in ("qdrant_client", "qdrant_client.models", "aiomysql", "uvicorn"):
    if _mod not in sys.modules:
        sys.modules[_mod] = MagicMock()


@pytest.fixture(autouse=True)
def _clear_settings_cache():
    """Invalidate lru_cache on Settings so env changes take effect."""
    from src.core.settings import get_settings

    get_settings.cache_clear()
    yield
    get_settings.cache_clear()


# ── Unit tests for _resolve_cors_origins ──────────────────────


def test_empty_origins_defaults_to_wildcard():
    from src.main import _resolve_cors_origins

    result = _resolve_cors_origins("")
    assert result == ["*"]


def test_whitespace_only_defaults_to_wildcard():
    from src.main import _resolve_cors_origins

    result = _resolve_cors_origins("   ")
    assert result == ["*"]


def test_comma_separated_origins_parsed():
    from src.main import _resolve_cors_origins

    result = _resolve_cors_origins("https://a.com, https://b.com")
    assert result == ["https://a.com", "https://b.com"]


def test_single_origin():
    from src.main import _resolve_cors_origins

    result = _resolve_cors_origins("https://prod.example.com")
    assert result == ["https://prod.example.com"]


def test_trailing_commas_ignored():
    from src.main import _resolve_cors_origins

    result = _resolve_cors_origins("https://a.com,")
    assert result == ["https://a.com"]


# ── Integration: verify CORS headers on preflight ─────────────


def test_cors_wildcard_allows_any_origin():
    """Default config (empty CORS_ALLOW_ORIGINS) returns Access-Control-Allow-Origin."""
    from unittest.mock import AsyncMock

    import httpx
    from fastapi import FastAPI
    from fastapi.middleware.cors import CORSMiddleware

    from src.main import _resolve_cors_origins

    app = FastAPI()
    origins = _resolve_cors_origins("")
    app.add_middleware(
        CORSMiddleware,
        allow_origins=origins,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    @app.get("/test")
    async def handler():
        return {"ok": True}

    transport = httpx.ASGITransport(app=app)
    import asyncio

    async def _check():
        async with httpx.AsyncClient(transport=transport, base_url="http://test") as c:
            r = await c.get("/test", headers={"Origin": "https://evil.com"})
            assert r.headers.get("access-control-allow-origin") == "https://evil.com"

    asyncio.run(_check())


def test_cors_whitelist_rejects_unknown_origin():
    """Specific origins should not echo an arbitrary Origin."""
    from fastapi import FastAPI
    from fastapi.middleware.cors import CORSMiddleware

    import httpx
    from src.main import _resolve_cors_origins

    app = FastAPI()
    origins = _resolve_cors_origins("https://trusted.com")
    app.add_middleware(
        CORSMiddleware,
        allow_origins=origins,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    @app.get("/test")
    async def handler():
        return {"ok": True}

    transport = httpx.ASGITransport(app=app)

    import asyncio

    async def _check():
        async with httpx.AsyncClient(transport=transport, base_url="http://test") as c:
            r = await c.get("/test", headers={"Origin": "https://evil.com"})
            assert "access-control-allow-origin" not in r.headers

    asyncio.run(_check())
