"""
FastAPI 依赖注入

管理所有服务的单例实例，通过 FastAPI 的 Depends 机制注入到路由中。
"""

from __future__ import annotations

from src.pipelines.orchestrator import PipelineOrchestrator
from src.services.database import DatabaseService
from src.services.embedding_client import EmbeddingClient
from src.services.llm_client import LLMClient
from src.services.qdrant_service import QdrantService

# 全局单例（在 app lifespan 中初始化）
_llm: LLMClient | None = None
_embedding: EmbeddingClient | None = None
_qdrant: QdrantService | None = None
_db: DatabaseService | None = None
_orchestrator: PipelineOrchestrator | None = None


async def init_services() -> None:
    """初始化所有服务（在 app startup 时调用）。"""
    global _llm, _embedding, _qdrant, _db, _orchestrator

    _llm = LLMClient()
    _embedding = EmbeddingClient()
    _qdrant = QdrantService(_embedding)
    _db = DatabaseService()

    # 初始化连接
    await _qdrant.ensure_collections()
    await _db.init_pool()

    # 创建编排器
    _orchestrator = PipelineOrchestrator(_llm, _qdrant, _db)


async def shutdown_services() -> None:
    """关闭所有服务（在 app shutdown 时调用）。"""
    if _embedding:
        await _embedding.close()
    if _qdrant:
        await _qdrant.close()
    if _db:
        await _db.close()


def get_orchestrator() -> PipelineOrchestrator:
    """获取 Pipeline 编排器实例。"""
    assert _orchestrator is not None, "服务未初始化，请检查 app lifespan"
    return _orchestrator


def get_services() -> tuple[LLMClient, QdrantService, DatabaseService, EmbeddingClient]:
    """获取所有服务实例（用于健康检查等）。"""
    assert _llm and _qdrant and _db and _embedding, "服务未初始化"
    return _llm, _qdrant, _db, _embedding
