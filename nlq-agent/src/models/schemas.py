"""
nlq-agent 数据模型定义

包含 SSE 事件协议、Agent 上下文、请求/响应 DTO 等。
所有模型均使用 Pydantic v2，保证序列化/反序列化的类型安全。
"""

from __future__ import annotations

from enum import Enum
from typing import Any

from pydantic import BaseModel, Field


# ═══════════════════════════════════════════════════════════════
# 1. Reasoning Protocol（与前端 reasoning-protocol.d.ts 一一对应）
# ═══════════════════════════════════════════════════════════════

class ReasoningStepKind(str, Enum):
    """推理步骤类型枚举，必须与 TS 端 ReasoningStepKind 保持同步。"""
    RECORD = "record"
    SPEC = "spec"
    RULE = "rule"
    CONDITION = "condition"
    GRADE = "grade"
    FALLBACK = "fallback"


class ReasoningStep(BaseModel):
    """单个推理步骤，对应前端 KgReasoningChain 的一个节点。"""
    kind: ReasoningStepKind
    label: str
    detail: str | None = None
    satisfied: bool | None = None
    field: str | None = None
    expected: str | None = None
    actual: str | float | int | None = None
    meta: dict[str, Any] | None = None


# ═══════════════════════════════════════════════════════════════
# 2. SSE 事件类型
# ═══════════════════════════════════════════════════════════════

class SSEEventType(str, Enum):
    """SSE 事件类型枚举。"""
    TEXT = "text"
    REASONING_STEP = "reasoning_step"
    RESPONSE_METADATA = "response_metadata"
    ERROR = "error"
    DONE = "done"


class SSEEvent(BaseModel):
    """统一的 SSE 事件载荷。"""
    type: SSEEventType
    content: str | None = None
    reasoning_step: ReasoningStep | None = None
    response_payload: dict[str, Any] | None = None
    error: str | None = None


# ═══════════════════════════════════════════════════════════════
# 3. 意图分类
# ═══════════════════════════════════════════════════════════════

class IntentType(str, Enum):
    """用户意图分类。"""
    STATISTICAL = "statistical"      # 统计聚合类：合格率、产量、均值
    TREND = "trend"                  # 趋势分析类：按时间维度的指标变化
    ROOT_CAUSE = "root_cause"        # 根因分析类：为什么不合格、异常原因
    CONCEPTUAL = "conceptual"        # 概念解释类：A类是什么、铁损定义
    OUT_OF_SCOPE = "out_of_scope"    # 超出范围


class IntentClassification(BaseModel):
    """意图分类结果。"""
    intent: IntentType
    confidence: float = Field(ge=0.0, le=1.0)
    extracted_entities: dict[str, Any] = Field(default_factory=dict)
    reasoning: str = ""


# ═══════════════════════════════════════════════════════════════
# 4. Agent 上下文（Stage 1 → Stage 2 的桥梁）
# ═══════════════════════════════════════════════════════════════

class FilterCondition(BaseModel):
    """从知识图谱提取的结构化过滤条件。"""
    field: str                          # 数据库列名，如 F_PERF_PS_LOSS
    operator: str                       # 运算符：<=, >=, =, BETWEEN, IN
    value: str | float | list           # 阈值或值列表
    display_name: str = ""              # 中文展示名，如 "Ps铁损"
    unit: str = ""                      # 单位，如 "W/kg"


class MetricDefinition(BaseModel):
    """从语义层检索到的指标定义。"""
    name: str                           # 指标名称，如 "合格率"
    formula: str                        # 计算公式，如 "SUM(CASE WHEN ...)/COUNT(*)"
    sql_template: str = ""              # SQL 模板片段
    description: str = ""               # 业务说明


class AgentContext(BaseModel):
    """
    Stage 1 的输出 / Stage 2 的输入。
    这是两个阶段之间的核心数据桥梁。
    """
    # 用户原始问题
    user_question: str

    # 意图分类
    intent: IntentClassification

    # 业务解释（纯文本，来自知识图谱检索）
    business_explanation: str = ""

    # 结构化过滤条件（从规则/规格中提取）
    filters: list[FilterCondition] = Field(default_factory=list)

    # 指标定义（从语义层检索）
    metrics: list[MetricDefinition] = Field(default_factory=list)

    # 检索到的原始文档片段（用于 Stage 2 的 prompt 注入）
    retrieved_documents: list[dict[str, Any]] = Field(default_factory=list)

    # Stage 1 产生的推理步骤（已通过 SSE 发送给前端）
    reasoning_steps: list[ReasoningStep] = Field(default_factory=list)


# ═══════════════════════════════════════════════════════════════
# 5. 请求 / 响应 DTO
# ═══════════════════════════════════════════════════════════════

class ChatMessage(BaseModel):
    """单条对话消息。"""
    role: str = Field(pattern=r"^(user|assistant|system)$")
    content: str


class ChatRequest(BaseModel):
    """POST /api/v1/chat/stream 请求体。"""
    messages: list[ChatMessage]
    session_id: str | None = None
    model_name: str | None = None


class HealthResponse(BaseModel):
    """GET /health 响应。"""
    status: str = "ok"
    version: str = ""
    qdrant_connected: bool = False
    mysql_connected: bool = False
    llm_available: bool = False
