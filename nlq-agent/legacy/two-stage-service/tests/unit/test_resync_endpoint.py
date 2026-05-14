"""
Unit tests for POST /api/v1/sync/resync-now admin endpoint.

Covers:
- token missing → 401
- token mismatch → 401
- token match + mocked bulk_resync_all → 202 + correct body
- settings.sync_admin_token empty → 401 (even when request carries token)
"""

from __future__ import annotations

from unittest.mock import AsyncMock, patch

import pytest
from httpx import ASGITransport, AsyncClient

from src.main import app


@pytest.fixture
def client():
    return AsyncClient(transport=ASGITransport(app=app), base_url="http://test")


@pytest.mark.asyncio
async def test_resync_no_token_returns_401(client: AsyncClient) -> None:
    """Missing Authorization header → 401."""
    async with client as c:
        resp = await c.post("/api/v1/sync/resync-now")
    assert resp.status_code == 401


@pytest.mark.asyncio
async def test_resync_wrong_token_returns_401(client: AsyncClient) -> None:
    """Wrong Bearer token → 401."""
    with patch("src.api.routes.get_settings") as mock_settings:
        mock_settings.return_value.sync_admin_token = "secret123"
        async with client as c:
            resp = await c.post(
                "/api/v1/sync/resync-now",
                headers={"Authorization": "Bearer wrong-token"},
            )
    assert resp.status_code == 401
    assert resp.json()["detail"] == "invalid_admin_token"


@pytest.mark.asyncio
async def test_resync_correct_token_returns_202(client: AsyncClient) -> None:
    """Correct token + mocked bulk_resync_all → 202 with expected body."""
    with (
        patch("src.api.routes.get_settings") as mock_settings,
        patch("src.api.routes.bulk_resync_all", new_callable=AsyncMock) as mock_resync,
    ):
        mock_settings.return_value.sync_admin_token = "secret123"
        mock_resync.return_value = {"rules": 12, "specs": 5, "duration_ms": 432}

        async with client as c:
            resp = await c.post(
                "/api/v1/sync/resync-now",
                headers={"Authorization": "Bearer secret123"},
            )

    assert resp.status_code == 202
    body = resp.json()
    assert body["status"] == "ok"
    assert body["rules"] == 12
    assert body["specs"] == 5
    assert body["duration_ms"] == 432


@pytest.mark.asyncio
async def test_resync_empty_config_token_returns_401(client: AsyncClient) -> None:
    """Empty sync_admin_token in settings → 401 even when request sends a token."""
    with patch("src.api.routes.get_settings") as mock_settings:
        mock_settings.return_value.sync_admin_token = ""
        async with client as c:
            resp = await c.post(
                "/api/v1/sync/resync-now",
                headers={"Authorization": "Bearer some-token"},
            )
    assert resp.status_code == 401
