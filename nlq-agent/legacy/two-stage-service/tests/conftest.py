"""Test-suite global fixtures.

Strips HTTP/SOCKS proxy environment variables before each test so httpx
clients constructed inside tests do not pick up developer-shell proxies
(e.g. clash/v2ray exposing ALL_PROXY=socks5://...) and fail with
``ImportError: Using SOCKS proxy, but the 'socksio' package is not installed``.
"""

from __future__ import annotations

import pytest


@pytest.fixture(autouse=True)
def _unset_proxy_env(monkeypatch: pytest.MonkeyPatch) -> None:
    for var in (
        "ALL_PROXY",
        "HTTP_PROXY",
        "HTTPS_PROXY",
        "NO_PROXY",
        "all_proxy",
        "http_proxy",
        "https_proxy",
        "no_proxy",
    ):
        monkeypatch.delenv(var, raising=False)
