"""
Embedding 客户端封装

对接 TEI (Text Embeddings Inference) 服务，将文本转换为向量。
支持单条和批量 embedding。
"""

from __future__ import annotations

import logging

import httpx

from src.core.settings import get_settings

logger = logging.getLogger(__name__)


class EmbeddingClient:
    """异步 Embedding 客户端，对接 TEI 服务。"""

    def __init__(self) -> None:
        settings = get_settings()
        self._base_url = settings.embedding_base_url.rstrip("/")
        self._model = settings.embedding_model
        headers: dict[str, str] = {}
        if settings.embedding_api_key:
            headers["Authorization"] = f"Bearer {settings.embedding_api_key}"
        self._client = httpx.AsyncClient(timeout=30.0, headers=headers)

    async def embed(self, text: str) -> list[float]:
        """
        将单条文本转换为向量。

        Args:
            text: 输入文本

        Returns:
            浮点数向量列表
        """
        vectors = await self.embed_batch([text])
        return vectors[0]

    async def embed_batch(self, texts: list[str]) -> list[list[float]]:
        """
        批量将文本转换为向量。

        Args:
            texts: 输入文本列表

        Returns:
            向量列表（与输入顺序一一对应）
        """
        try:
            # TEI 兼容 OpenAI embeddings API 格式
            response = await self._client.post(
                f"{self._base_url}/embeddings",
                json={
                    "model": self._model,
                    "input": texts,
                },
            )
            response.raise_for_status()
            data = response.json()
            # 按 index 排序，确保顺序一致
            embeddings = sorted(data["data"], key=lambda x: x["index"])
            return [item["embedding"] for item in embeddings]
        except Exception as e:
            logger.error("Embedding 调用失败: %s", e)
            raise

    async def health_check(self) -> bool:
        """检查 TEI 服务是否可用。"""
        try:
            response = await self._client.get(f"{self._base_url}/health")
            return response.status_code == 200
        except Exception:
            return False

    async def close(self) -> None:
        """关闭 HTTP 客户端。"""
        await self._client.aclose()
