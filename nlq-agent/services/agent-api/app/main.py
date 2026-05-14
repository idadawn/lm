"""FastAPI 应用入口模块."""

from collections.abc import AsyncGenerator
from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.api import chat, health, kg, sync
from app.core.config import settings
from app.core.database import engine
from app.core.sentry_integration import init_sentry
from app.knowledge_graph.manager import close_knowledge_graph, init_knowledge_graph
from app.knowledge_graph.schema_loader import refresh_schema_cache


@asynccontextmanager
async def lifespan(app: FastAPI) -> AsyncGenerator[None, None]:
    """应用生命周期管理.

    启动时初始化资源，关闭时清理资源。
    """
    # 启动时：初始化 Sentry（如果配置了 DSN）
    init_sentry(settings.SENTRY_DSN)

    # 启动时：初始化知识图谱
    print("[START] NLQ-Agent API starting...")

    # 初始化知识图谱（如果启用）
    await init_knowledge_graph()

    # 初始化 Chat2SQL schema cache（best-effort）
    try:
        await refresh_schema_cache()
    except Exception as exc:  # noqa: BLE001
        print(f"[WARN] schema_loader.refresh failed: {exc}")

    yield

    # 关闭时：清理资源
    print("[STOP] NLQ-Agent API stopping...")
    await close_knowledge_graph()
    await engine.dispose()


# 创建 FastAPI 应用
app = FastAPI(
    title="NLQ-Agent API",
    description="自然语言查询工业质量数据的 AI Agent API",
    version="0.1.0",
    lifespan=lifespan,
)

# 配置 CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.CORS_ORIGINS,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# 注册路由
app.include_router(chat.router, prefix="/api/v1")
app.include_router(health.router, prefix="/api/v1")
app.include_router(kg.router, prefix="/api/v1/kg")
app.include_router(sync.router, prefix="/api/v1/sync")


@app.get("/")
async def root() -> dict:
    """根路径.

    Returns:
        服务基本信息
    """
    return {
        "name": "NLQ-Agent API",
        "version": "0.1.0",
        "docs": "/docs",
    }


# Mon Mar 16 11:45:10     2026
# Mon Mar 16 11:48:44     2026
