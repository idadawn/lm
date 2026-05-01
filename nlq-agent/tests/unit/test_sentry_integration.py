"""Tests for Sentry error tracking integration."""

from __future__ import annotations

import sys
from unittest.mock import MagicMock, patch

from src.core.sentry_integration import init_sentry


def test_no_dsn_does_not_call_init():
    """When dsn is None/empty, sentry_sdk.init should never be called."""
    with patch.dict(sys.modules, {"sentry_sdk": MagicMock()}):
        with patch("src.core.sentry_integration.sentry_sdk", create=True) as mock_sdk:
            # Even if sentry_sdk were importable, init_sentry must not call it
            init_sentry(None)
            init_sentry("")
            mock_sdk.init.assert_not_called()


def test_with_dsn_calls_sentry_init():
    """When a DSN is provided, sentry_sdk.init is called with it."""
    mock_sdk = MagicMock()
    with patch.dict(sys.modules, {"sentry_sdk": mock_sdk}):
        init_sentry("https://example@sentry.io/123")

    mock_sdk.init.assert_called_once()
    call_kwargs = mock_sdk.init.call_args
    assert call_kwargs[1]["dsn"] == "https://example@sentry.io/123"


def test_with_dsn_but_sentry_not_installed():
    """When sentry_sdk is not importable, the function logs a warning instead of crashing."""
    with patch.dict(sys.modules, {"sentry_sdk": None}):
        # Remove from importable modules so ImportError fires
        modules_backup = sys.modules.pop("sentry_sdk", None)
        try:
            init_sentry("https://example@sentry.io/123")  # should not raise
        finally:
            if modules_backup is not None:
                sys.modules["sentry_sdk"] = modules_backup
