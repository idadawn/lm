"""Pydantic 请求/响应模型."""

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
