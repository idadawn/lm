"""
Unit tests for the diagnostic field registry.

覆盖：
- 注册表查不到 → graceful skip
- 注册表命中 → 生成正确诊断 SELECT
- 添加新 metric 项 → 不需要改函数体即可工作（monkeypatch 注入测试 spec）
"""

from __future__ import annotations

from unittest.mock import AsyncMock, MagicMock

import pytest

from src.models.schemas import (
    AgentContext,
    FilterCondition,
    IntentClassification,
    IntentType,
    MetricDefinition,
    ReasoningStep,
    ReasoningStepKind,
)
from src.pipelines.stage2.data_sql_agent import (
    DIAGNOSTIC_FIELD_REGISTRY,
    DataSQLAgent,
    DiagnosticSpec,
    canonical_field_key,
)
from src.services.database import DatabaseService
from src.services.llm_client import LLMClient
from src.services.sse_emitter import SSEEmitter


@pytest.fixture
def agent() -> DataSQLAgent:
    llm = MagicMock(spec=LLMClient)
    db = MagicMock(spec=DatabaseService)
    emitter = SSEEmitter()
    return DataSQLAgent(llm=llm, db=db, emitter=emitter)


@pytest.fixture
def base_context() -> AgentContext:
    return AgentContext(
        user_question="test",
        intent=IntentClassification(intent=IntentType.STATISTICAL, confidence=1.0),
    )


class TestCanonicalFieldKey:
    """验证 canonical_field_key 归一化行为。"""

    def test_chinese_key_returns_self(self) -> None:
        assert canonical_field_key("合格率") == "合格率"
        assert canonical_field_key("抽样数量") == "抽样数量"

    def test_english_alias_maps_to_chinese(self) -> None:
        assert canonical_field_key("qualified_rate") == "合格率"
        assert canonical_field_key("sample_count") == "抽样数量"

    def test_unregistered_returns_none(self) -> None:
        assert canonical_field_key("unknown_field") is None
        assert canonical_field_key("") is None


class TestDiagnosticFieldRegistry:
    """验证 DIAGNOSTIC_FIELD_REGISTRY 基础行为。"""

    def test_registry_has_expected_entries(self) -> None:
        assert "合格率" in DIAGNOSTIC_FIELD_REGISTRY
        assert "抽样数量" in DIAGNOSTIC_FIELD_REGISTRY
        assert "qualified_rate" in DIAGNOSTIC_FIELD_REGISTRY
        assert "sample_count" in DIAGNOSTIC_FIELD_REGISTRY

    def test_registry_miss_graceful_skip(self) -> None:
        """未注册字段返回 None，不抛异常。"""
        assert DIAGNOSTIC_FIELD_REGISTRY.get("不存在的字段") is None
        assert canonical_field_key("不存在的字段") is None

    def test_spec_fields_populated(self) -> None:
        spec = DIAGNOSTIC_FIELD_REGISTRY["合格率"]
        assert spec.metric_template == "合格率"
        assert spec.sql_expression != ""
        assert spec.alias == "actual_合格率"
        assert spec.canonical_key == "合格率"


