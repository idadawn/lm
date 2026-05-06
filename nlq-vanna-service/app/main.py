"""
FastAPI application entry point for nlq-vanna-service.

Lifespan:
  - Calls bootstrap_knowledge() on startup (placeholder until Wave 2).
  - Shuts down cleanly on stop.

Workers:
  --workers 1 is intentional and must NOT be increased without first auditing
  Vanna's generate_sql() for process-level thread safety.  The function uses
  internal state (Qdrant client + TEI HTTP session) that has not been verified
  safe across multiple forked OS processes sharing the same collection.
  Use a single worker + asyncio.to_thread() for I/O concurrency instead.

Routes registered:
  GET  /healthz              — liveness probe (no auth)
  POST /api/v1/chat/stream   — SSE chat endpoint (Wave 2)
"""

from __future__ import annotations

import logging
import os
from contextlib import asynccontextmanager
from typing import AsyncGenerator

from pathlib import Path

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse

from app.api.chat_stream import router as chat_router
from app.api.health import router as health_router
from app.config import get_settings

_STATIC_DIR = Path(__file__).resolve().parent.parent / "static"


# ---------------------------------------------------------------------------
# Logging setup
# ---------------------------------------------------------------------------

def _configure_logging() -> None:
    settings = get_settings()
    logging.basicConfig(
        level=settings.log_level.upper(),
        format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
    )


# ---------------------------------------------------------------------------
# Knowledge bootstrap — real implementation (Wave 2)
# ---------------------------------------------------------------------------


# ---------------------------------------------------------------------------
# Lifespan
# ---------------------------------------------------------------------------

@asynccontextmanager
async def lifespan(app: FastAPI) -> AsyncGenerator[None, None]:
    _configure_logging()
    logger = logging.getLogger(__name__)
    logger.info("nlq-vanna-service starting up")

    from app.deps import get_vanna_app  # noqa: PLC0415
    from app.knowledge.bootstrap import bootstrap_knowledge  # noqa: PLC0415

    settings = get_settings()
    vn = get_vanna_app()
    await bootstrap_knowledge(vn, settings)

    logger.info("nlq-vanna-service ready")
    yield
    logger.info("nlq-vanna-service shutting down")


# ---------------------------------------------------------------------------
# Application factory
# ---------------------------------------------------------------------------

def create_app() -> FastAPI:
    settings = get_settings()

    application = FastAPI(
        title="nlq-vanna-service",
        version="0.1.0",
        description="Vanna-based NLQ microservice for laboratory data analysis",
        lifespan=lifespan,
    )

    # CORS — env-driven; wildcard + credentials is invalid per CORS spec
    _cors_origins_env = os.getenv("CORS_ORIGINS", "").strip()
    _cors_origins = [o.strip() for o in _cors_origins_env.split(",") if o.strip()]
    application.add_middleware(
        CORSMiddleware,
        allow_origins=_cors_origins or ["*"],
        allow_credentials=bool(_cors_origins),  # credentials only when origins are explicit
        allow_methods=["GET", "POST"],
        allow_headers=["Authorization", "Content-Type", "X-Request-Origin"],
    )

    # Health check (no auth)
    application.include_router(health_router)

    # SSE chat stream endpoint: POST /api/v1/chat/stream
    # router already has full path /api/v1/chat/stream — no prefix needed
    application.include_router(chat_router)

    # Demo chat UI — minimal HTML page consuming /api/v1/chat/stream
    @application.get("/chat", include_in_schema=False)
    async def chat_ui() -> FileResponse:
        return FileResponse(_STATIC_DIR / "chat.html")

    return application


app = create_app()
