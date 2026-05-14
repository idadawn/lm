"""
Unit tests for F2 short-circuit in _backfill_conditions.

覆盖：
- 主行已含字段 → 短路，无诊断 query 发出
- 主行不含字段 → 走诊断 query，结果回填
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
from src.pipelines.stage2.data_sql_agent import DataSQLAgent
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


class TestBackfillShortCircuit:
    """验证 F2 短路逻辑。"""

    @pytest.mark.asyncio
    async def test_short_circuit_when_main_row_has_field(
        self, agent: DataSQLAgent, base_context: AgentContext
    ) -> None:
        """主查询行已含 canonical 字段时短路，不触发诊断 query。"""
        agent._db.validate_sql = MagicMock()
        agent._db.execute_query = AsyncMock()

        # 预先注册一个 condition 推理步骤
        step = ReasoningStep(
            kind=ReasoningStepKind.CONDITION,
            label="合格率 >= 75",
            field="qualified_rate",
            expected=">= 75",
        )
        agent._emitter._reasoning_steps = [step]

        ctx = base_context.model_copy(update={
            "metrics": [MetricDefinition(name="合格率", formula="")],
            "filters": [
                FilterCondition(field="qualified_rate", operator=">=", value=75, display_name="合格率"),
            ],
        })

        # 主查询结果行中已包含 "合格率"
        query_result = {
            "columns": ["product_spec_id", "合格率"],
            "rows": [{"product_spec_id": "spec_A", "合格率": 88.5}],
            "row_count": 1,
            "truncated": False,
        }

        result = await agent._backfill_conditions(ctx, query_result)

        # 应该有 SSE 事件被生成
        assert len(result) > 0
        # 诊断 query 不应被触发
        agent._db.validate_sql.assert_not_called()
        agent._db.execute_query.assert_not_awaited()
        # 步骤已被更新
        assert step.actual == 88.5
        assert step.satisfied is True

    @pytest.mark.asyncio
    async def test_diagnostic_query_when_main_row_lacks_field(
        self, agent: DataSQLAgent, base_context: AgentContext
    ) -> None:
        """主查询行不含字段时走诊断 query 并回填结果。"""
        agent._db.validate_sql = MagicMock(return_value=(True, ""))
        agent._db.execute_query = AsyncMock(
            return_value={
                "columns": ["product_spec_id", "actual_合格率"],
                "rows": [{"product_spec_id": "spec_A", "actual_合格率": 82.0}],
                "row_count": 1,
                "truncated": False,
            }
        )

        step = ReasoningStep(
            kind=ReasoningStepKind.CONDITION,
            label="合格率 >= 75",
            field="qualified_rate",
            expected=">= 75",
        )
        agent._emitter._reasoning_steps = [step]

        ctx = base_context.model_copy(update={
            "metrics": [MetricDefinition(name="合格率", formula="")],
            "filters": [
                FilterCondition(field="qualified_rate", operator=">=", value=75, display_name="合格率"),
            ],
        })

        # 主查询结果行中不包含 "合格率"
        query_result = {
            "columns": ["product_spec_id"],
            "rows": [{"product_spec_id": "spec_A"}],
            "row_count": 1,
            "truncated": False,
        }

        result = await agent._backfill_conditions(ctx, query_result)

        # 应该有 SSE 事件被生成
        assert len(result) > 0
        # 诊断 query 应该被触发
        agent._db.validate_sql.assert_called_once()
        agent._db.execute_query.assert_awaited_once()
        # 步骤已被更新
        assert step.actual == 82.0
        assert step.satisfied is True

    @pytest.mark.asyncio
    async def test_partial_short_circuit_mixed_fields(
        self, agent: DataSQLAgent, base_context: AgentContext
    ) -> None:
        """部分字段短路、部分字段走诊断 query。"""
        agent._db.validate_sql = MagicMock(return_value=(True, ""))
        agent._db.execute_query = AsyncMock(
            return_value={
                "columns": ["product_spec_id", "actual_抽样数量"],
                "rows": [{"product_spec_id": "spec_A", "actual_抽样数量": 150}],
                "row_count": 1,
                "truncated": False,
            }
        )

        step_qr = ReasoningStep(
            kind=ReasoningStepKind.CONDITION,
            label="合格率 >= 75",
            field="qualified_rate",
            expected=">= 75",
        )
        step_sc = ReasoningStep(
            kind=ReasoningStepKind.CONDITION,
            label="抽样数量 >= 100",
            field="sample_count",
            expected=">= 100",
        )
        agent._emitter._reasoning_steps = [step_qr, step_sc]

        ctx = base_context.model_copy(update={
            "metrics": [MetricDefinition(name="合格率", formula="")],
            "filters": [
                FilterCondition(field="qualified_rate", operator=">=", value=75, display_name="合格率"),
                FilterCondition(field="sample_count", operator=">=", value=100, display_name="抽样数量"),
            ],
        })

        # 主查询行只包含 "合格率"
        query_result = {
            "columns": ["product_spec_id", "合格率"],
            "rows": [{"product_spec_id": "spec_A", "合格率": 90.0}],
            "row_count": 1,
            "truncated": False,
        }

        result = await agent._backfill_conditions(ctx, query_result)

        assert len(result) > 0
        # 合格率已被短路
        assert step_qr.actual == 90.0
        assert step_qr.satisfied is True
        # 抽样数量通过诊断 query 回填
        assert step_sc.actual == 150
        assert step_sc.satisfied is True
        # 诊断 query 只应触发一次（为抽样数量）
        agent._db.validate_sql.assert_called_once()
        agent._db.execute_query.assert_awaited_once()
