"""
nlq-agent 应用入口

FastAPI 应用的创建、生命周期管理和中间件配置。
"""

from __future__ import annotations

import logging
from contextlib import asynccontextmanager

import uvicorn
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from src.api.dependencies import init_services, shutdown_services
from src.api.middleware import (
    CorrelationIDMiddleware,
    QueryLengthGuard,
    RateLimitInMem,
    RequestSizeLimit,
)
from src.api.routes import router
from src.core.logging_config import setup_structured_logging
from src.core.sentry_integration import init_sentry
from src.core.settings import get_settings


@asynccontextmanager
async def lifespan(app: FastAPI):
    """应用生命周期管理。"""
    settings = get_settings()
    setup_structured_logging(level=settings.log_level)
    logger = logging.getLogger(__name__)

    # Startup
    logger.info("nlq-agent 正在启动...")
    init_sentry(settings.sentry_dsn)
    try:
        await init_services()
        logger.info("所有服务初始化完成")
    except Exception as e:
        logger.error("服务初始化失败: %s", e)
        logger.warning("部分功能可能不可用，服务将继续启动")

    yield

    # Shutdown
    logger.info("nlq-agent 正在关闭...")
    await shutdown_services()
    logger.info("所有服务已关闭")


def _resolve_cors_origins(raw: str) -> list[str]:
    """Parse comma-separated CORS_ALLOW_ORIGINS; empty → ["*"] with warning."""
    if not raw.strip():
        logger = logging.getLogger(__name__)
        logger.warning("CORS_ALLOW_ORIGINS is empty — allowing all origins (suitable for dev only)")
        return ["*"]
    return [o.strip() for o in raw.split(",") if o.strip()]


def create_app() -> FastAPI:
    """创建 FastAPI 应用实例。"""
    settings = get_settings()

    app = FastAPI(
        title="nlq-agent",
        description="路美实验室数据分析 — 两阶段问答微服务",
        version=settings.app_version,
        lifespan=lifespan,
    )

    # CORS 中间件（从 settings 读取白名单）
    origins = _resolve_cors_origins(settings.cors_allow_origins)
    app.add_middleware(
        CORSMiddleware,
        allow_origins=origins,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    # 请求安全中间件（执行顺序：CorrelationID → RateLimit → QueryLength → RequestSize → handler）
    app.add_middleware(RequestSizeLimit)
    app.add_middleware(QueryLengthGuard)
    app.add_middleware(RateLimitInMem)
    app.add_middleware(CorrelationIDMiddleware)

    # 注册路由
    app.include_router(router)

    return app


# 创建应用实例（供 uvicorn 直接引用）
app = create_app()


if __name__ == "__main__":
    settings = get_settings()
    uvicorn.run(
        "src.main:app",
        host=settings.host,
        port=settings.port,
        reload=settings.debug,
        log_level=settings.log_level.lower(),
    )
