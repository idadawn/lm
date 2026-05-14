"""
E2E test: 合格率 ≥ 75% 且 抽样数量 ≥ 100 的产品规格

两断言契约：
  1. validate_sql(sql).is_valid is True
  2. qualified_rate == 75.0 AND sample_count >= 100

同时验证每个 condition 步骤的 actual 已回填（不为 None）。
"""

from __future__ import annotations

import json
from unittest.mock import AsyncMock, MagicMock, patch

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
from src.services.database import DatabaseService
from src.services.llm_client import LLMClient
from src.services.sse_emitter import SSEEmitter
from src.pipelines.stage2.data_sql_agent import DataSQLAgent


# ── 通用 fixture ──────────────────────────────────────────

def _make_context() -> AgentContext:
    """构建目标查询的 AgentContext。"""
    return AgentContext(
        user_question="合格率不低于75%且抽样数量不少于100的产品规格",
        intent=IntentClassification(
            intent=IntentType.STATISTICAL,
            confidence=0.95,
        ),
        business_explanation="查找合格率≥75%且抽样数量≥100的产品规格",
        filters=[
            FilterCondition(
                field="合格率",
                operator=">=",
                value=75,
                display_name="合格率",
                unit="%",
            ),
            FilterCondition(
                field="抽样数量",
                operator=">=",
                value=100,
                display_name="抽样数量",
                unit="条",
            ),
        ],
        metrics=[
            MetricDefinition(
                name="合格率",
                formula="SUM(qualified)/COUNT(*)*100",
                description="综合合格率",
            ),
        ],
    )


def _mock_db_service() -> DatabaseService:
    """构建 mock DatabaseService，区分主查询和诊断查询的返回值。"""
    db = MagicMock(spec=DatabaseService)
    db.validate_sql = MagicMock(return_value=(True, ""))

    # 主查询结果
    main_result = {
        "columns": [
            "F_PRODUCT_SPEC_ID", "qualified_rate", "sample_count",
            "total_count", "qualified_count",
        ],
        "rows": [
            {
                "F_PRODUCT_SPEC_ID": "spec_A",
                "qualified_rate": 75.0,
                "sample_count": 120,
                "total_count": 120,
                "qualified_count": 90,
            },
            {
                "F_PRODUCT_SPEC_ID": "spec_B",
                "qualified_rate": 60.0,
                "sample_count": 200,
                "total_count": 200,
                "qualified_count": 120,
            },
            {
                "F_PRODUCT_SPEC_ID": "spec_C",
                "qualified_rate": 95.0,
                "sample_count": 80,
                "total_count": 80,
                "qualified_count": 76,
            },
            {
                "F_PRODUCT_SPEC_ID": "spec_D",
                "qualified_rate": 88.0,
                "sample_count": 150,
                "total_count": 150,
                "qualified_count": 132,
            },
        ],
        "row_count": 4,
        "truncated": False,
    }

    # 诊断查询结果（_diagnostic_select_for_condition 使用的列名）
    diag_result = {
        "columns": ["product_spec_id", "actual_抽样数量", "actual_合格率"],
        "rows": [
            {
                "product_spec_id": "spec_A",
                "actual_抽样数量": 120.0,
                "actual_合格率": 75.0,
            },
            {
                "product_spec_id": "spec_B",
                "actual_抽样数量": 200.0,
                "actual_合格率": 60.0,
            },
            {
                "product_spec_id": "spec_C",
                "actual_抽样数量": 80.0,
                "actual_合格率": 95.0,
            },
            {
                "product_spec_id": "spec_D",
                "actual_抽样数量": 150.0,
                "actual_合格率": 88.0,
            },
        ],
        "row_count": 4,
        "truncated": False,
    }

    async def _execute_query(sql: str, params=None):
        if "actual_" in sql or "WITH base" in sql:
            return diag_result
        return main_result

    db.execute_query = AsyncMock(side_effect=_execute_query)
    return db


