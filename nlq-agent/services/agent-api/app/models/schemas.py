"""Pydantic 请求/响应模型."""

from enum import Enum
from typing import Literal

from pydantic import BaseModel, Field


class ChatMessage(BaseModel):
    """聊天消息模型."""

    role: Literal["user", "assistant", "system"] = Field(..., description="消息角色")
    content: str = Field(..., description="消息内容")


class ChatRequest(BaseModel):
    """聊天请求模型."""

    messages: list[ChatMessage] = Field(..., description="消息列表")
    session_id: str | None = Field(default=None, description="会话ID")
    model_name: str | None = Field(default=None, description="模型名称")
    auth_context: dict | None = Field(default=None, description="鉴权上下文")


class StreamEvent(BaseModel):
    """SSE 流事件模型."""

    type: Literal[
        "text",
        "tool_start",
        "tool_end",
        "chart",
        "reasoning_step",
        "response_metadata",
        "error",
        "done",
    ] = Field(..., description="事件类型")
    content: str | None = Field(default=None, description="文本内容")
    tool_name: str | None = Field(default=None, description="工具名称")
    tool_input: dict | None = Field(default=None, description="工具输入")
    tool_output: dict | None = Field(default=None, description="工具输出")
    chart_spec: dict | None = Field(default=None, description="图表配置")
    reasoning_step: dict | None = Field(default=None, description="KG 推理链步骤")
    response_payload: dict | None = Field(default=None, description="最终响应载荷")
    error: str | None = Field(default=None, description="错误信息")


class MetricDefinition(BaseModel):
    """指标定义模型."""

    id: int = Field(..., description="指标ID")
    name: str = Field(..., description="指标名称")
    column_name: str = Field(..., description="数据库字段名")
    formula: str = Field(..., description="计算公式")
    unit: str = Field(..., description="单位")
    formula_type: str = Field(..., description="公式类型")
    description: str | None = Field(default=None, description="指标说明")


class GradeRule(BaseModel):
    """等级判定规则模型."""

    id: int = Field(..., description="规则ID")
    formula_id: int = Field(..., description="公式ID")
    name: str = Field(..., description="等级名称")
    priority: int = Field(..., description="优先级")
    quality_status: str = Field(..., description="质量状态")
    color: str = Field(..., description="颜色标识")
    is_default: bool = Field(..., description="是否默认等级")
    conditions: list[dict] = Field(default_factory=list, description="判定条件")


class QueryResult(BaseModel):
    """查询结果模型."""

    metric_name: str = Field(..., description="指标名称")
    column_name: str = Field(..., description="字段名")
    value: float | None = Field(default=None, description="查询值")
    unit: str = Field(..., description="单位")
    aggregation: str = Field(..., description="聚合方式")
    grade: str | None = Field(default=None, description="判定等级")
    formula_description: str | None = Field(default=None, description="公式说明")
    time_range_label: str = Field(..., description="时间范围描述")


# --------------------------------------------------------------------------- #
# Ontology / Knowledge Graph DTOs (Phase 2+ )
# --------------------------------------------------------------------------- #


class ReasoningStepKind(str, Enum):
    """推理步骤类型."""

    INTENT = "intent"
    ENTITY = "entity"
    PATH = "path"
    RECORD = "record"
    SPEC = "spec"
    RULE = "rule"
    CONDITION = "condition"
    METRIC = "metric"
    SQL = "sql"
    STAT = "stat"
    ANSWER = "answer"
    FALLBACK = "fallback"


class OntologyRef(BaseModel):
    """本体对象引用."""

    type: str = Field(..., description="对象类型")
    id: str = Field(..., description="对象ID")
    label: str = Field(..., description="显示标签")


class EdgeRef(BaseModel):
    """本体关系引用."""

    source: str = Field(..., description="源节点ID")
    target: str = Field(..., description="目标节点ID")
    relation: str = Field(..., description="关系类型")


class EvidenceItem(BaseModel):
    """证据项."""

    label: str = Field(..., description="证据标签")
    value: str | float | None = Field(default=None, description="证据值")
    unit: str | None = Field(default=None, description="单位")
    source: str | None = Field(default=None, description="数据来源")


