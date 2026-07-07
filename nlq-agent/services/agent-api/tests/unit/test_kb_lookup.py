"""kb_lookup / terms 关键词命中回归测试.

背景：业务反馈「很多关键词命中不了」。根因是旧评分公式里 keyword 分支
封顶 0.45 + title 0.10 = 0.55，永远低于 min_score 0.6——没有 alias 字面
子串命中的问题必 miss；且无全角/半角、空白、口语变体归一化。

本文件锁定修复后的行为：真实措辞的问题必须命中预期词条。
纯函数测试，不依赖数据库 / LLM / 网络。
"""

from __future__ import annotations

import pytest

from app.knowledge_graph.kb_lookup import lookup_kb
from app.knowledge_graph.terms import expand_keywords, extract_terms, normalize_text

# --------------------------------------------------------------------------- #
# normalize_text
# --------------------------------------------------------------------------- #


@pytest.mark.parametrize(
    ("raw", "expected"),
    [
        ("Ａ类占比？", "a类占比"),  # 全角字母 + 全角问号
        ("labeling 和 First_Inspection 区别", "labeling和first_inspection区别"),
        ("甲班是啥", "甲班是什么"),  # 口语变体
        ("首检合格率咋算的", "首检合格率怎么算的"),
        ("  炉号　格式  ", "炉号格式"),  # 半角/全角空白
        ("", ""),
    ],
)
def test_normalize_text(raw: str, expected: str) -> None:
    assert normalize_text(raw) == expected


# --------------------------------------------------------------------------- #
# extract_terms / expand_keywords（aliases.json 同义词桥接）
# --------------------------------------------------------------------------- #


def test_extract_terms_maps_synonym_to_canonical() -> None:
    canonicals = {t["canonical"] for t in extract_terms("上月铁损超标的炉子有哪些")}
    assert "PsLoss" in canonicals
    assert "F_PS_LOSS" in canonicals


def test_extract_terms_longest_match_wins() -> None:
    # 「叠片系数」整体命中 LamFactor，不应被更短的词拆散
    canonicals = {t["canonical"] for t in extract_terms("查一下叠片系数的趋势")}
    assert "LamFactor" in canonicals


def test_expand_keywords_includes_db_column_form() -> None:
    keywords = expand_keywords("铁损和矫顽力最近怎么样")
    assert "PsLoss" in keywords
    assert "F_PS_LOSS" in keywords
    assert "Hc" in keywords


def test_extract_terms_empty_question() -> None:
    assert extract_terms("") == []


# --------------------------------------------------------------------------- #
# lookup_kb 命中用例（真实措辞）
# --------------------------------------------------------------------------- #

HIT_CASES = [
    # 旧算法本来就能命中的（alias 子串），修复后不能退化
    ("炉号是怎么组成的", "furnace_no_format"),
    ("一交合格率怎么算", "first_inspection_rate"),
    ("有几条生产线", "line_dimension"),
    ("单卷重量是什么字段", "single_coil_weight"),
    # 旧算法必 miss：keyword 组合过不了 0.6 阈值
    ("炉号的格式是什么样的", "furnace_no_format"),
    ("labeling 和 first_inspection 的区别", "labeling_vs_first_inspection"),
    # 旧算法必 miss：口语变体 / 全角
    ("甲班是啥", "shift_dimension"),
    ("首检合格率咋算的", "first_inspection_rate"),
    ("Ａ类占比怎么看", "quality_level_distribution"),
    ("贴标和一次交检有啥区别", "labeling_vs_first_inspection"),
    # 新增词条
    ("铁损是什么意思", "ps_loss"),
    ("什么是矫顽力", "hc"),
    ("激磁功率是什么", "ss_power"),
    ("叠片系数是啥", "lam_factor"),
    ("环样性能检测是什么", "magnetic_data"),
    ("炉号带K是什么", "scratch_k"),
    ("单片性能检测是什么", "single_sheet"),
    ("产品规格是什么", "product_spec"),
    ("叠片数据是什么", "raw_data"),
]


@pytest.mark.parametrize(("question", "expected_id"), HIT_CASES)
def test_lookup_kb_hits(question: str, expected_id: str) -> None:
    entry = lookup_kb(question)
    assert entry is not None, f"应命中 {expected_id}，实际 miss: {question!r}"
    assert entry.get("id") == expected_id, (
        f"{question!r} 命中了 {entry.get('id')}，预期 {expected_id}"
    )


# --------------------------------------------------------------------------- #
# lookup_kb 不应命中的用例（防误报）
# --------------------------------------------------------------------------- #

MISS_CASES = [
    "今天天气怎么样",
    "帮我写一首诗",
    "上个月的产量数据导出一下",  # 数据查询，应走 chat2sql，不该被 KB 抢答
    # 数据查询守卫：提到指标名但要的是"数"，必须放行给数据查询链路
    "上月甲班Ps铁损平均值是多少",
    "最近一周铁损趋势怎么样",
    "上个月一次交检合格率是多少",
]


@pytest.mark.parametrize("question", MISS_CASES)
def test_lookup_kb_misses(question: str) -> None:
    entry = lookup_kb(question)
    assert entry is None, f"不应命中却命中了 {entry and entry.get('id')}: {question!r}"


def test_lookup_kb_empty_question() -> None:
    assert lookup_kb("") is None
