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
from src.api.middleware import QueryLengthGuard, RateLimitInMem, RequestSizeLimit
from src.api.routes import router
from src.core.settings import get_settings


def setup_logging() -> None:
    """配置日志格式。"""
    settings = get_settings()
    logging.basicConfig(
        level=getattr(logging, settings.log_level.upper(), logging.INFO),
        format="%(asctime)s | %(levelname)-7s | %(name)s | %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S",
    )


@asynccontextmanager
async def lifespan(app: FastAPI):
    """应用生命周期管理。"""
    setup_logging()
    logger = logging.getLogger(__name__)

    # Startup
    logger.info("nlq-agent 正在启动...")
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


def create_app() -> FastAPI:
    """创建 FastAPI 应用实例。"""
    settings = get_settings()

    app = FastAPI(
        title="nlq-agent",
        description="路美实验室数据分析 — 两阶段问答微服务",
        version=settings.app_version,
        lifespan=lifespan,
    )

    # CORS 中间件
    app.add_middleware(
        CORSMiddleware,
        allow_origins=["*"],  # 生产环境应限制为具体域名
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    # 请求安全中间件（执行顺序：RateLimit → QueryLength → RequestSize → handler）
    app.add_middleware(RequestSizeLimit)
    app.add_middleware(QueryLengthGuard)
    app.add_middleware(RateLimitInMem)

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
