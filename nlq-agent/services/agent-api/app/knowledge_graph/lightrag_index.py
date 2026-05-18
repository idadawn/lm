"""LightRAG 检索增强管理器.

设计要点
--------
1. 嵌入式（与 nlq-agent 同进程），不引入新服务进程。
2. Embedding：本地 BGE-small-zh-v1.5（默认）—— 零外网依赖，CPU 即可。
3. LLM：复用 LiteLLM 网关（通过 `get_llm()`），insert 时做实体抽取也走同一个口。
4. 存储：working_dir 下的 JSON 文件（KV/Graph）+ NanoVectorDB（向量），整个目录可挂载持久化。
5. 单例：`_instance` 模块级，首次访问时 lazy init。失败置 None，调用方降级。

公开 API
--------
- ``get_lightrag()``：返回单例 LightRAG（或 None，未启用/初始化失败时）
- ``index_docs(docs)``：批量 insert，docs 是 :func:`build_lightrag_doc` 返回的字典列表
- ``query(question, mode)``：检索，返回 :class:`LightRAGResult`

跟"客户能用起来"的对齐
----------------------
- 任何检索失败都不抛异常，返回空结果让调用方降级；
- 所有 insert / query 调用都打 INFO 日志，便于运维定位；
- citation 来自 doc.source（不让 LLM 编造），下游 response_formatter 把它直接铺到答案末尾。
"""

from __future__ import annotations

import asyncio
import dataclasses
import logging
from pathlib import Path
from typing import Any, Iterable

import numpy as np

from app.core.config import settings

logger = logging.getLogger("nlq-agent.lightrag")

# 模块级单例 + 初始化锁（避免并发首请求时多次构建）
_instance: Any = None
_init_lock = asyncio.Lock()
_init_failed = False  # 启动失败标记，避免每次请求都重试


@dataclasses.dataclass
class LightRAGDoc:
    """喂给 LightRAG 的一条文档片段."""

    id: str  # stable，重新索引时用于去重（如 "node:FurnaceNoParsed"）
    content: str  # 给 LLM 看的人话版本
    source: str  # citation 字符串（如 "Neo4j:FurnaceNoParsed"）
    metadata: dict[str, Any] | None = None


@dataclasses.dataclass
class LightRAGResult:
    """查询结果."""

    answer: str  # LightRAG 综合答案（可能含 LLM 改写）
    chunks: list[dict[str, Any]]  # 召回的原片段（带 source）
    citations: list[str]  # 去重后的 source 列表
    confidence: float  # 0~1，基于 top-1 相似度
    mode: str  # 实际使用的检索模式


def build_lightrag_doc(*, id: str, content: str, source: str, metadata: dict | None = None) -> LightRAGDoc:
    """统一的 doc 构造入口，方便日后加 validator。"""
    return LightRAGDoc(id=id, content=content, source=source, metadata=metadata or {})


# --------------------------------------------------------------------------- #
# Embedding / LLM adapter（适配 LightRAG 的函数签名）
# --------------------------------------------------------------------------- #


_embedder_cache: Any = None


async def _local_embedding_func(texts: list[str]) -> np.ndarray:
    """本地 BGE 嵌入。第一次调用时 lazy 加载模型，常驻内存。"""
    global _embedder_cache
    if _embedder_cache is None:
        try:
            from sentence_transformers import SentenceTransformer
        except ImportError as e:
            raise RuntimeError(
                "sentence-transformers not installed; run `uv add sentence-transformers`"
            ) from e
        logger.info("loading embedding model: %s on %s",
                    settings.LIGHTRAG_EMBEDDING_MODEL, settings.LIGHTRAG_EMBEDDING_DEVICE)
        _embedder_cache = SentenceTransformer(
            settings.LIGHTRAG_EMBEDDING_MODEL,
            device=settings.LIGHTRAG_EMBEDDING_DEVICE,
        )
    # encode 是 sync，但调用频次低（chunk 级），不需要丢线程池
    arr = _embedder_cache.encode(texts, normalize_embeddings=True, show_progress_bar=False)
    return np.asarray(arr, dtype=np.float32)


async def _llm_complete_func(prompt: str, system_prompt: str | None = None,
                              history_messages: list[dict] | None = None, **kwargs: Any) -> str:
    """LightRAG 内部抽取 / 综合答案时用的 LLM 函数，复用项目 LiteLLM 网关。"""
    from app.core.llm_factory import get_llm

    llm = get_llm()
    messages: list[dict[str, str]] = []
    if system_prompt:
        messages.append({"role": "system", "content": system_prompt})
    for h in history_messages or []:
        messages.append({"role": h.get("role", "user"), "content": h.get("content", "")})
    messages.append({"role": "user", "content": prompt})
    try:
        resp = await llm.ainvoke(messages)
        return str(resp.content)
    except Exception as e:
        logger.warning("lightrag LLM call failed: %s", e)
        return ""