class TestDiagnosticSelectRegistryDriven:
    """验证 _diagnostic_select_for_condition 为注册表驱动。"""

    @pytest.mark.asyncio
    async def test_registry_hit_generates_correct_sql(
        self, agent: DataSQLAgent, base_context: AgentContext
    ) -> None:
        """注册表命中时生成包含正确投影的诊断 SELECT。"""
        agent._db.validate_sql = MagicMock(return_value=(True, ""))
        agent._db.execute_query = AsyncMock(
            return_value={
                "columns": ["product_spec_id", "actual_合格率", "actual_抽样数量"],
                "rows": [
                    {
                        "product_spec_id": "spec_A",
                        "actual_合格率": 85.0,
                        "actual_抽样数量": 120,
                    }
                ],
                "row_count": 1,
                "truncated": False,
            }
        )

        # 预先注册 condition 推理步骤，否则 update_condition_step 找不到匹配
        agent._emitter._reasoning_steps = [
            ReasoningStep(
                kind=ReasoningStepKind.CONDITION,
                label="合格率 >= 75",
                field="qualified_rate",
                expected=">= 75",
            ),
            ReasoningStep(
                kind=ReasoningStepKind.CONDITION,
                label="抽样数量 >= 100",
                field="sample_count",
                expected=">= 100",
            ),
        ]

        ctx = base_context.model_copy(update={
            "metrics": [MetricDefinition(name="合格率", formula="")],
            "filters": [
                FilterCondition(field="qualified_rate", operator=">=", value=75, display_name="合格率"),
                FilterCondition(field="sample_count", operator=">=", value=100, display_name="抽样数量"),
            ],
        })

        result = await agent._diagnostic_select_for_condition(
            ctx,
            {
                "columns": ["product_spec_id"],
                "rows": [{"product_spec_id": "spec_A"}],
                "row_count": 1,
                "truncated": False,
            },
        )

        assert len(result) > 0
        # 验证 validate_sql 被调用且 SQL 包含注册表中的别名
        validate_sql_call_args = agent._db.validate_sql.call_args[0][0]
        assert "actual_合格率" in validate_sql_call_args
        assert "actual_抽样数量" in validate_sql_call_args

    @pytest.mark.asyncio
    async def test_registry_miss_skips_gracefully(
        self, agent: DataSQLAgent, base_context: AgentContext
    ) -> None:
        """注册表未命中时 graceful skip，不发查询。"""
        agent._db.validate_sql = MagicMock()
        agent._db.execute_query = AsyncMock()

        ctx = base_context.model_copy(update={
            "metrics": [MetricDefinition(name="合格率", formula="")],
            "filters": [
                FilterCondition(field="unknown_field", operator="=", value="x", display_name="未知字段"),
            ],
        })

        result = await agent._diagnostic_select_for_condition(
            ctx,
            {
                "columns": ["product_spec_id"],
                "rows": [{"product_spec_id": "spec_A"}],
                "row_count": 1,
                "truncated": False,
            },
        )

        assert result == []
        agent._db.validate_sql.assert_not_called()
        agent._db.execute_query.assert_not_awaited()

    @pytest.mark.asyncio
    async def test_new_metric_via_monkeypatch(
        self, agent: DataSQLAgent, base_context: AgentContext, monkeypatch: pytest.MonkeyPatch
    ) -> None:
        """通过 monkeypatch 注入新 spec，验证不改函数体即可工作。"""
        new_spec = DiagnosticSpec(
            metric_template="合格率",
            sql_expression="SUM(total_count)",
            alias="actual_总产量",
            canonical_key="总产量",
        )

        # 临时扩展注册表
        monkeypatch.setitem(DIAGNOSTIC_FIELD_REGISTRY, "总产量", new_spec)
        monkeypatch.setitem(DIAGNOSTIC_FIELD_REGISTRY, "total_yield", new_spec)

        agent._db.validate_sql = MagicMock(return_value=(True, ""))
        agent._db.execute_query = AsyncMock(
            return_value={
                "columns": ["product_spec_id", "actual_总产量"],
                "rows": [
                    {"product_spec_id": "spec_A", "actual_总产量": 999}
                ],
                "row_count": 1,
                "truncated": False,
            }
        )

        ctx = base_context.model_copy(update={
            "metrics": [MetricDefinition(name="合格率", formula="")],
            "filters": [
                FilterCondition(field="total_yield", operator=">=", value=100, display_name="总产量"),
            ],
        })

        await agent._diagnostic_select_for_condition(
            ctx,
            {
                "columns": ["product_spec_id"],
                "rows": [{"product_spec_id": "spec_A"}],
                "row_count": 1,
                "truncated": False,
            },
        )

        # 验证 validate_sql 被调用且包含新别名
        validate_sql_call_args = agent._db.validate_sql.call_args[0][0]
        assert "actual_总产量" in validate_sql_call_args
        # 清理 monkeypatch（pytest 自动处理）
