"""
Health check endpoint.

GET /healthz — no auth required, used by Docker health checks and uptime monitors.
"""

from __future__ import annotations

from fastapi import APIRouter
from pydantic import BaseModel

from app.config import get_settings

router = APIRouter()


class HealthResponse(BaseModel):
    status: str
    version: str
    knowledge_version: str


@router.get("/healthz", response_model=HealthResponse, tags=["ops"])
async def healthz() -> HealthResponse:
    """
    Return service liveness status.

    Always returns HTTP 200 when the process is alive.
    The ``knowledge_version`` field reflects the KNOWLEDGE_VERSION env var
    so operators can confirm which knowledge snapshot is loaded.
    """
    settings = get_settings()
    return HealthResponse(
        status="ok",
        version="0.1.0",
        knowledge_version=settings.knowledge_version,
    )