# --------------------------------------------------------------------------- #
# 单例初始化
# --------------------------------------------------------------------------- #


async def get_lightrag() -> Any | None:
    """返回 LightRAG 实例（或 None，未启用/失败时）。"""
    global _instance, _init_failed
    if _instance is not None:
        return _instance
    if _init_failed or not settings.LIGHTRAG_ENABLED:
        return None

    async with _init_lock:
        if _instance is not None:  # 双检
            return _instance
        try:
            from lightrag import LightRAG
            from lightrag.utils import EmbeddingFunc

            working_dir = Path(settings.LIGHTRAG_WORKING_DIR)
            working_dir.mkdir(parents=True, exist_ok=True)
            logger.info("initializing LightRAG at %s", working_dir)

            rag = LightRAG(
                working_dir=str(working_dir),
                embedding_func=EmbeddingFunc(
                    embedding_dim=settings.LIGHTRAG_EMBEDDING_DIM,
                    max_token_size=512,
                    func=_local_embedding_func,
                ),
                llm_model_func=_llm_complete_func,
                # 用本地存储，避免引入新服务
                kv_storage="JsonKVStorage",
                graph_storage="NetworkXStorage",
                vector_storage="NanoVectorDBStorage",
                doc_status_storage="JsonDocStatusStorage",
                enable_llm_cache=True,
            )
            await rag.initialize_storages()
            _instance = rag
            logger.info("LightRAG ready")
            return _instance
        except Exception:
            logger.exception("LightRAG initialization failed; will run in degraded mode")
            _init_failed = True
            return None


async def index_docs(docs: Iterable[LightRAGDoc]) -> dict[str, Any]:
    """批量插入文档片段。返回 {inserted, skipped, errors} 统计。"""
    rag = await get_lightrag()
    stats = {"inserted": 0, "skipped": 0, "errors": 0}
    if rag is None:
        logger.warning("LightRAG not ready; index_docs skipped")
        return stats

    for d in docs:
        try:
            # ids 用于去重 / 增量更新；file_paths 作为 citation 来源（LightRAG 内部会把它存到 chunk 的 metadata 里）
            await rag.ainsert(
                d.content,
                ids=[d.id],
                file_paths=[d.source],
            )
            stats["inserted"] += 1
        except Exception:
            logger.exception("index doc failed: id=%s source=%s", d.id, d.source)
            stats["errors"] += 1
    logger.info("index_docs done: %s", stats)
    return stats


async def query(question: str, *, mode: str = "mix", top_k: int = 10) -> LightRAGResult:
    """检索 + 综合答案。任何失败返回空结果（不抛异常）。

    返回：
        LightRAGResult.answer  — LLM 综合后的答案
        LightRAGResult.chunks  — 召回的原片段（带 source 字段，用于 citation）
        LightRAGResult.citations — 去重后的 source 列表
        LightRAGResult.confidence — 0~1，基于召回片段数 + 关键词匹配
    """
    rag = await get_lightrag()
    if rag is None or not question.strip():
        return LightRAGResult(answer="", chunks=[], citations=[], confidence=0.0, mode=mode)

    # 同时拿答案（mode）和上下文（only_need_context）：
    # 两次查询，前者给用户看，后者给我们提 citation。LightRAG 内部带 LLM cache，第二次不烧 token。
    try:
        from lightrag import QueryParam
        ans_param = QueryParam(mode=mode, top_k=top_k, response_type="Multiple Paragraphs")
        ctx_param = QueryParam(mode=mode, top_k=top_k, only_need_context=True)
        raw_answer = await rag.aquery(question, param=ans_param)
        raw_ctx = await rag.aquery(question, param=ctx_param)
    except Exception:
        logger.exception("LightRAG query failed: %s", question[:60])
        return LightRAGResult(answer="", chunks=[], citations=[], confidence=0.0, mode=mode)

    answer = raw_answer if isinstance(raw_answer, str) else str(raw_answer)
    ctx_str = raw_ctx if isinstance(raw_ctx, str) else str(raw_ctx)
    chunks = _parse_chunks_from_context(ctx_str)

    # 去重 source，保持原顺序
    citations: list[str] = []
    seen: set[str] = set()
    for ch in chunks:
        s = (ch.get("source") or "").strip()
        if s and s not in seen:
            seen.add(s)
            citations.append(s)

    confidence = _estimate_confidence(chunks, question, answer)
    return LightRAGResult(
        answer=answer, chunks=chunks, citations=citations,
        confidence=confidence, mode=mode,
    )


