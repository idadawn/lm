"""
Sentry error tracking integration.

Initialises sentry_sdk when SENTRY_DSN is set; otherwise no-op with a log.
"""

from __future__ import annotations

import logging

logger = logging.getLogger(__name__)


def init_sentry(dsn: str | None) -> None:
    """Initialise Sentry SDK if a DSN is provided.

    Call this once during application startup.  When ``dsn`` is empty / None
    the function returns immediately after logging a debug message.
    """
    if not dsn:
        logger.debug("SENTRY_DSN not configured — error tracking disabled")
        return

    try:
        import sentry_sdk
    except ImportError:
        logger.warning("sentry_sdk not installed — SENTRY_DSN ignored")
        return

    sentry_sdk.init(dsn=dsn, traces_sample_rate=0.1)
    logger.info("Sentry error tracking enabled (dsn=%s…)", dsn[:20])