class ReasoningStep(BaseModel):
    """统一推理步骤模型.

    供 RootCauseAgent、Chat2SQL Agent 及前端共同使用。
    """

    id: str = Field(..., description="步骤唯一ID")
    kind: str = Field(
        ..., description="步骤类型: intent/entity/path/record/spec/rule/condition/metric/sql/stat/answer/fallback"
    )
    title: str = Field(..., description="步骤标题")
    summary: str = Field(default="", description="步骤摘要")
    status: Literal["pending", "running", "success", "warning", "failed"] = Field(
        default="success", description="步骤状态"
    )
    ontology_refs: list[OntologyRef] = Field(
        default_factory=list, description="引用的本体对象"
    )
    edge_refs: list[EdgeRef] = Field(default_factory=list, description="引用的本体关系")
    evidence: list[EvidenceItem] = Field(default_factory=list, description="证据列表")
    confidence: float | None = Field(default=None, description="置信度 0-1")
    meta: dict | None = Field(default=None, description="扩展字段")
    # 兼容原有松散字段
    field: str | None = Field(default=None, description="条件字段")
    expected: str | None = Field(default=None, description="期望值")
    actual: str | float | None = Field(default=None, description="实际值")
    satisfied: bool | None = Field(default=None, description="是否满足")
    label: str | None = Field(default=None, description="兼容旧版标签")
    detail: str | None = Field(default=None, description="兼容旧版详情")


class OntologyNode(BaseModel):
    """图谱节点 DTO."""

    id: str = Field(..., description="节点全局唯一ID")
    type: str = Field(
        ..., description="对象类型: ProductSpec/JudgmentRule/Metric/RuleCondition/..."
    )
    label: str = Field(..., description="显示标签")
    subtitle: str | None = Field(default=None, description="副标题")
    status: Literal["ok", "warning", "error", "unknown"] | None = Field(
        default=None, description="节点状态"
    )
    metrics: dict[str, str | float] | None = Field(default=None, description="指标数据")
    badges: list[str] | None = Field(default=None, description="徽章标签")
    raw: dict | None = Field(default=None, description="原始数据")


class OntologyEdge(BaseModel):
    """图谱边 DTO."""

    id: str = Field(..., description="边唯一ID")
    source: str = Field(..., description="源节点ID")
    target: str = Field(..., description="目标节点ID")
    relation: str = Field(..., description="关系类型")
    label: str | None = Field(default=None, description="显示标签")
    status: Literal["active", "muted", "failed"] | None = Field(
        default=None, description="边状态"
    )


class OntologyCombo(BaseModel):
    """图谱聚合组 DTO."""

    id: str = Field(..., description="组ID")
    label: str = Field(..., description="显示标签")
    type: str = Field(..., description="组类型")
    collapsed: bool = Field(default=False, description="是否折叠")
    parent_id: str | None = Field(default=None, description="父组ID")


class OntologyGraphDTO(BaseModel):
    """图谱数据传输对象.

    供 /subgraph、/explain 等接口返回。
    """

    nodes: list[OntologyNode] = Field(default_factory=list, description="节点列表")
    edges: list[OntologyEdge] = Field(default_factory=list, description="边列表")
    combos: list[OntologyCombo] | None = Field(default=None, description="聚合组列表")
    highlights: dict[str, list[str]] | None = Field(
        default=None, description="高亮配置: {nodeIds:[], edgeIds:[]}"
    )


class ExplainRequest(BaseModel):
    """问答解释请求."""

    question: str = Field(..., description="用户问题")
    session_id: str | None = Field(default=None, description="会话ID")
    context: dict | None = Field(default=None, description="上下文")


class ExplainResponse(BaseModel):
    """问答解释响应（非流式）."""

    answer: str = Field(..., description="自然语言答案")
    answer_card: dict | None = Field(default=None, description="结构化摘要卡片")
    reasoning_steps: list[ReasoningStep] = Field(default_factory=list, description="推理链")
    subgraph: OntologyGraphDTO | None = Field(default=None, description="相关子图")
    evidence_table: list[dict] | None = Field(default=None, description="证据表")
    suggested_actions: list[dict] | None = Field(default=None, description="建议动作")


class ResolveRequest(BaseModel):
    """实体解析请求."""

    phrase: str = Field(..., description="自然语言短语")
    context: dict | None = Field(default=None, description="上下文")


class ResolvedEntity(BaseModel):
    """解析出的实体."""

    type: str = Field(..., description="实体类型")
    id: str = Field(..., description="实体ID")
    label: str = Field(..., description="显示标签")
    confidence: float = Field(..., description="置信度")
    meta: dict | None = Field(default=None, description="元数据")


class SubgraphRequest(BaseModel):
    """子图查询请求."""

    anchor_type: str = Field(..., description="锚点类型")
    anchor_id: str = Field(..., description="锚点ID")
    depth: int = Field(default=2, ge=1, le=4, description="查询深度")
    relation_filter: str | None = Field(default=None, description="关系过滤")
