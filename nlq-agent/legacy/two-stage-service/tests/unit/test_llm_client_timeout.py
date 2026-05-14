"""
Unit tests for LLMClient HTTP timeout configuration.

Asserts that AsyncOpenAI is constructed with an explicit httpx.Timeout
sourced from settings — the equivalent fix for the OpenAI-SDK-based
code path of PRODUCTION_CHECKLIST item 'LLM streaming chunk_size 合理'.
"""

from __future__ import annotations

import httpx
import pytest

from src.core.settings import Settings, get_settings
from src.services.llm_client import LLMClient


@pytest.fixture(autouse=True)
def _reset_settings_cache():
    """Clear get_settings lru_cache between tests so env overrides take effect."""
    get_settings.cache_clear()
    yield
    get_settings.cache_clear()


def test_client_uses_httpx_timeout() -> None:
    """LLMClient._client.timeout must be an httpx.Timeout instance."""
    client = LLMClient()
    assert isinstance(client._client.timeout, httpx.Timeout)


def test_default_timeout_values_match_settings() -> None:
    """Default timeouts come from Settings defaults.

    `settings` and `LLMClient()`'s internal `get_settings()` are two distinct
    Settings instances that both read identical class defaults — they are
    not required to be the same object for this assertion to hold.
    """
    settings = Settings()
    client = LLMClient()
    timeout = client._client.timeout

    assert timeout.connect == settings.llm_http_connect_timeout_s
    assert timeout.read == settings.llm_http_read_timeout_s
    # Pool/write share the catch-all llm_http_timeout_s default.
    assert timeout.write == settings.llm_http_timeout_s
    assert timeout.pool == settings.llm_http_timeout_s


def test_env_override_propagates_to_client(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    """Setting LLM_HTTP_READ_TIMEOUT_S via env must reach the AsyncOpenAI client."""
    monkeypatch.setenv("LLM_HTTP_READ_TIMEOUT_S", "99.5")
    monkeypatch.setenv("LLM_HTTP_CONNECT_TIMEOUT_S", "7.5")
    get_settings.cache_clear()

    client = LLMClient()
    timeout = client._client.timeout

    assert timeout.read == 99.5
    assert timeout.connect == 7.5
