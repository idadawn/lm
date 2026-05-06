"""
Unit tests for DatabaseService connection-pool kwargs.

Asserts that init_pool() passes pool_recycle=3600 (production hardening)
plus the previously-existing pool kwargs unchanged.
"""

from __future__ import annotations

from unittest.mock import AsyncMock, patch

import pytest

from src.services.database import DatabaseService


@pytest.mark.asyncio
async def test_init_pool_sets_pool_recycle_to_3600() -> None:
    """init_pool must explicitly request pool_recycle=3600."""
    db = DatabaseService()
    fake_pool = object()  # opaque sentinel

    with patch(
        "src.services.database.aiomysql.create_pool",
        new_callable=AsyncMock,
        return_value=fake_pool,
    ) as mock_create:
        await db.init_pool()

    assert mock_create.await_count == 1
    kwargs = mock_create.await_args.kwargs

    assert "pool_recycle" in kwargs, "pool_recycle missing — recycle hardening lost"
    assert kwargs["pool_recycle"] == 3600
    assert isinstance(kwargs["pool_recycle"], int)


@pytest.mark.asyncio
async def test_init_pool_preserves_other_kwargs() -> None:
    """Adding pool_recycle must not regress the existing pool kwargs."""
    db = DatabaseService()
    fake_pool = object()

    with patch(
        "src.services.database.aiomysql.create_pool",
        new_callable=AsyncMock,
        return_value=fake_pool,
    ) as mock_create:
        await db.init_pool()

    kwargs = mock_create.await_args.kwargs

    expected_keys = {
        "host",
        "port",
        "user",
        "password",
        "db",
        "charset",
        "autocommit",
        "maxsize",
        "minsize",
        "pool_recycle",
    }
    assert set(kwargs.keys()) == expected_keys, (
        f"Unexpected pool kwargs change. Got: {sorted(kwargs.keys())}, "
        f"expected: {sorted(expected_keys)}"
    )
    assert kwargs["autocommit"] is True
    assert kwargs["maxsize"] == 10
    assert kwargs["minsize"] == 2
