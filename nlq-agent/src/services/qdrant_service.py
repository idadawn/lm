"""
Qdrant 向量数据库服务

负责：
1. Collection 的创建与管理
2. 文档的向量化与写入（indexing）
3. 语义检索（retrieval）
4. 增量同步（当 .NET 后端通知规则/规格变更时）
"""

from __future__ import annotations

import logging
from typing import Any

from qdrant_client import AsyncQdrantClient
from qdrant_client.models import (
    Distance,
    FieldCondition,
    Filter,
    MatchValue,
    PointStruct,
    VectorParams,
)

from src.core.settings import get_settings
from src.services.embedding_client import EmbeddingClient

logger = logging.getLogger(__name__)


class QdrantService:
    """Qdrant 向量数据库操作封装。"""

    def __init__(self, embedding_client: EmbeddingClient) -> None:
        settings = get_settings()
        self._client = AsyncQdrantClient(
            host=settings.qdrant_host,
            port=settings.qdrant_port,
            grpc_port=settings.qdrant_grpc_port,
            api_key=settings.qdrant_api_key or None,
        )
        self._embedding = embedding_client
        self._dim = settings.embedding_dim
        self._top_k = settings.retrieval_top_k

    # ── Collection 管理 ──────────────────────────────────────

    async def ensure_collections(self) -> None:
        """确保所有必要的 Collection 存在，不存在则创建。"""
        settings = get_settings()
        for name in [
            settings.collection_rules,
            settings.collection_specs,
            settings.collection_metrics,
        ]:
            exists = await self._client.collection_exists(name)
            if not exists:
                await self._client.create_collection(
                    collection_name=name,
                    vectors_config=VectorParams(
                        size=self._dim,
                        distance=Distance.COSINE,
                    ),
                )
                logger.info("已创建 Qdrant Collection: %s", name)

    # ── 文档写入（Indexing）──────────────────────────────────

    async def upsert_documents(
        self,
        collection_name: str,
        documents: list[dict[str, Any]],
    ) -> int:
        """
        批量写入/更新文档到指定 Collection。

        Args:
            collection_name: Collection 名称
            documents: 文档列表，每个文档需包含：
                - id: str — 唯一标识
                - text: str — 用于向量化的文本
                - metadata: dict — 附加元数据（如 spec_code, rule_name 等）

        Returns:
            成功写入的文档数量
        """
        if not documents:
            return 0

        texts = [doc["text"] for doc in documents]
        vectors = await self._embedding.embed_batch(texts)

        points = [
            PointStruct(
                id=idx,
                vector=vector,
                payload={
                    "doc_id": doc["id"],
                    "text": doc["text"],
                    **doc.get("metadata", {}),
                },
            )
            for idx, (doc, vector) in enumerate(zip(documents, vectors))
        ]

        await self._client.upsert(
            collection_name=collection_name,
            points=points,
        )
        logger.info(
            "已写入 %d 条文档到 %s", len(points), collection_name
        )
        return len(points)

    # ── 语义检索（Retrieval）─────────────────────────────────

    async def search(
        self,
        collection_name: str,
        query: str,
        *,
        top_k: int | None = None,
        filter_conditions: dict[str, str] | None = None,
        score_threshold: float = 0.3,
    ) -> list[dict[str, Any]]:
        """
        语义检索：将 query 向量化后在指定 Collection 中检索。

        Args:
            collection_name: Collection 名称
            query: 用户查询文本
            top_k: 返回数量（默认使用配置值）
            filter_conditions: 额外的精确匹配过滤（如 {"spec_code": "120"}）
            score_threshold: 最低相似度阈值

        Returns:
            检索结果列表，每项包含 text, score, metadata
        """
        query_vector = await self._embedding.embed(query)

        # 构建过滤条件
        qdrant_filter = None
        if filter_conditions:
            must = [
                FieldCondition(key=k, match=MatchValue(value=v))
                for k, v in filter_conditions.items()
            ]
            qdrant_filter = Filter(must=must)

        response = await self._client.query_points(
            collection_name=collection_name,
            query=query_vector,
            limit=top_k or self._top_k,
            query_filter=qdrant_filter,
            score_threshold=score_threshold,
        )

        return [
            {
                "text": hit.payload.get("text", ""),
                "score": hit.score,
                "doc_id": hit.payload.get("doc_id", ""),
                **{
                    k: v
                    for k, v in hit.payload.items()
                    if k not in ("text", "doc_id")
                },
            }
            for hit in response.points
        ]

    async def search_multi_collection(
        self,
        query: str,
        *,
        top_k: int | None = None,
    ) -> dict[str, list[dict[str, Any]]]:
        """
        同时在所有 Collection 中检索（规则、规格、指标）。

        Returns:
            以 collection 名称为 key 的检索结果字典
        """
        settings = get_settings()
        results = {}
        for name in [
            settings.collection_rules,
            settings.collection_specs,
            settings.collection_metrics,
        ]:
            try:
                hits = await self.search(name, query, top_k=top_k)
                results[name] = hits
            except Exception as e:
                logger.warning("检索 %s 失败: %s", name, e)
                results[name] = []
        return results

    # ── 健康检查 ─────────────────────────────────────────────

    async def health_check(self) -> bool:
        """检查 Qdrant 是否可用。"""
        try:
            await self._client.get_collections()
            return True
        except Exception:
            return False

    async def close(self) -> None:
        """关闭客户端连接。"""
        await self._client.close()
