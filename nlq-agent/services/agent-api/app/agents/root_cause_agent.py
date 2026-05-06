"""RootCauseAgent — 单条记录的判等级根因解释。

设计要点
--------

1. **State-first + Stream-second**: 节点把整条 ReasoningStep 序列写入 state 的
   ``reasoning_steps`` 字段（供非流式 ``/chat`` 端点二通道消费），同时通过
   ``adispatch_custom_event("reasoning_step", step)`` 让 ``chat.py`` 把同一份步骤
   流式转发为 SSE 事件，前端的 ``<Reasoning>`` 折叠块按到达顺序累积渲染。

2. **图谱遍历下沉到 ``app.tools.graph_tools``**: 本模块只负责中文 NL 实体抽取、
   工具调用、事件分发、响应文本拼装；KG / SQL 细节由 ``traverse_judgment_path``
   ``@tool`` 提供。

3. **降级而非崩溃**: 任何步骤失败（无炉号、记录不存在、KG 不可用、Cypher 异常、
   规则缺失）都由 graph_tools 编码成 ``kind="fallback"`` 步骤；本节点照样 dispatch
   + 写 state，并在 markdown response 里给出可读降级提示。
"""

from __future__ import annotations

import re
from typing import Any

from langchain_core.callbacks import adispatch_custom_event
from langchain_core.messages import HumanMessage

from app.tools.graph_tools import traverse_judgment_path

_IDENTIFIER_PATTERN = re.compile(r"([A-Za-z0-9一-鿿-]{4,})")
_GRADE_PATTERN = re.compile(r"([ABCabc])[级級]")


# --------------------------------------------------------------------------- #
# LangGraph node
# --------------------------------------------------------------------------- #


async def root_cause_agent_node(state: dict[str, Any]) -> dict[str, Any]:
    """Explain why a single furnace or batch record is judged as a grade."""
    entities = dict(state.get("entities", {}))
    context = dict(state.get("context", {}))
    user_question = _get_last_user_message(state.get("messages", []))

    furnace_no = str(
        entities.get("furnace_no")
        or context.get("furnace_no")
        or _extract_identifier(user_question, ("炉号",))
        or ""
    ).strip()
    batch_no = str(
        entities.get("batch_no")
        or context.get("batch_no")
        or _extract_identifier(user_question, ("批次", "批号"))
        or ""
    ).strip()
    target_grade = str(
        entities.get("grade")
        or context.get("grade")
        or _extract_grade(user_question)
        or ""
    ).upper()

    # Fast path: no identifier at all → return a single fallback step + hint.
    if not furnace_no and not batch_no:
        fallback_step = {
            "kind": "fallback",
            "label": "请提供要归因的炉号或批次号",
        }
        await adispatch_custom_event("reasoning_step", fallback_step)
        return {
            "response": (
                "请提供要归因的炉号或批次号，例如"
                "“为什么炉号 1丙20260110-1 是 C 级？”"
            ),
            "intent": "root_cause",
            "entities": entities,
            "context": context,
            "reasoning_steps": [fallback_step],
        }

    # Walk the multi-hop path via the @tool. Always returns a non-empty list.
    steps: list[dict[str, Any]] = await traverse_judgment_path.ainvoke(
        {
            "furnace_no": furnace_no,
            "batch_no": batch_no or None,
            "target_grade": target_grade,
        }
    )

    # Stream each step as a custom SSE event (state-first, stream-second).
    for step in steps:
        await adispatch_custom_event("reasoning_step", step)

    response_text = _build_root_cause_response(
        furnace_no=furnace_no,
        batch_no=batch_no,
        target_grade=target_grade,
        steps=steps,
    )

    enriched_context = dict(context)
    record_step = next((s for s in steps if s.get("kind") == "record"), None)
    if record_step and isinstance(record_step.get("meta"), dict):
        meta = record_step["meta"]
        if meta.get("furnace_no"):
            enriched_context["furnace_no"] = str(meta["furnace_no"])
        if meta.get("batch_no"):
            enriched_context["batch_no"] = str(meta["batch_no"])
    spec_step = next((s for s in steps if s.get("kind") == "spec"), None)
    if spec_step and isinstance(spec_step.get("meta"), dict):
        spec_code = spec_step["meta"].get("spec_code")
        if spec_code:
            enriched_context["spec_code"] = str(spec_code)
    grade_step = next((s for s in steps if s.get("kind") == "grade"), None)
    if grade_step and isinstance(grade_step.get("meta"), dict):
        grade = grade_step["meta"].get("grade")
        if grade:
            enriched_context["grade"] = str(grade)

    enriched_entities = {**entities}
    if furnace_no:
        enriched_entities["furnace_no"] = furnace_no
    if batch_no:
        enriched_entities["batch_no"] = batch_no
    if target_grade:
        enriched_entities["grade"] = target_grade
    if "spec_code" in enriched_context:
        enriched_entities.setdefault("spec_code", enriched_context["spec_code"])

    return {
        "response": response_text,
        "intent": "root_cause",
        "entities": enriched_entities,
        "context": enriched_context,
        "reasoning_steps": steps,
    }


