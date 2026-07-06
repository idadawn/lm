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
from app.knowledge_graph.terms import extract_terms, normalize_text

_KB_PATH = Path(__file__).parent / "knowledge_base.json"

# 数据查询意图标记：带这些词的问题要的是"数",不是"定义"，应交给数据查询链路。
# 命中标记时把阈值抬到 0.7——只有 alias 级强命中（问题里真写了完整定义式说法）才由 KB 接管，
# 「铁损」「一次交检」这类关键词组合不足以抢答。
_DATA_QUERY_MARKERS = (
    "平均", "均值", "最大", "最小", "最高", "最低", "多少", "几条", "几卷",
    "分布", "趋势", "对比", "排名", "排行", "统计", "汇总", "明细", "导出", "列出",
    "查一下", "查询", "上月", "上个月", "本月", "今天", "昨天", "最近", "同比", "环比",
)


def _looks_like_data_query(q_norm: str) -> bool:
    return any(m in q_norm for m in _DATA_QUERY_MARKERS)

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


def lookup_kb(question: str, *, min_score: float = 0.45) -> dict[str, Any] | None:
    """在知识库里找最匹配的条目。

    问题与词条两侧都先经 terms.normalize_text 归一化（全角→半角、去空白标点、
    口语变体「啥/咋」→「什么/怎么」），再做子串比对；同时用 aliases.json 做
    同义词桥接——问题里命中的规范词（如「磁损」→ PsLoss）拼进匹配文本参与比对。

    评分规则（满分 1.0+）：
    - alias 归一化后与问题相等 → 直接返回
    - 任一 alias 是匹配文本的子串 → +0.7（强匹配）
    - keyword 命中 → 每个 +0.25，封顶 +0.5（两个关键词即可过阈值）
    - title 归一化后（≥4 字）整体出现在问题中 → +0.2
    数据查询守卫：问题带「平均/分布/多少/上月…」等取数标记时，阈值抬到 0.7，
    避免定义类词条抢答本该走 SQL 的数据问题。
    返回最高分条目（≥ min_score）；否则 None。
    """
    if not question:
        return None

    q = normalize_text(question)
    if not q:
        return None

    if _looks_like_data_query(q):
        min_score = max(min_score, 0.7)

    # 同义词桥接：命中的规范词拼进匹配文本，用 § 分隔避免跨词误配
    bridged = [normalize_text(t["canonical"]) for t in extract_terms(question)]
    q_ext = "§".join([q, *bridged]) if bridged else q

    kb = _load_kb()
    entries = kb.get("entries", [])

    best_entry: dict[str, Any] | None = None
    best_score: float = 0.0

    for entry in entries:
        score = 0.0
        aliases = entry.get("aliases", []) or []
        keywords = entry.get("keywords", []) or []
        title = entry.get("title", "")

        # 1. alias 精确匹配 / 子串匹配（双侧归一化）
        for alias in aliases:
            al = normalize_text(alias)
            if len(al) < 2:
                continue
            if al == q:
                return entry  # 完全相同直接返回
            if al in q_ext:
                score = max(score, 0.7)

        # 2. keyword 命中数（双侧归一化，含同义词桥接文本）
        kw_hits = 0
        for kw in keywords:
            kw_norm = normalize_text(kw)
            if len(kw_norm) >= 2 and kw_norm in q_ext:
                kw_hits += 1
        if kw_hits:
            score += min(0.5, kw_hits * 0.25)

        # 3. title 整体命中（中文标题无空格，按整体子串比对）
        title_norm = normalize_text(title)
        if len(title_norm) >= 4 and title_norm in q:
            score += 0.2

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
