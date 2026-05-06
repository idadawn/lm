"""
FastAPI dependency providers.

- get_settings()     : cached settings singleton (re-exported from config)
- verify_token()     : Bearer token guard (skip when AUTH_TOKEN is empty)
- get_vanna_app()    : lru_cache singleton returning the VannaApp instance
"""

from __future__ import annotations

from functools import lru_cache
from typing import Optional

from fastapi import Header, HTTPException, status

from app.config import Settings, get_settings


# ---------------------------------------------------------------------------
# Settings dependency (lru_cache already applied in config.py)
# ---------------------------------------------------------------------------


def settings_dep() -> Settings:
    """FastAPI-injectable alias for get_settings()."""
    return get_settings()


# ---------------------------------------------------------------------------
# Token verification
#
# Accepts the X-Request-Origin header as per plan Principle 3:
#   "accept X-Request-Origin: web|mobile|embedded header but ignore for auth"
# The header value is logged/forwarded but does NOT affect authentication logic.
#
# AUTH_TOKEN empty → skip Bearer check entirely.
# AUTH_TOKEN set   → require `Authorization: Bearer <token>`, exact match.
# ---------------------------------------------------------------------------


async def verify_token(
    authorization: Optional[str] = Header(default=None),
    x_request_origin: Optional[str] = Header(default=None),
) -> str:
    """
    Verify Bearer token.

    Parameters
    ----------
    authorization:
        Standard HTTP `Authorization` header, expected format:
        ``Bearer <token>``
    x_request_origin:
        Optional caller hint: ``web`` | ``mobile`` | ``embedded``.
        Accepted but not used in authorization decisions (plan Principle 3).

    Returns
    -------
    str
        The validated token string, or empty string when auth is disabled.

    Raises
    ------
    HTTPException 401
        When AUTH_TOKEN is configured and the supplied token does not match.
    """
    cfg = get_settings()

    # Auth disabled — accept all requests
    if not cfg.auth_token:
        return ""

    # Auth enabled — strict Bearer check
    if not authorization or not authorization.startswith("Bearer "):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Missing or malformed Authorization header (expected: Bearer <token>)",
            headers={"WWW-Authenticate": "Bearer"},
        )

    token = authorization.removeprefix("Bearer ").strip()
    if token != cfg.auth_token:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid token",
            headers={"WWW-Authenticate": "Bearer"},
        )

    return token


# ---------------------------------------------------------------------------
# VannaApp dependency — lru_cache singleton
# ---------------------------------------------------------------------------


@lru_cache(maxsize=1)
def _vanna_app_singleton() -> object:
    """Create and cache the VannaApp instance (called once per process).

    Uses lru_cache(maxsize=1) so the heavy initialization (Qdrant client,
    TEI HTTP session, vLLM client) happens only on first call.
    """
    from app.vanna_app import create_vanna_app  # noqa: PLC0415

    return create_vanna_app()


def get_vanna_app() -> object:
    """FastAPI dependency that returns the cached VannaApp singleton.

    Returns:
        The fully initialised VannaApp instance (QdrantStoreMixin +
        TeiEmbedMixin + VllmLlmMixin + VannaBase).
    """
    return _vanna_app_singleton()
