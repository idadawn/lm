"""健康检查 API 模块."""

from fastapi import APIRouter

router = APIRouter()


@router.get("/health")
async def health_check() -> dict:
    """健康检查端点.

    Returns:
        服务状态信息
    """
    return {
        "status": "healthy",
        "service": "nlq-agent-api",
        "version": "0.1.0",
    }


@router.get("/ready")
async def readiness_check() -> dict:
    """就绪检查端点.

    Returns:
        服务就绪状态
    """
    return {
        "ready": True,
    }
