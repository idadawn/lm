"""
Unit tests for _evaluate_condition operator-class branching.

覆盖所有算子分类：数值比较、列表匹配(IN/NOT IN)、范围匹配(BETWEEN)，
以及类型不匹配时的 ConditionEvalError。
"""

from __future__ import annotations

import pytest

from src.pipelines.stage2.data_sql_agent import ConditionEvalError, DataSQLAgent
from src.services.database import DatabaseService
from src.services.llm_client import LLMClient
from src.services.sse_emitter import SSEEmitter


@pytest.fixture
def agent() -> DataSQLAgent:
    """创建测试用 DataSQLAgent（依赖全部 mock）。"""
    llm = __import__("unittest.mock", fromlist=["MagicMock"]).MagicMock(spec=LLMClient)
    db = __import__("unittest.mock", fromlist=["MagicMock"]).MagicMock(spec=DatabaseService)
    emitter = SSEEmitter()
    return DataSQLAgent(llm=llm, db=db, emitter=emitter)


class TestNumericComparison:
    """数值比较算子：<=, >=, =, <, >"""

    def test_greater_equal_true(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition(80, ">=", 75) is True

    def test_greater_equal_false(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition(70, ">=", 75) is False

    def test_less_equal_true(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition(75, "<=", 80) is True

    def test_equal_true(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition(75, "=", 75) is True

    def test_less_than(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition(74, "<", 75) is True
        assert agent._evaluate_condition(75, "<", 75) is False

    def test_greater_than(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition(76, ">", 75) is True
        assert agent._evaluate_condition(75, ">", 75) is False

    def test_string_numeric_coercion(self, agent: DataSQLAgent) -> None:
        """字符串数值可以比较。"""
        assert agent._evaluate_condition("80", ">=", "75") is True

    def test_non_numeric_raises(self, agent: DataSQLAgent) -> None:
        """非数值类型比较抛 ConditionEvalError。"""
        with pytest.raises(ConditionEvalError):
            agent._evaluate_condition("abc", ">=", 75)

    def test_unknown_operator_raises(self, agent: DataSQLAgent) -> None:
        """未知算子抛 ConditionEvalError。"""
        with pytest.raises(ConditionEvalError, match="未知算子"):
            agent._evaluate_condition(80, "LIKE", "75")


class TestListMatching:
    """列表匹配算子：IN, NOT IN"""

    def test_in_true(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition("A类", "IN", ["A类", "B类"]) is True

    def test_in_false(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition("不合格", "IN", ["A类", "B类"]) is False

    def test_not_in_true(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition("不合格", "NOT IN", ["A类", "B类"]) is True

    def test_not_in_false(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition("A类", "NOT IN", ["A类", "B类"]) is False

    def test_in_requires_list(self, agent: DataSQLAgent) -> None:
        """IN 算子期望值非列表时抛 ConditionEvalError。"""
        with pytest.raises(ConditionEvalError, match="列表类型"):
            agent._evaluate_condition("A类", "IN", "单值")

    def test_in_case_insensitive_operator(self, agent: DataSQLAgent) -> None:
        """算子大小写不敏感。"""
        assert agent._evaluate_condition("A类", "in", ["A类"]) is True


class TestRangeMatching:
    """范围匹配算子：BETWEEN"""

    def test_between_in_range(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition(50, "BETWEEN", [10, 100]) is True

    def test_between_at_boundary(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition(10, "BETWEEN", [10, 100]) is True
        assert agent._evaluate_condition(100, "BETWEEN", [10, 100]) is True

    def test_between_out_of_range(self, agent: DataSQLAgent) -> None:
        assert agent._evaluate_condition(5, "BETWEEN", [10, 100]) is False

    def test_between_requires_two_elements(self, agent: DataSQLAgent) -> None:
        """BETWEEN 期望值不是两元素列表时抛 ConditionEvalError。"""
        with pytest.raises(ConditionEvalError, match="两个元素"):
            agent._evaluate_condition(50, "BETWEEN", [10])

    def test_between_non_numeric_raises(self, agent: DataSQLAgent) -> None:
        """BETWEEN 操作数非数值时抛 ConditionEvalError。"""
        with pytest.raises(ConditionEvalError):
            agent._evaluate_condition("abc", "BETWEEN", [10, 100])
