"""
Pipeline 集成测试

测试 Stage 1 + Stage 2 的完整流程，使用 mock 替代外部依赖。
"""

from __future__ import annotations

import json
from unittest.mock import AsyncMock, MagicMock, patch

import pytest

from src.models.schemas import (
    AgentContext,
    ChatMessage,
    ChatRequest,
    FilterCondition,
    IntentClassification,
    IntentType,
    MetricDefinition,
    ReasoningStep,
    ReasoningStepKind,
)
from src.services.sse_emitter import SSEEmitter


# ═══════════════════════════════════════════════════════════════
# SSEEmitter 单元测试
# ═══════════════════════════════════════════════════════════════

class TestSSEEmitter:
    """测试 SSE 事件发射器。"""

    def test_emit_reasoning_step(self):
        emitter = SSEEmitter()
        step = ReasoningStep(
            kind=ReasoningStepKind.RULE,
            label="A类判定规则",
            detail="铁损 ≤ 0.80 W/kg",
        )
        event = emitter.emit_reasoning_step(step)

        assert event.startswith("data: ")
        assert event.endswith("\n\n")
        data = json.loads(event[6:].strip())
        assert data["type"] == "reasoning_step"
        assert data["reasoning_step"]["kind"] == "rule"
        assert data["reasoning_step"]["label"] == "A类判定规则"

    def test_emit_text(self):
        emitter = SSEEmitter()
        event = emitter.emit_text("本月合格率为 95.2%")

        data = json.loads(event[6:].strip())
        assert data["type"] == "text"
        assert data["content"] == "本月合格率为 95.2%"

    def test_emit_response_metadata(self):
        emitter = SSEEmitter()
        step = ReasoningStep(kind=ReasoningStepKind.GRADE, label="合格率: 95.2%")
        emitter.emit_reasoning_step(step)

        event = emitter.emit_response_metadata({"sql": "SELECT ..."})
        data = json.loads(event[6:].strip())

        assert data["type"] == "response_metadata"
        assert "reasoning_steps" in data["response_payload"]
        assert data["response_payload"]["sql"] == "SELECT ..."

    def test_emit_done(self):
        emitter = SSEEmitter()
        event = emitter.emit_done()

        data = json.loads(event[6:].strip())
        assert data["type"] == "done"

    def test_update_condition_step(self):
        emitter = SSEEmitter()
        step = ReasoningStep(
            kind=ReasoningStepKind.CONDITION,
            label="铁损阈值",
            field="F_PERF_PS_LOSS",
            expected="<= 0.80",
        )
        emitter.emit_reasoning_step(step)

        updated = emitter.update_condition_step("F_PERF_PS_LOSS", 0.75, True)
        assert updated is not None
        assert updated.actual == 0.75
        assert updated.satisfied is True

    def test_update_condition_step_not_found(self):
        emitter = SSEEmitter()
        result = emitter.update_condition_step("F_NONEXISTENT", 0, False)
        assert result is None


# ═══════════════════════════════════════════════════════════════
# 数据模型测试
# ═══════════════════════════════════════════════════════════════

class TestSchemas:
    """测试 Pydantic 数据模型。"""

    def test_chat_request(self):
        req = ChatRequest(
            messages=[
                ChatMessage(role="user", content="本月合格率是多少？"),
            ],
        )
        assert len(req.messages) == 1
        assert req.messages[0].role == "user"

    def test_intent_classification(self):
        intent = IntentClassification(
            intent=IntentType.STATISTICAL,
            confidence=0.95,
            extracted_entities={"metric": "合格率", "time_range": "本月"},
        )
        assert intent.intent == IntentType.STATISTICAL
        assert intent.confidence == 0.95

    def test_filter_condition(self):
        fc = FilterCondition(
            field="F_PERF_PS_LOSS",
            operator="<=",
            value=0.80,
            display_name="Ps铁损",
            unit="W/kg",
        )
        assert fc.field == "F_PERF_PS_LOSS"
        assert fc.operator == "<="

    def test_agent_context(self):
        ctx = AgentContext(
            user_question="本月合格率是多少？",
            intent=IntentClassification(
                intent=IntentType.STATISTICAL,
                confidence=0.9,
            ),
            business_explanation="合格率 = 三项全合格数 / 总数 * 100",
            filters=[
                FilterCondition(
                    field="F_PROD_DATE",
                    operator="BETWEEN",
                    value="2026-04-01 AND 2026-04-30",
                    display_name="生产日期",
                ),
            ],
            metrics=[
                MetricDefinition(
                    name="合格率",
                    formula="COUNT(合格) / COUNT(*) * 100",
                    description="综合合格率",
                ),
            ],
        )
        assert ctx.intent.intent == IntentType.STATISTICAL
        assert len(ctx.filters) == 1
        assert len(ctx.metrics) == 1


# ═══════════════════════════════════════════════════════════════
# SQL 安全验证测试
# ═══════════════════════════════════════════════════════════════

class TestDatabaseSafety:
    """测试 SQL 安全验证逻辑。"""

    def test_valid_select(self):
        from src.services.database import DatabaseService
        db = DatabaseService()
        is_valid, _ = db.validate_sql("SELECT * FROM LAB_INTERMEDIATE_DATA LIMIT 10")
        assert is_valid

    def test_reject_insert(self):
        from src.services.database import DatabaseService
        db = DatabaseService()
        is_valid, error = db.validate_sql("INSERT INTO LAB_INTERMEDIATE_DATA VALUES (...)")
        assert not is_valid
        assert "INSERT" in error

    def test_reject_drop(self):
        from src.services.database import DatabaseService
        db = DatabaseService()
        is_valid, error = db.validate_sql("DROP TABLE LAB_INTERMEDIATE_DATA")
        assert not is_valid

    def test_reject_update(self):
        from src.services.database import DatabaseService
        db = DatabaseService()
        is_valid, error = db.validate_sql("UPDATE LAB_INTERMEDIATE_DATA SET F_NAME='x'")
        assert not is_valid

    def test_reject_non_select(self):
        from src.services.database import DatabaseService
        db = DatabaseService()
        is_valid, error = db.validate_sql("SHOW TABLES")
        assert not is_valid
        assert "仅允许 SELECT" in error
