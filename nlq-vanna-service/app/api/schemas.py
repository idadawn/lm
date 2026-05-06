"""Pydantic schemas for the NLQ Vanna service API.

ReasoningStep mirrors the TypeScript interface in
nlq-agent/packages/shared-types/src/reasoning-protocol.ts.

StreamEvent / ChatRequest mirror index.ts (shared-types).

Do NOT redefine these in other modules — always import from here.
"""

from __future__ import annotations

from typing import Any, Literal

from pydantic import BaseModel

# ---------------------------------------------------------------------------
# ReasoningStep  (mirrors reasoning-protocol.ts)
# ---------------------------------------------------------------------------

ReasoningStepKind = Literal[
    "record",
    "spec",
    "rule",
    "condition",
    "grade",
    "fallback",
]


class ReasoningStep(BaseModel):
    """Single step in the KG reasoning chain.

    Mirrors ReasoningStep in reasoning-protocol.ts (shared-types).
    """

    kind: ReasoningStepKind
    label: str
    detail: str | None = None
    satisfied: bool | None = None
    field: str | None = None
    expected: str | None = None
    actual: str | int | float | None = None
    meta: dict[str, Any] | None = None


# ---------------------------------------------------------------------------
# ChatRequest / ChatMessage  (mirrors index.ts ChatRequest)
# ---------------------------------------------------------------------------


class ChatMessage(BaseModel):
    role: Literal["user", "assistant", "system"]
    content: str


class ChatRequest(BaseModel):
    messages: list[ChatMessage]
    session_id: str | None = None
    model_name: str | None = None
    auth_context: dict[str, Any] | None = None


# ---------------------------------------------------------------------------
# StreamEvent  (mirrors index.ts StreamEvent / StreamEventType)
# ---------------------------------------------------------------------------

StreamEventType = Literal[
    "text",
    "tool_start",
    "tool_end",
    "chart",
    "reasoning_step",
    "response_metadata",
    "error",
    "done",
]


class StreamEvent(BaseModel):
    """SSE payload envelope.

    Mirrors StreamEvent in index.ts (shared-types).
    Use model_dump_json(exclude_none=True) when serialising to SSE data field.
    """

    type: StreamEventType
    content: str | None = None
    tool_name: str | None = None
    tool_input: dict[str, Any] | None = None
    tool_output: dict[str, Any] | None = None
    reasoning_step: ReasoningStep | None = None
    response_payload: dict[str, Any] | None = None
    error: str | None = None
