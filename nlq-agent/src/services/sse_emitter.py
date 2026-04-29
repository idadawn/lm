"""
SSE 事件发射器

负责将 Agent 产生的推理步骤和文本回答格式化为标准 SSE 事件流。
严格遵循前端 nlqAgent.ts 的解析协议：

    data: {"type": "reasoning_step", "reasoning_step": {...}}
    data: {"type": "text", "content": "..."}
    data: {"type": "response_metadata", "response_payload": {...}}
    data: {"type": "error", "error": "..."}
    data: {"type": "done"}
"""

from __future__ import annotations

import json
import logging
from typing import Any, AsyncIterator

from src.models.schemas import ReasoningStep, SSEEvent, SSEEventType

logger = logging.getLogger(__name__)


class SSEEmitter:
    """
    SSE 事件发射器。

    使用方式：
        emitter = SSEEmitter()
        emitter.emit_reasoning_step(step)
        emitter.emit_text("部分回答...")
        emitter.emit_done()

        # 获取所有事件
        async for event_str in emitter.stream():
            yield event_str
    """

    def __init__(self) -> None:
        self._events: list[str] = []
        self._reasoning_steps: list[ReasoningStep] = []

    def _format_sse(self, data: dict[str, Any]) -> str:
        """格式化为 SSE 协议字符串。"""
        return f"data: {json.dumps(data, ensure_ascii=False)}\n\n"

    def emit_reasoning_step(self, step: ReasoningStep) -> str:
        """
        发射一个推理步骤事件。

        Args:
            step: 推理步骤对象

        Returns:
            格式化后的 SSE 字符串
        """
        self._reasoning_steps.append(step)
        event = SSEEvent(
            type=SSEEventType.REASONING_STEP,
            reasoning_step=step,
        )
        sse_str = self._format_sse(event.model_dump(exclude_none=True))
        self._events.append(sse_str)
        return sse_str

    def emit_text(self, content: str) -> str:
        """
        发射一个文本 chunk 事件。

        Args:
            content: 文本内容

        Returns:
            格式化后的 SSE 字符串
        """
        event = SSEEvent(type=SSEEventType.TEXT, content=content)
        sse_str = self._format_sse(event.model_dump(exclude_none=True))
        self._events.append(sse_str)
        return sse_str

    def emit_response_metadata(self, extra: dict[str, Any] | None = None) -> str:
        """
        发射 response_metadata 事件（包含完整推理链）。

        这是最终事件，前端会用 reasoning_steps 覆盖之前逐步收到的步骤。

        Args:
            extra: 额外的元数据

        Returns:
            格式化后的 SSE 字符串
        """
        payload: dict[str, Any] = {
            "reasoning_steps": [
                s.model_dump(exclude_none=True) for s in self._reasoning_steps
            ],
        }
        if extra:
            payload.update(extra)

        event = SSEEvent(
            type=SSEEventType.RESPONSE_METADATA,
            response_payload=payload,
        )
        sse_str = self._format_sse(event.model_dump(exclude_none=True))
        self._events.append(sse_str)
        return sse_str

    def emit_error(self, error_message: str) -> str:
        """发射错误事件。"""
        event = SSEEvent(type=SSEEventType.ERROR, error=error_message)
        sse_str = self._format_sse(event.model_dump(exclude_none=True))
        self._events.append(sse_str)
        return sse_str

    def emit_done(self) -> str:
        """发射完成事件。"""
        event = SSEEvent(type=SSEEventType.DONE)
        sse_str = self._format_sse(event.model_dump(exclude_none=True))
        self._events.append(sse_str)
        return sse_str

    def get_all_steps(self) -> list[ReasoningStep]:
        """获取已发射的所有推理步骤。"""
        return self._reasoning_steps.copy()

    def update_condition_step(
        self,
        field: str,
        actual: str | float | int,
        satisfied: bool,
    ) -> ReasoningStep | None:
        """
        回填 condition 步骤的 actual 和 satisfied 值。
        Stage 2 查询完数据后调用此方法。

        Args:
            field: 条件字段名
            actual: 实际查询值
            satisfied: 是否满足条件

        Returns:
            更新后的 ReasoningStep（如果找到匹配的 condition 步骤）
        """
        for step in self._reasoning_steps:
            if step.kind == "condition" and step.field == field:
                step.actual = actual
                step.satisfied = satisfied
                return step
        return None
