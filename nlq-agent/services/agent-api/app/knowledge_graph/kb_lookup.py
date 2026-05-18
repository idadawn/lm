"""业务知识库查找入口（双层）.

第 1 层：LightRAG 检索（如果启用）
   - 语义召回 + 实体抽取后的图谱关系
   - 覆盖 Neo4j 节点 + 全部 JSON 字典 + DB 配置 + docs

第 2 层：静态 knowledge_base.json（始终可用）
   - 不依赖 LightRAG 也能跑（feature flag 关闭、初始化失败时）
   - 用 alias / keyword 加权匹配，业务专家可直接编辑文件

匹配优先级：
   - LightRAG 命中且 confidence >= LIGHTRAG_CONFIDENCE_THRESHOLD → 走它
   - 否则降级到静态匹配
   - 都没命中 → 返回 None，调用方走原有路由
"""

from __future__ import annotations

import json
from pathlib import Path
from typing import Any

from app.core.logger import logger

_KB_PATH = Path(__file__).parent / "knowledge_base.json"

# 模块级缓存（FastAPI 单 worker；多 worker 时每 worker 各持一份）
_kb_cache: dict[str, Any] | None = None
_kb_mtime: float | None = None


def _load_kb() -> dict[str, Any]:
    """按文件 mtime 判断是否需要重新加载。dev 时改完 JSON 立即生效。"""
    global _kb_cache, _kb_mtime
    try:
        mtime = _KB_PATH.stat().st_mtime
    except FileNotFoundError:
        logger.warning("[kb_lookup] knowledge_base.json not found at %s", _KB_PATH)
        return {"entries": []}

    if _kb_cache is None or _kb_mtime != mtime:
        try:
            with _KB_PATH.open("r", encoding="utf-8") as f:
                _kb_cache = json.load(f)
            _kb_mtime = mtime
            entries_count = len(_kb_cache.get("entries", []))
            logger.info("[kb_lookup] loaded %d entries from knowledge_base.json", entries_count)
        except Exception as e:
            logger.exception("[kb_lookup] failed to load knowledge_base.json: %s", e)
            return {"entries": []}
    return _kb_cache or {"entries": []}


def lookup_kb(question: str, *, min_score: float = 0.6) -> dict[str, Any] | None:
    """在知识库里找最匹配的条目。

    匹配方式：大小写不敏感（对中文等价 identity，对英文/字段名 F_LABELING vs labeling 兼容）。

    评分规则（满分 1.0+）：
    - alias 与 question 字面相等（忽略大小写）→ 直接返回
    - 任一 alias 是 question 的子串 → +0.7（强匹配）
    - keyword 在 question 中命中 → 每个 +0.15（最高 +0.45）
    - title 中的关键词出现 → +0.1
    返回最高分条目（≥ min_score）；否则 None。
    """
    if not question:
        return None

    q = question.strip().lower()

    kb = _load_kb()
    entries = kb.get("entries", [])

    best_entry: dict[str, Any] | None = None
    best_score: float = 0.0

    for entry in entries:
        score = 0.0
        aliases = entry.get("aliases", []) or []
        keywords = entry.get("keywords", []) or []
        title = entry.get("title", "")

        # 1. alias 精确匹配 / 子串匹配（大小写不敏感）
        for alias in aliases:
            if not alias:
                continue
            al = alias.lower()
            if al == q:
                return entry  # 完全相同直接返回
            if al in q:
                score = max(score, 0.7)

        # 2. keyword 命中数（大小写不敏感）
        kw_hits = sum(1 for kw in keywords if kw and kw.lower() in q)
        if kw_hits:
            score += min(0.45, kw_hits * 0.15)

        # 3. title 关键词命中
        if title:
            title_tokens = [t for t in title.replace("?", "").replace("？", "").split() if len(t) >= 2]
            for tok in title_tokens:
                if tok.lower() in q:
                    score += 0.1
                    break

        if score > best_score:
            best_score = score
            best_entry = entry

    if best_entry is not None and best_score >= min_score:
        logger.info(
            "[kb_lookup] matched entry id=%s topic=%s score=%.2f question=%r",
            best_entry.get("id"), best_entry.get("topic"), best_score, question[:60],
        )
        return best_entry

    logger.debug("[kb_lookup] no match for question=%r (best_score=%.2f)", question[:60], best_score)
    return None


def list_all_entries() -> list[dict[str, Any]]:
    """暴露给 /api/v1/kg/knowledge-base 之类的诊断接口用。"""
    kb = _load_kb()
    return kb.get("entries", [])


# --------------------------------------------------------------------------- #
# LightRAG 双层入口：优先用语义检索，失败/未启用时降级到静态匹配
# --------------------------------------------------------------------------- #


async def lookup_kb_smart(question: str) -> dict[str, Any] | None:
    """优先 LightRAG，降级到静态 lookup_kb。

    返回结构（与静态版兼容，但多了 `lightrag` 字段）：
        {
          "id": str,
          "topic": str,
          "answer": str,             # 给前端展示的 markdown
          "citations": list[str],    # source 列表（可能空）
          "confidence": float,       # 0~1
          "lightrag": bool,          # True 表示走的 LightRAG
        }
    """
    # 先试 LightRAG（语义检索）
    try:
        from app.core.config import settings
        from app.knowledge_graph import lightrag_index

        if settings.LIGHTRAG_ENABLED:
            result = await lightrag_index.query(question, mode="hybrid", top_k=8)
            if result.confidence >= settings.LIGHTRAG_CONFIDENCE_THRESHOLD and result.answer:
                logger.info(
                    "[kb_lookup] LightRAG hit | conf=%.2f | citations=%d | question=%r",
                    result.confidence, len(result.citations), question[:60],
                )
                return {
                    "id": "lightrag",
                    "topic": "lightrag",
                    "answer": result.answer,
                    "citations": result.citations[:5],
                    "confidence": result.confidence,
                    "chunks": result.chunks[:5],
                    "lightrag": True,
                }
            # LightRAG 没到高置信度阈值，但可能是有用上下文 —— 暂不直接答，让调用方决定要不要注入下游 prompt
            if result.chunks:
                logger.debug(
                    "[kb_lookup] LightRAG low-confidence (conf=%.2f); fall through",
                    result.confidence,
                )
    except Exception:
        logger.exception("[kb_lookup] LightRAG path failed; fallback to static")

    # 降级：静态匹配
    hit = lookup_kb(question)
    if hit:
        return {
            "id": hit.get("id", "static"),
            "topic": hit.get("topic", ""),
            "answer": hit.get("answer", ""),
            "citations": [f"knowledge_base.json#{hit.get('id', '')}"],
            "confidence": 0.7,  # 静态匹配的固定置信度
            "chunks": [],
            "lightrag": False,
        }
    return None


async def fetch_lightrag_context(question: str, top_k: int = 5) -> list[dict[str, Any]]:
    """给下游 agent（chat2sql 等）拿"中等置信度"的上下文片段，注入到它们的 prompt。

    返回 list[{content, source}]，可能为空。不抛异常。
    """
    try:
        from app.core.config import settings
        from app.knowledge_graph import lightrag_index

        if not settings.LIGHTRAG_ENABLED:
            return []
        # 用 local 模式拿实体级片段，比 hybrid 更准更快
        result = await lightrag_index.query(question, mode="local", top_k=top_k)
        return result.chunks[:top_k]
    except Exception:
        logger.exception("[kb_lookup] fetch_lightrag_context failed")
        return []
