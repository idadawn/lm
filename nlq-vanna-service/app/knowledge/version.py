"""知识版本管理。

通过 Qdrant collection metadata 存储/读取 knowledge_version 字段，
用于判断是否需要重新索引知识库。

约定：元数据点使用 subtype="__meta__" 标记，所有搜索路径应
通过 must_not filter 排除该 subtype，防止元数据点污染 RAG 召回结果。
"""

from __future__ import annotations

import logging

from qdrant_client import QdrantClient

# 元数据点的 subtype 标记常量（供搜索 filter 引用）
_META_SUBTYPE = "__meta__"

logger = logging.getLogger(__name__)


def compute_collection_metadata(
    client: QdrantClient, collection_name: str
) -> dict | None:
    """从 Qdrant collection 元数据读取 payload metadata。

    Args:
        client:          已初始化的 QdrantClient。
        collection_name: Qdrant collection 名称。

    Returns:
        包含 metadata 字段的 dict，或 None（collection 不存在/无 metadata）。
    """
    try:
        existing = {c.name for c in client.get_collections().collections}
        if collection_name not in existing:
            return None
        info = client.get_collection(collection_name=collection_name)
        # Qdrant 1.10+ 将 collection-level metadata 存于 config.params 下
        # 以 payload_schema / optimizer_config 外的自定义字段形式暴露；
        # 实践中通过在 collection 名称对应的"元点"里存一个固定 payload 来实现。
        # 我们用一个 id=0 的特殊 scroll 读取元数据点。
        result, _ = client.scroll(
            collection_name=collection_name,
            scroll_filter=None,
            limit=1,
            offset=None,
            with_payload=True,
            with_vectors=False,
        )
        # 从 collection 描述中提取版本信息——Qdrant 没有原生 collection metadata
        # 因此我们约定用一个固定 payload key "__collection_meta__" 的点存储。
        for point in result:
            payload = point.payload or {}
            if "__collection_meta__" in payload:
                return dict(payload["__collection_meta__"])
        return {}
    except Exception as exc:  # noqa: BLE001
        logger.warning("compute_collection_metadata failed: %s", exc)
        return None


def set_collection_metadata(
    client: QdrantClient, collection_name: str, metadata: dict
) -> None:
    """将 metadata dict 写入 collection 的元数据点。

    约定：使用 id="__meta__" 的点（payload 含 "__collection_meta__" 键）
    来存储 collection 级别的版本信息。

    Args:
        client:          已初始化的 QdrantClient。
        collection_name: Qdrant collection 名称。
        metadata:        要写入的元数据字典，例如 {"knowledge_version": "v1"}。
    """
    from qdrant_client.http import models as qmodels  # noqa: PLC0415

    # 使用固定字符串 ID 的点存储 collection 元数据
    # Qdrant 点 ID 只支持 uint64 或 UUID，用 UUID 的全零变体表示元数据点
    meta_point_id = "00000000000000000000000000000000"
    try:
        client.upsert(
            collection_name=collection_name,
            points=[
                qmodels.PointStruct(
                    id=meta_point_id,
                    vector=[0.0] * _get_vector_dim(client, collection_name),
                    payload={"__collection_meta__": metadata, "subtype": _META_SUBTYPE},
                )
            ],
        )
        logger.info(
            "set_collection_metadata: collection=%s metadata=%s",
            collection_name,
            metadata,
        )
    except Exception as exc:  # noqa: BLE001
        logger.warning("set_collection_metadata failed: %s", exc)


def _get_vector_dim(client: QdrantClient, collection_name: str) -> int:
    """获取 collection 向量维度，用于构造零向量元数据点。"""
    try:
        info = client.get_collection(collection_name=collection_name)
        params = info.config.params.vectors
        if hasattr(params, "size"):
            return int(params.size)
        # 若 params 是 dict 形式（命名向量），取第一个
        if isinstance(params, dict):
            first = next(iter(params.values()))
            return int(first.size)
    except Exception:  # noqa: BLE001
        pass
    return 1024  # fallback: BAAI/bge-m3 默认维度


def should_reindex(
    client: QdrantClient, collection_name: str, target_version: str
) -> bool:
    """判断是否需要重新索引知识库。

    以下情况返回 True（需要重索引）：
      1. Collection 不存在
      2. Collection metadata 为 None（读取失败）
      3. Metadata 中 knowledge_version 字段缺失
      4. Metadata 中 knowledge_version 与 target_version 不匹配

    Args:
        client:          已初始化的 QdrantClient。
        collection_name: Qdrant collection 名称。
        target_version:  期望的知识版本（来自 Settings.knowledge_version）。

    Returns:
        True 表示需要重索引，False 表示版本已匹配可跳过。
    """
    metadata = compute_collection_metadata(client, collection_name)

    if metadata is None:
        logger.info(
            "should_reindex=True: collection '%s' 不存在或元数据读取失败",
            collection_name,
        )
        return True

    current_version = metadata.get("knowledge_version")
    if current_version is None:
        logger.info(
            "should_reindex=True: collection '%s' 缺少 knowledge_version 字段",
            collection_name,
        )
        return True

    if current_version != target_version:
        logger.info(
            "should_reindex=True: collection '%s' 版本不匹配 current=%s target=%s",
            collection_name,
            current_version,
            target_version,
        )
        return True

    logger.info(
        "should_reindex=False: collection '%s' 版本已是最新 version=%s",
        collection_name,
        target_version,
    )
    return False
