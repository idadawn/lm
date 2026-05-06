"""Unit tests for reasoning mapper (mapper.py).

Covers:
  - subtype_to_kind: 8 subtype strings → correct ReasoningStepKind
  - subtype_to_kind: unknown subtype → "fallback"
  - build_terminal_step:
      row_count=0  → kind="fallback"
      row_count=1  → kind="record"
      row_count>1 + 含判级关键词 → kind="grade"
      row_count>1 + 不含判级关键词 → kind="record"
      row_count=1 + 含判级关键词 → kind="grade"
"""

from __future__ import annotations

import pandas as pd
import pytest

from app.api.schemas import ReasoningStep
from app.reasoning.mapper import build_terminal_step, subtype_to_kind


# ---------------------------------------------------------------------------
# subtype_to_kind
# ---------------------------------------------------------------------------


class TestSubtypeToKind:
    """All 8 ADR-6 subtype values map to the correct kind."""

    def test_ddl_maps_to_spec(self):
        assert subtype_to_kind("ddl") == "spec"

    def test_qa_maps_to_spec(self):
        assert subtype_to_kind("qa") == "spec"

    def test_terminology_maps_to_rule(self):
        assert subtype_to_kind("terminology") == "rule"

    def test_judgment_rule_maps_to_rule(self):
        assert subtype_to_kind("judgment_rule") == "rule"

    def test_condition_maps_to_condition(self):
        assert subtype_to_kind("condition") == "condition"

    def test_grade_maps_to_grade(self):
        assert subtype_to_kind("grade") == "grade"

    def test_record_maps_to_record(self):
        assert subtype_to_kind("record") == "record"

    def test_fallback_maps_to_fallback(self):
        assert subtype_to_kind("fallback") == "fallback"

    def test_unknown_maps_to_fallback(self):
        assert subtype_to_kind("nonexistent_subtype") == "fallback"

    def test_empty_string_maps_to_fallback(self):
        assert subtype_to_kind("") == "fallback"


# ---------------------------------------------------------------------------
# build_terminal_step
# ---------------------------------------------------------------------------


class TestBuildTerminalStep:
    """Decision table tests for build_terminal_step."""

    # --- row_count == 0 -------------------------------------------------

    def test_empty_df_returns_fallback(self):
        df = pd.DataFrame()
        step = build_terminal_step(df, question="查询铁损", sql="SELECT 1")
        assert isinstance(step, ReasoningStep)
        assert step.kind == "fallback"

    def test_empty_df_fallback_label(self):
        df = pd.DataFrame()
        step = build_terminal_step(df, question="查询铁损", sql="SELECT 1")
        assert step.label  # non-empty label

    # --- row_count == 1 without grade keyword --------------------------

    def test_single_row_no_grade_keyword_returns_record(self):
        df = pd.DataFrame({"iron_loss": [1.05], "spec": ["120"]})
        step = build_terminal_step(df, question="查询铁损记录", sql="SELECT 1")
        assert step.kind == "record"

    def test_single_row_record_label(self):
        df = pd.DataFrame({"iron_loss": [1.05]})
        step = build_terminal_step(df, question="查询铁损", sql="SELECT 1")
        assert "1" in step.label  # label mentions 1 record

    # --- row_count > 1 without grade keyword ---------------------------

    def test_multi_row_no_grade_keyword_returns_record(self):
        df = pd.DataFrame({"iron_loss": [1.05, 1.10, 0.98]})
        step = build_terminal_step(df, question="查询所有铁损数据", sql="SELECT 1")
        assert step.kind == "record"

    def test_multi_row_record_label_contains_count(self):
        df = pd.DataFrame({"val": list(range(5))})
        step = build_terminal_step(df, question="查询数据", sql="SELECT 1")
        assert step.kind == "record"
        assert "5" in step.label

    # --- row_count > 1 with grade keyword ------------------------------

    def test_grade_keyword_判级_returns_grade(self):
        df = pd.DataFrame({"grade": ["A"], "iron_loss": [1.05]})
        step = build_terminal_step(df, question="查询铁损判级结果", sql="SELECT 1")
        assert step.kind == "grade"

    def test_grade_keyword_等级_returns_grade(self):
        df = pd.DataFrame({"grade": ["B"]})
        step = build_terminal_step(df, question="产品等级是什么", sql="SELECT 1")
        assert step.kind == "grade"

    def test_grade_keyword_级别_returns_grade(self):
        df = pd.DataFrame({"level": ["C"]})
        step = build_terminal_step(df, question="查看级别", sql="SELECT 1")
        assert step.kind == "grade"

    def test_grade_keyword_评级_returns_grade(self):
        df = pd.DataFrame({"result": ["合格"]})
        step = build_terminal_step(df, question="如何评级", sql="SELECT 1")
        assert step.kind == "grade"

    def test_grade_keyword_定级_returns_grade(self):
        df = pd.DataFrame({"result": ["A"]})
        step = build_terminal_step(df, question="定级结果", sql="SELECT 1")
        assert step.kind == "grade"

    # --- row_count == 1 with grade keyword ----------------------------

    def test_single_row_with_grade_keyword_returns_grade(self):
        df = pd.DataFrame({"grade": ["A"]})
        step = build_terminal_step(df, question="这个产品判级结果", sql="SELECT 1")
        assert step.kind == "grade"

    # --- meta fields ---------------------------------------------------

    def test_terminal_step_has_meta(self):
        df = pd.DataFrame({"a": [1]})
        step = build_terminal_step(df, question="query", sql="SELECT 1")
        assert step.meta is not None
        assert "row_count" in step.meta

    def test_fallback_meta_has_row_count_zero(self):
        df = pd.DataFrame()
        step = build_terminal_step(df, question="query", sql="SELECT 1")
        assert step.meta is not None
        assert step.meta.get("row_count") == 0
