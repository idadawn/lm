"""知识图谱管理模块.

处理知识图谱的生命周期：初始化、刷新、关闭。
"""

from typing import TYPE_CHECKING

from app.core.config import settings
from app.core.logger import logger

if TYPE_CHECKING:
    from app.knowledge_graph.base import BaseKnowledgeGraph

# 全局图谱实例
_knowledge_graph: "BaseKnowledgeGraph | None" = None


async def init_knowledge_graph(force: bool = False) -> "BaseKnowledgeGraph | None":
    """初始化知识图谱.

    根据配置创建对应的后端实现。

    Args:
        force: 是否忽略 NEO4J_ENABLED 开关强制初始化

    Returns:
        知识图谱实例，如果未启用则返回None
    """
    global _knowledge_graph

    if not force and not settings.NEO4J_ENABLED:
        logger.info("[KnowledgeGraph] Neo4j is disabled, skipping initialization")
        return None

    try:
        from app.knowledge_graph.neo4j_graph import Neo4jKnowledgeGraph

        logger.info("[KnowledgeGraph] Initializing Neo4j backend...")
        _knowledge_graph = Neo4jKnowledgeGraph()

        # 构建图谱
        await _knowledge_graph.build()
        logger.info("[KnowledgeGraph] Neo4j graph built successfully")

        return _knowledge_graph

    except Exception as e:
        logger.error(f"[KnowledgeGraph] Failed to initialize: {e}")
        _knowledge_graph = None
        return None


async def close_knowledge_graph() -> None:
    """关闭知识图谱连接."""
    global _knowledge_graph

    if _knowledge_graph is not None:
        try:
            await _knowledge_graph.close()
            logger.info("[KnowledgeGraph] Connection closed")
        except Exception as e:
            logger.error(f"[KnowledgeGraph] Error closing connection: {e}")
        finally:
            _knowledge_graph = None


async def refresh_knowledge_graph() -> bool:
    """刷新知识图谱数据.

    Returns:
        是否刷新成功
    """
    global _knowledge_graph

    if _knowledge_graph is None:
        logger.warning("[KnowledgeGraph] Not initialized, cannot refresh")
        return False

    try:
        await _knowledge_graph.refresh()
        logger.info("[KnowledgeGraph] Refreshed successfully")
        return True
    except Exception as e:
        logger.error(f"[KnowledgeGraph] Refresh failed: {e}")
        return False


def get_knowledge_graph() -> "BaseKnowledgeGraph | None":
    """获取知识图谱实例.

    Returns:
        知识图谱实例，如果未初始化则返回None
    """
    return _knowledge_graph


def is_knowledge_graph_ready() -> bool:
    """检查知识图谱是否已准备好.

    Returns:
        是否可用
    """
    return _knowledge_graph is not None


def get_knowledge_graph_status() -> dict[str, str | bool]:
    """获取知识图谱当前状态."""
    if _knowledge_graph is not None:
        return {
            "ready": True,
            "enabled": settings.NEO4J_ENABLED,
            "backend": "neo4j",
            "message": "Knowledge graph is ready",
        }

    if not settings.NEO4J_ENABLED:
        return {
            "ready": False,
            "enabled": False,
            "backend": "neo4j",
            "message": "Neo4j knowledge graph is disabled by configuration",
        }

    return {
        "ready": False,
        "enabled": True,
        "backend": "neo4j",
        "message": "Knowledge graph is not initialized",
    }