def _parse_chunks_from_context(ctx: str) -> list[dict[str, Any]]:
    """从 LightRAG only_need_context 输出里抽出 chunks + 它们的 source（file_path）。

    LightRAG ≥1.4 的 context 格式（YAML/JSON 混合，按模式不同有差异）：
        ----- Sources / Chunks -----
        ```json
        [
          {"id": ..., "content": "...", "file_path": "knowledge_base.json#furnace_no_format"},
          ...
        ]
        ```
        或：
        -----Sources(...)-----
        ```csv
        id,content,file_path
        1,"...","..."
        ```

    我们用宽容解析：尝试 JSON 块 → 失败再 CSV → 都失败回退到段落切片（保底有 content，没 source）。
    """
    chunks: list[dict[str, Any]] = []
    if not ctx:
        return chunks
    import csv
    import io
    import re

    # 1. 尝试抓 ```json [...] ``` 代码块
    for m in re.finditer(r"```json\s*(\[.*?\])\s*```", ctx, re.DOTALL):
        try:
            arr = json.loads(m.group(1))
        except Exception:
            continue
        if isinstance(arr, list):
            for item in arr:
                if not isinstance(item, dict):
                    continue
                src = (item.get("file_path") or item.get("source") or "").strip()
                content = (item.get("content") or item.get("description") or "").strip()
                if content:
                    chunks.append({"content": content[:1000], "source": src,
                                   "entity": item.get("entity"), "id": item.get("id")})

    # 2. 尝试 ```csv ... ``` 代码块
    if not chunks:
        for m in re.finditer(r"```csv\s*(.*?)\s*```", ctx, re.DOTALL):
            try:
                reader = csv.DictReader(io.StringIO(m.group(1)))
                for row in reader:
                    src = (row.get("file_path") or row.get("source") or "").strip()
                    content = (row.get("content") or row.get("description") or "").strip()
                    if content:
                        chunks.append({"content": content[:1000], "source": src,
                                       "entity": row.get("entity"), "id": row.get("id")})
            except Exception:
                pass

    # 3. 兜底：按段落切，source 留空
    if not chunks:
        for block in ctx.split("\n\n"):
            block = block.strip()
            if not block or block.startswith("-----") or block.startswith("```"):
                continue
            chunks.append({"content": block[:800], "source": ""})

    return chunks


def _estimate_confidence(chunks: list[dict[str, Any]], question: str, answer: str) -> float:
    """估算置信度（0~1）。

    评分维度：
    - 召回片段数（多 → 高）
    - 答案中是否包含"我没有足够信息"等 disclaimer（有 → 低）
    - question 关键词在 chunks 里的命中比例
    """
    if not chunks:
        return 0.0
    score = 0.45

    # 召回片段越多越高（最多 +0.2）
    score += min(0.2, len(chunks) * 0.025)

    # disclaimer 惩罚
    ans_lower = (answer or "").lower()
    disclaimers = [
        "没有足够的信息", "没有足够信息", "无法回答", "不知道",
        "i don't know", "i cannot find", "no information",
    ]
    if any(d in ans_lower for d in disclaimers):
        return min(0.5, score * 0.5)  # disclaimer 强压低到 ≤0.5

    # 关键词命中
    q = question.strip()
    if len(q) >= 2:
        # 取 question 里长度 ≥2 的连续子串做匹配
        joined = "\n".join((ch.get("content") or "") for ch in chunks[:5]).lower()
        hits = 0
        for i in range(0, len(q) - 1, 2):
            piece = q[i:i + 3].lower()
            if len(piece) >= 2 and piece in joined:
                hits += 1
        score += min(0.35, hits * 0.05)

    return round(min(1.0, score), 3)


# --------------------------------------------------------------------------- #
# 运维 / 状态查询
# --------------------------------------------------------------------------- #


async def is_ready() -> bool:
    """前端 /api/v1/kg/health 调用。"""
    if not settings.LIGHTRAG_ENABLED:
        return False
    rag = await get_lightrag()
    return rag is not None


async def reset() -> None:
    """清掉单例（测试 / 重建索引后调用）。"""
    global _instance, _init_failed
    if _instance is not None:
        try:
            await _instance.finalize_storages()
        except Exception:
            logger.debug("finalize_storages failed", exc_info=True)
    _instance = None
    _init_failed = False
