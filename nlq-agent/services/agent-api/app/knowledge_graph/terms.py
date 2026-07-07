"""统一业务词表：aliases.json 的加载、文本归一化与同义词扫描.

kb_lookup（知识库匹配）与 chat2sql（schema 关键词筛列）共用这一份词表，
避免各处硬编码关键词列表各自漂移。词表文件按 mtime 热加载，改完即生效。
"""

from __future__ import annotations

import json
import re
import unicodedata
from pathlib import Path

from app.core.logger import logger

_ALIASES_PATH = Path(__file__).parent / "aliases.json"

_aliases_cache: dict[str, dict[str, list[str]]] | None = None
_aliases_mtime: float | None = None
# 词表索引：[(归一化词, 规范名, 节点类型)]，按词长降序（贪心最长优先扫描）
_vocab_cache: list[tuple[str, str, str]] | None = None

# 口语变体 → 书面语（归一化时统一替换，让「是啥」「咋算」命中「是什么」「怎么算」类别名）
_COLLOQUIAL = (("啥", "什么"), ("咋", "怎么"))

# 归一化时去掉的空白与标点；保留下划线/连字符（字段名 F_PS_LOSS、炉号 1甲...-1-4 需要它们）
_PUNCT_RE = re.compile(r"[\s,，.。、？?！!；;：:()（）【】\[\]{}\"'“”‘’「」『』]+")


def normalize_text(s: str) -> str:
    """NFKC（全角→半角）+ 小写 + 去空白标点 + 口语变体归一。中文不受影响。"""
    if not s:
        return ""
    s = unicodedata.normalize("NFKC", s).lower()
    s = _PUNCT_RE.sub("", s)
    for src, dst in _COLLOQUIAL:
        s = s.replace(src, dst)
    return s


def _load_aliases() -> dict[str, dict[str, list[str]]]:
    """按文件 mtime 判断是否需要重新加载；重载时同步失效词表索引。"""
    global _aliases_cache, _aliases_mtime, _vocab_cache
    try:
        mtime = _ALIASES_PATH.stat().st_mtime
    except FileNotFoundError:
        logger.warning("[terms] aliases.json not found at %s", _ALIASES_PATH)
        return {}

    if _aliases_cache is None or _aliases_mtime != mtime:
        try:
            with _ALIASES_PATH.open("r", encoding="utf-8") as f:
                data = json.load(f)
            _aliases_cache = {
                k: v for k, v in data.items() if not k.startswith("_") and isinstance(v, dict)
            }
            _aliases_mtime = mtime
            _vocab_cache = None
            total = sum(len(g) for g in _aliases_cache.values())
            logger.info("[terms] loaded %d canonical terms from aliases.json", total)
        except Exception:
            logger.exception("[terms] failed to load aliases.json")
            return {}
    return _aliases_cache or {}


def _vocab() -> list[tuple[str, str, str]]:
    global _vocab_cache
    data = _load_aliases()
    if _vocab_cache is None:
        items: list[tuple[str, str, str]] = []
        seen: set[tuple[str, str]] = set()
        for node_type, group in data.items():
            for canonical, synonyms in group.items():
                for term in (canonical, *(synonyms or [])):
                    nt = normalize_text(str(term))
                    # 单字符词（如 "P"、"S"）几乎必然误命中，跳过
                    if len(nt) < 2:
                        continue
                    key = (nt, canonical)
                    if key in seen:
                        continue
                    seen.add(key)
                    items.append((nt, canonical, node_type))
        items.sort(key=lambda x: len(x[0]), reverse=True)
        _vocab_cache = items
    return _vocab_cache


def extract_terms(question: str) -> list[dict[str, str]]:
    """在问题中扫描业务词表，返回命中的规范词。

    贪心最长优先：命中「叠片系数」后，该段文本不再让更短的「叠片」重复命中。
    返回 [{"canonical": 规范名, "type": 节点类型, "matched": 命中的归一化词}]。
    """
    q = normalize_text(question)
    if not q:
        return []
    # 第一遍：贪心最长优先圈定命中的词面（span 不重叠）
    matched_terms: set[str] = set()
    consumed = [False] * len(q)
    for nt, _canonical, _node_type in _vocab():
        if nt in matched_terms:
            continue
        start = q.find(nt)
        while start != -1:
            if not any(consumed[start : start + len(nt)]):
                for i in range(start, start + len(nt)):
                    consumed[i] = True
                matched_terms.add(nt)
                break
            start = q.find(nt, start + 1)
    # 第二遍：同一词面可能映射多个规范名（如「铁损」→ PsLoss 和 F_PS_LOSS），全部收集
    hits: list[dict[str, str]] = []
    seen_canonical: set[str] = set()
    for nt, canonical, node_type in _vocab():
        if nt in matched_terms and canonical not in seen_canonical:
            seen_canonical.add(canonical)
            hits.append({"canonical": canonical, "type": node_type, "matched": nt})
    return hits


def expand_keywords(question: str) -> list[str]:
    """把问题里的业务词扩展为规范名 + 对应 DB 列名形态，供 schema 表/列匹配。

    例：「铁损」→ ["PsLoss", "铁损", "F_PS_LOSS"]。返回去重后保持顺序的列表。
    """
    out: list[str] = []
    for hit in extract_terms(question):
        canonical = hit["canonical"]
        out.append(canonical)
        if hit["matched"] != normalize_text(canonical):
            out.append(hit["matched"])
        # formula 规范名是逻辑驼峰名（PsLoss），补充 F_ 蛇形列名帮助按列名匹配
        if hit["type"] == "formula" and not canonical.upper().startswith("F_"):
            snake = re.sub(r"(?<!^)([A-Z])", r"_\1", canonical).upper()
            out.append(f"F_{snake}")
    seen: set[str] = set()
    return [k for k in out if not (k in seen or seen.add(k))]
