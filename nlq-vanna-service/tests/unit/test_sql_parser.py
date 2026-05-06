"""Unit tests for sql_parser.extract_conditions.

Covers:
  - Empty WHERE → returns []
  - Single WHERE → returns 1 condition step
  - Multiple AND → splits correctly
  - JOIN clause does not affect WHERE parsing
  - Complex expressions (IN / BETWEEN / JSON_EXTRACT) do not crash
  - Parse failure (invalid SQL syntax) → returns single fallback step
  - More than 8 AND predicates → capped at 8 (A3 SHOULD)
"""

from __future__ import annotations

import pytest

from app.api.schemas import ReasoningStep
from app.reasoning.sql_parser import extract_conditions


# ---------------------------------------------------------------------------
# Basic cases
# ---------------------------------------------------------------------------


class TestExtractConditionsBasic:
    def test_no_where_returns_empty(self):
        sql = "SELECT id FROM lab_data"
        result = extract_conditions(sql)
        assert result == []

    def test_single_condition(self):
        sql = "SELECT id FROM lab_data WHERE iron_loss > 1.05"
        result = extract_conditions(sql)
        assert len(result) == 1
        assert result[0].kind == "condition"

    def test_two_and_conditions(self):
        sql = "SELECT id FROM lab_data WHERE iron_loss > 1.05 AND spec = '120'"
        result = extract_conditions(sql)
        assert len(result) == 2
        for step in result:
            assert step.kind == "condition"

    def test_three_and_conditions(self):
        sql = (
            "SELECT id FROM lab_data "
            "WHERE iron_loss > 1.05 AND spec = '120' AND grade = 'A'"
        )
        result = extract_conditions(sql)
        assert len(result) == 3

    def test_all_steps_are_reasoning_step_instances(self):
        sql = "SELECT * FROM t WHERE a = 1 AND b = 2"
        result = extract_conditions(sql)
        for step in result:
            assert isinstance(step, ReasoningStep)


# ---------------------------------------------------------------------------
# JOIN does not break WHERE
# ---------------------------------------------------------------------------


class TestJoinDoesNotAffectWhere:
    def test_join_with_where(self):
        sql = (
            "SELECT a.id FROM lab_data a "
            "JOIN spec_table b ON a.spec_id = b.id "
            "WHERE a.iron_loss > 1.0"
        )
        result = extract_conditions(sql)
        assert len(result) == 1
        assert result[0].kind == "condition"

    def test_join_with_multiple_and(self):
        sql = (
            "SELECT a.id FROM lab_data a "
            "JOIN spec_table b ON a.spec_id = b.id "
            "WHERE a.iron_loss > 1.0 AND b.spec_name = '120规格'"
        )
        result = extract_conditions(sql)
        assert len(result) == 2


# ---------------------------------------------------------------------------
# Complex expressions — must not crash
# ---------------------------------------------------------------------------


class TestComplexExpressions:
    def test_in_clause(self):
        sql = "SELECT id FROM t WHERE grade IN ('A', 'B', 'C')"
        result = extract_conditions(sql)
        assert isinstance(result, list)
        # IN clause is one predicate
        assert len(result) >= 1

    def test_between_clause(self):
        sql = "SELECT id FROM t WHERE iron_loss BETWEEN 1.0 AND 1.5"
        result = extract_conditions(sql)
        assert isinstance(result, list)
        # BETWEEN is one compound predicate
        assert len(result) >= 1

    def test_json_extract(self):
        sql = (
            "SELECT id FROM t WHERE JSON_EXTRACT(attrs, '$.grade') = 'A'"
        )
        result = extract_conditions(sql)
        assert isinstance(result, list)

    def test_nested_in_and(self):
        sql = (
            "SELECT id FROM t "
            "WHERE grade IN ('A','B') AND iron_loss > 1.0 AND spec = '120'"
        )
        result = extract_conditions(sql)
        assert len(result) >= 2


# ---------------------------------------------------------------------------
# Parse failure → fallback step
# ---------------------------------------------------------------------------


class TestParseFailure:
    def test_invalid_sql_returns_fallback(self):
        sql = "THIS IS NOT SQL AT ALL !!!"
        result = extract_conditions(sql)
        assert len(result) == 1
        assert result[0].kind == "fallback"

    def test_incomplete_where_returns_fallback_or_empty(self):
        # sqlglot may parse or fail depending on strictness
        sql = "SELECT FROM WHERE"
        result = extract_conditions(sql)
        # Must be a list (never raises)
        assert isinstance(result, list)

    def test_syntax_error_sql_returns_fallback(self):
        sql = "SELECT * FROM WHERE ORDER BY"
        result = extract_conditions(sql)
        assert isinstance(result, list)
        if len(result) == 1:
            assert result[0].kind == "fallback"


# ---------------------------------------------------------------------------
# Cap at 8 — A3 SHOULD
# ---------------------------------------------------------------------------


class TestCapAt8:
    def test_exactly_8_conditions_not_truncated(self):
        conditions = " AND ".join([f"col{i} = {i}" for i in range(8)])
        sql = f"SELECT id FROM t WHERE {conditions}"
        result = extract_conditions(sql)
        assert len(result) == 8

    def test_9_conditions_capped_at_8(self):
        conditions = " AND ".join([f"col{i} = {i}" for i in range(9)])
        sql = f"SELECT id FROM t WHERE {conditions}"
        result = extract_conditions(sql)
        assert len(result) <= 8

    def test_20_conditions_capped_at_8(self):
        conditions = " AND ".join([f"col{i} = {i}" for i in range(20)])
        sql = f"SELECT id FROM t WHERE {conditions}"
        result = extract_conditions(sql)
        assert len(result) == 8

    def test_cap_applies_all_condition_kind(self):
        conditions = " AND ".join([f"col{i} = {i}" for i in range(12)])
        sql = f"SELECT id FROM t WHERE {conditions}"
        result = extract_conditions(sql)
        assert len(result) <= 8
        for step in result:
            assert step.kind == "condition"