# --------------------------------------------------------------------------- #
# Markdown response builder (state["response"] fallback text)
# --------------------------------------------------------------------------- #


def _build_root_cause_response(
    furnace_no: str,
    batch_no: str,
    target_grade: str,
    steps: list[dict[str, Any]],
) -> str:
    """Render markdown summary of the reasoning chain.

    The text mirrors what the SSE ``<Reasoning>`` block streams, but lives in
    ``state["response"]`` so non-streaming consumers and clients without the
    reasoning-step renderer still get a useful answer.
    """
    fallback_steps = [s for s in steps if s.get("kind") == "fallback"]
    if fallback_steps and not any(s.get("kind") == "grade" for s in steps):
        # Path could not complete — surface the first/most-actionable hint.
        first = fallback_steps[0]
        detail = first.get("detail")
        suffix = f"\n\n备注：{detail}" if detail else ""
        return f"{first.get('label', '判定根因不可用')}{suffix}"

    grade_step = next((s for s in steps if s.get("kind") == "grade"), None)
    record_step = next((s for s in steps if s.get("kind") == "record"), None)
    spec_step = next((s for s in steps if s.get("kind") == "spec"), None)
    rule_step = next((s for s in steps if s.get("kind") == "rule"), None)
    conditions = [s for s in steps if s.get("kind") == "condition"]

    record_meta = record_step.get("meta", {}) if record_step else {}
    resolved_furnace = record_meta.get("furnace_no") or furnace_no or "未知炉号"
    resolved_batch = record_meta.get("batch_no") or batch_no or "未知批次"
    spec_meta = spec_step.get("meta", {}) if spec_step else {}
    spec_code = spec_meta.get("spec_code") or "未知规格"
    grade_meta = grade_step.get("meta", {}) if grade_step else {}
    grade = grade_meta.get("grade") or target_grade or "未知"

    lines = [
        f"炉号 `{resolved_furnace}`（批次 `{resolved_batch}`，规格 `{spec_code}`）"
        f"被归因为 **{grade} 级**。",
    ]

    if rule_step:
        lines.append("")
        lines.append(f"采用规则：{rule_step.get('label', '')}")

    if conditions:
        lines.append("")
        lines.append("| 条件 | 结果 | 实际值 | 期望 |")
        lines.append("| --- | --- | --- | --- |")
        for cond in conditions:
            label = cond.get("label", "")
            sat = cond.get("satisfied")
            if sat is True:
                mark = "满足"
            elif sat is False:
                mark = "不满足"
            else:
                mark = "未结构化"
            actual = cond.get("actual", "-")
            expected = cond.get("expected", "-")
            lines.append(f"| {label} | {mark} | {actual} | {expected} |")

    if grade_step:
        lines.append("")
        lines.append("原因总结：")
        lines.append(grade_step.get("label", ""))

    return "\n".join(lines)


# --------------------------------------------------------------------------- #
# NL parsing helpers (kept; not graph-related).
# --------------------------------------------------------------------------- #


def _get_last_user_message(messages: list[Any]) -> str:
    """Return the last user message content."""
    for msg in reversed(messages):
        if isinstance(msg, HumanMessage):
            return str(msg.content)
        if isinstance(msg, dict) and (
            msg.get("type") == "human" or msg.get("role") == "user"
        ):
            return str(msg.get("content", ""))
    return ""


def _extract_identifier(question: str, keywords: tuple[str, ...]) -> str | None:
    """Extract furnace or batch identifier from natural language."""
    for keyword in keywords:
        match = re.search(
            rf"{keyword}\s*[:：]?\s*({_IDENTIFIER_PATTERN.pattern})", question
        )
        if match:
            return match.group(1)
    return None


def _extract_grade(question: str) -> str | None:
    """Extract grade mention like A/B/C from the question."""
    match = _GRADE_PATTERN.search(question)
    if not match:
        return None
    return match.group(1).upper()
