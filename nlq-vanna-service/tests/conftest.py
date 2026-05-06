"""Shared fixtures for all test suites.

Provides:
  - mock_httpx_client  : respx-patched httpx.Client (scope=function)
  - mock_qdrant_client : MagicMock replacing QdrantClient
  - fake_settings      : minimal Settings-like object with required fields
  - known_term         : the canonical test terminology string "叠片系数"
"""

from __future__ import annotations

from unittest.mock import MagicMock, patch

import pytest

# ---------------------------------------------------------------------------
# known_term fixture — canonical test terminology keyword
# ---------------------------------------------------------------------------


@pytest.fixture
def known_term() -> str:
    """Return the canonical test terminology string used across doc-retrieval tests."""
    return "叠片系数"


# ---------------------------------------------------------------------------
# fake_settings fixture
# ---------------------------------------------------------------------------


@pytest.fixture
def fake_settings():
    """Return a minimal settings-like namespace with required service config."""
    settings = MagicMock()
    settings.qdrant_url = "http://localhost:6333"
    settings.embedding_dim = 768
    settings.tei_url = "http://localhost:8080"
    settings.vllm_url = "http://localhost:8000"
    settings.vllm_model = "Qwen2.5-7B-Instruct"
    settings.mysql_host = "localhost"
    settings.mysql_port = 3306
    settings.mysql_user = "root"
    settings.mysql_password = "password"
    settings.mysql_db = "lumei"
    return settings


# ---------------------------------------------------------------------------
# mock_qdrant_client fixture
# ---------------------------------------------------------------------------


@pytest.fixture
def mock_qdrant_client():
    """Yield a MagicMock that replaces QdrantClient for the duration of the test."""
    with patch("qdrant_client.QdrantClient") as mock_cls:
        mock_instance = MagicMock()
        mock_cls.return_value = mock_instance

        # Default: collection already exists (no creation needed)
        collections_resp = MagicMock()
        collections_resp.collections = []
        mock_instance.get_collections.return_value = collections_resp

        yield mock_instance


# ---------------------------------------------------------------------------
# mock_httpx_client fixture
# ---------------------------------------------------------------------------


@pytest.fixture
def mock_httpx_client():
    """Yield a MagicMock replacing httpx.Client for unit tests that call HTTP APIs."""
    with patch("httpx.Client") as mock_cls:
        mock_instance = MagicMock()
        mock_cls.return_value.__enter__ = lambda s: mock_instance
        mock_cls.return_value.__exit__ = MagicMock(return_value=False)
        mock_cls.return_value = mock_instance
        yield mock_instance