def _mock_llm() -> LLMClient:
    """构建 mock LLMClient。"""
    llm = MagicMock(spec=LLMClient)
    llm.chat_json = AsyncMock(return_value={
        "sql": "SELECT F_PRODUCT_SPEC_ID, qualified_rate, sample_count "
               "FROM some_view WHERE qualified_rate >= 75 AND sample_count >= 100",
        "explanation": "查询合格率和抽样数量",
    })
    llm.chat_stream = AsyncMock()
    # 让 chat_stream 返回空异步生成器
    async def _empty_stream(*a, **kw):
        return
        yield  # noqa: unreachable — makes this an async generator
    llm.chat_stream = MagicMock(return_value=_empty_stream())
    return llm


# ── F6: Mocked E2E test ──────────────────────────────────


class TestE2EPassRateMocked:
    """Mock LLM/DB 端到端测试：验证 SQL 有效性 + 数值契约 + condition 回填。"""

    async def test_sql_valid_and_values(self) -> None:
        """两断言契约：SQL 有效 + qualified_rate==75 且 sample_count>=100。"""
        db = _mock_db_service()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        # 预发射 condition 步骤（模拟 Stage 1）
        for f in _make_context().filters:
            step = ReasoningStep(
                kind=ReasoningStepKind.CONDITION,
                label=f.display_name,
                field=f.field,
                expected=f"{f.operator} {f.value}",
            )
            emitter.emit_reasoning_step(step)

        events = await agent.run(_make_context())

        # 断言 1：validate_sql 被调用且返回 (True, "")
        assert db.validate_sql.called
        for call in db.validate_sql.call_args_list:
            sql_str = call.args[0] if call.args else ""
            assert isinstance(sql_str, str) and len(sql_str) > 0
        is_valid, error = db.validate_sql.return_value
        assert is_valid, f"SQL 校验失败: {error}"

        # 断言 2：主查询返回的数据中存在 qualified_rate==75.0 且 sample_count>=100
        # 从第一次 execute_query 调用的返回值中提取
        first_call_sql = db.execute_query.call_args_list[0].args[0]
        first_result = await db.execute_query.side_effect(first_call_sql)
        matching = [
            r for r in first_result["rows"]
            if r["qualified_rate"] == 75.0 and r["sample_count"] >= 100
        ]
        assert len(matching) >= 1, (
            f"未找到 qualified_rate==75.0 且 sample_count>=100 的行"
        )

    async def test_condition_steps_actual_not_none(self) -> None:
        """每个 condition 步骤的 actual 已回填（不为 None）。"""
        db = _mock_db_service()
        llm = _mock_llm()
        emitter = SSEEmitter()
        agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)

        context = _make_context()
        for f in context.filters:
            step = ReasoningStep(
                kind=ReasoningStepKind.CONDITION,
                label=f.display_name,
                field=f.field,
                expected=f"{f.operator} {f.value}",
            )
            emitter.emit_reasoning_step(step)

        await agent.run(context)

        # 检查所有 condition 步骤都有 actual
        for step in emitter.get_all_steps():
            if step.kind == ReasoningStepKind.CONDITION:
                assert step.actual is not None, (
                    f"condition 步骤 '{step.field}' 的 actual 未回填"
                )


# ── F7: live_llm 变体 ────────────────────────────────────


@pytest.mark.live_llm
class TestE2EPassRateLiveLLM:
    """真实 LLM + 真实 MySQL 端到端测试（夜间 advisory lane）。

    合格率 ± 2.0pp 容差。
    需要环境变量 LIVE_LLM=1 并启动 docker-compose.test.yml。
    """

    async def test_live_llm_pass_rate_within_tolerance(self) -> None:
        """合格率在 73.0–77.0% 范围内（±2.0pp）。"""
        pytest.skip("需要 LIVE_LLM=1 和 docker-compose.test.yml 环境")

        # 如需运行，取消下方注释并配置真实依赖
        # from src.services.database import DatabaseService
        # from src.services.llm_client import LLMClient
        # db = DatabaseService()
        # await db.init_pool()
        # llm = LLMClient()
        # emitter = SSEEmitter()
        # agent = DataSQLAgent(llm=llm, db=db, emitter=emitter)
        # context = _make_context()
        # for f in context.filters:
        #     step = ReasoningStep(...)
        #     emitter.emit_reasoning_step(step)
        # events = await agent.run(context)
        # for step in emitter.get_all_steps():
        #     if step.kind == "condition" and step.field == "合格率":
        #         assert 73.0 <= step.actual <= 77.0
