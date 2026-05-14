"""聊天 API 模块.

提供 SSE 流式对话端点。
"""

import json
import time
import uuid
from collections.abc import AsyncGenerator
from typing import Any

from fastapi import APIRouter, Request
from fastapi.responses import StreamingResponse
from langchain_core.messages import HumanMessage

from app.agents.graph import create_agent_graph
from app.core.auth import validate_chat_auth
from app.models.schemas import ChatRequest, StreamEvent

from app.core.config import settings as _app_settings

router = APIRouter()
_SESSION_CONTEXTS: dict[str, dict[str, Any]] = {}

# 活跃工具调用跟踪：tool_call_id -> {start_time, name, input}
_ACTIVE_TOOL_CALLS: dict[str, dict[str, Any]] = {}


def _default_model_name() -> str:
    return _app_settings.DEFAULT_MODEL_NAME


@router.post("/chat/stream")
async def chat_stream(request: ChatRequest, http_request: Request) -> StreamingResponse:
    """流式对话端点.

    接收用户消息，通过 LangGraph Agent 处理，返回 SSE 流式响应。
    使用 astream_events 实现真正的流式输出。

    Args:
        request: 聊天请求

    Returns:
        SSE 流式响应
    """
    graph = create_agent_graph()
    auth_context = await validate_chat_auth(
        _build_auth_context(request, http_request),
    )

    # 生成 session_id（如果未提供）
    session_id = request.session_id or str(uuid.uuid4())
    session_context = _load_session_context(session_id)
    model_name = _resolve_model_name(session_id, request.model_name)

    # 每轮对话的 turn_id，用于把事件绑定到具体 turn
    turn_id = f"turn_{uuid.uuid4().hex[:12]}"

    async def event_generator() -> AsyncGenerator[str, None]:
        """生成 SSE 事件流."""
        # 转换消息格式
        messages = []
        for msg in request.messages:
            if msg.role == "user":
                messages.append(HumanMessage(content=msg.content))

        # 初始状态 - 使用字典形式
        initial_state = {
            "messages": messages,
            "session_id": session_id,
            "model_name": model_name,
            "auth_context": auth_context,
            "intent": "unknown",
            "entities": {},
            "context": session_context,
            "tool_results": {},
            "chart_config": None,
            "response": "",
        }

        # 流式执行图 - 使用 astream_events
        try:
            async for event in graph.astream_events(
                initial_state,
                version="v2",
                config={"configurable": {"thread_id": session_id}},
            ):
                event_type = event.get("event", "")
                event_name = event.get("name", "")
                event_data = event.get("data", {})

                # 处理 LLM 流式输出（打字机效果）
                # 仅转发"用户可见答复节点"的输出；以下节点的中间 LLM 调用产出
                # （JSON 分类、SQL JSON 草稿、列选 plan 等）都不能漏到聊天正文里。
                if event_type == "on_chat_model_stream":
                    metadata = event.get("metadata", {}) or {}
                    source_node = metadata.get("langgraph_node", "")
                    INTERNAL_LLM_NODES = {
                        "intent_classifier",
                        # chat2sql 内部 4-5 次 LLM call（schema_pick/column_pick/sql_draft/sql_fix）
                        # 都是中间产物，最终 narrative 通过 response_metadata 一次性下发。
                        "chat2sql_agent",
                    }
                    if source_node in INTERNAL_LLM_NODES:
                        continue
                    chunk = event_data.get("chunk")
                    if chunk and hasattr(chunk, "content") and chunk.content:
                        yield _format_event(
                            StreamEvent(
                                type="text",
                                content=chunk.content,
                            )
                        )

                # 处理工具开始调用
                elif event_type == "on_tool_start":
                    tool_input = event_data.get("input", {})
                    tool_call_id = f"tool_{uuid.uuid4().hex[:8]}"
                    tool_name = event_name
                    # 保存开始时间，用于后续计算 duration
                    _ACTIVE_TOOL_CALLS[tool_call_id] = {
                        "start_time": time.time(),
                        "name": tool_name,
                        "input": tool_input,
                    }
                    yield _format_event(
                        StreamEvent(
                            type="tool_start",
                            tool_name=tool_name,
                            tool_input={
                                "tool_call_id": tool_call_id,
                                "turn_id": turn_id,
                                "name": tool_name,
                                "input": tool_input
                                if isinstance(tool_input, dict)
                                else {"input": str(tool_input)},
                                "summary": _summarize_tool_input(tool_name, tool_input),
                            },
                        )
                    )

                # 处理工具结束调用
                elif event_type == "on_tool_end":
                    tool_output = event_data.get("output")
                    output_dict = {}
                    if isinstance(tool_output, dict):
                        output_dict = tool_output
                    elif hasattr(tool_output, "__dict__"):
                        output_dict = tool_output.__dict__
                    else:
                        output_dict = {"result": str(tool_output)}

                    # 尝试匹配最近开始的一个同名工具调用
                    matched_id = None
                    for tid, tinfo in list(_ACTIVE_TOOL_CALLS.items()):
                        if tinfo["name"] == event_name:
                            matched_id = tid
                            break

                    duration_ms = None
                    if matched_id:
                        start_time = _ACTIVE_TOOL_CALLS[matched_id].get("start_time")
                        if start_time:
                            duration_ms = int((time.time() - start_time) * 1000)
                        del _ACTIVE_TOOL_CALLS[matched_id]

                    # 如果没有匹配到，生成一个临时 id
                    tool_call_id = matched_id or f"tool_{uuid.uuid4().hex[:8]}"

                    yield _format_event(
                        StreamEvent(
                            type="tool_end",
                            tool_name=event_name,
                            tool_output={
                                "tool_call_id": tool_call_id,
                                "turn_id": turn_id,
                                "name": event_name,
                                "output": output_dict,
                                "duration_ms": duration_ms,
                                "summary": _summarize_tool_output(event_name, output_dict),
                                "status": "success",
                            },
                        )
                    )

                # 处理 LangGraph node 内通过 adispatch_custom_event 推送的推理链步骤
                elif event_type == "on_custom_event" and event_name == "reasoning_step":
                    payload = event_data if isinstance(event_data, dict) else {}
                    # 补充 turn_id 到 reasoning_step，便于前端绑定
                    payload["turn_id"] = turn_id
                    yield _format_event(
                        StreamEvent(
                            type="reasoning_step",
                            reasoning_step=payload,
                        )
                    )

                elif event_type == "on_chain_end" and event_name == "response_formatter":
                    output = event_data.get("output", {})
                    if isinstance(output, dict):
                        chart_config = output.get("chart_config")
                        _store_session_state(
                            session_id,
                            output.get("context"),
                            model_name,
                        )
                        if chart_config:
                            yield _format_event(
                                StreamEvent(
                                    type="chart",
                                    chart_spec=chart_config,
                                )
                            )

                        yield _format_event(
                            StreamEvent(
                                type="response_metadata",
                                response_payload={
                                    "session_id": session_id,
                                    "model_name": model_name,
                                    "response": output.get("response", ""),
                                    "chart_config": chart_config,
                                    "intent": output.get("intent"),
                                    "entities": output.get("entities"),
                                    "context": output.get("context"),
                                    "calculation_explanation": output.get(
                                        "calculation_explanation"
                                    ),
                                    "grade_judgment": output.get("grade_judgment"),
                                    "reasoning_steps": output.get(
                                        "reasoning_steps", []
                                    ),
                                    "turn_id": turn_id,
                                },
                            )
                        )

            # 发送结束标记
            yield _format_event(StreamEvent(type="done"))

        except Exception as e:
            # 发送错误事件 - 转成中文友好提示
            import traceback as _tb
            _tb.print_exc()
            yield _format_event(
                StreamEvent(
                    type="error",
                    error=_humanize_error_zh(e),
                )
            )
            yield _format_event(StreamEvent(type="done"))

    return StreamingResponse(
        event_generator(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "X-Accel-Buffering": "no",
            "Content-Type": "text/event-stream",
        },
    )


def _format_event(event: StreamEvent) -> str:
    """格式化 SSE 事件.

    Args:
        event: 事件对象

    Returns:
        SSE 格式的字符串
    """
    data = event.model_dump(exclude_none=True)

    if event.type == "done":
        return "data: [DONE]\n\n"

    return f"data: {json.dumps(data, ensure_ascii=False)}\n\n"


# --------------------------------------------------------------------------- #
# Tool call summary helpers
# --------------------------------------------------------------------------- #


def _summarize_tool_input(tool_name: str, tool_input: Any) -> str:
    """为 tool_start 生成一句话摘要."""
    if not isinstance(tool_input, dict):
        return f"正在调用 {tool_name}..."
    if tool_name == "traverse_judgment_path":
        furnace = tool_input.get("furnace_no", "")
        grade = tool_input.get("target_grade", "")
        return f"正在查询炉号 {furnace} 的 {grade} 级判定根因..."
    if tool_name in ("execute_safe_sql", "run_sql"):
        return "正在执行数据查询..."
    if tool_name == "query_knowledge_graph":
        return "正在查询知识图谱..."
    return f"正在调用 {tool_name}..."


def _summarize_tool_output(tool_name: str, tool_output: dict[str, Any]) -> str:
    """为 tool_end 生成一句话摘要."""
    if tool_name == "traverse_judgment_path":
        steps = tool_output.get("output", tool_output)
        if isinstance(steps, list):
            return f"完成根因分析，共 {len(steps)} 个步骤"
        return "根因分析完成"
    if tool_name in ("execute_safe_sql", "run_sql"):
        result = tool_output.get("output", tool_output)
        if isinstance(result, list):
            return f"查询完成，返回 {len(result)} 条记录"
        return "查询完成"
    return f"{tool_name} 执行完成"


@router.post("/chat")
async def chat(request: ChatRequest, http_request: Request) -> dict[str, Any]:
    """非流式对话端点（用于测试）.

    Args:
        request: 聊天请求

    Returns:
        完整响应
    """
    graph = create_agent_graph()
    auth_context = await validate_chat_auth(
        _build_auth_context(request, http_request),
    )

    # 生成 session_id（如果未提供）
    session_id = request.session_id or str(uuid.uuid4())
    session_context = _load_session_context(session_id)
    model_name = _resolve_model_name(session_id, request.model_name)

    # 转换消息格式
    messages = []
    for msg in request.messages:
        if msg.role == "user":
            messages.append(HumanMessage(content=msg.content))

    # 初始状态
    initial_state = {
        "messages": messages,
        "session_id": session_id,
        "model_name": model_name,
        "auth_context": auth_context,
        "intent": "unknown",
        "entities": {},
        "context": session_context,
        "tool_results": {},
        "chart_config": None,
        "response": "",
    }

    # 执行图
    result = await graph.ainvoke(initial_state)
    _store_session_state(session_id, result.get("context"), model_name)

    return {
        "session_id": session_id,
        "model_name": model_name,
        "response": result.get("response", ""),
        "chart_config": result.get("chart_config"),
        "intent": result.get("intent"),
        "entities": result.get("entities"),
        "context": result.get("context"),
        "calculation_explanation": result.get("calculation_explanation"),
        "grade_judgment": result.get("grade_judgment"),
        "reasoning_steps": result.get("reasoning_steps", []),
    }


def _load_session_context(session_id: str) -> dict[str, Any]:
    """Load previously stored context for a chat session."""
    session_state = _SESSION_CONTEXTS.get(session_id, {})
    if isinstance(session_state.get("context"), dict):
        return dict(session_state["context"])
    return dict(session_state)


def _resolve_model_name(session_id: str, request_model_name: str | None) -> str:
    """Resolve the active model for a request.

    优先级：请求体显式传入 > .env 的 DEFAULT_MODEL_NAME。
    不再回退到 _SESSION_CONTEXTS 里历史缓存的 model_name，避免上一次跑过
    gpt-4o 后续请求都被锁死在那个不存在的模型。
    """
    if request_model_name:
        return request_model_name
    return _default_model_name()


_ERROR_PATTERNS_ZH: list[tuple[str, str]] = [
    ("Model Not Exist", "当前模型在 LLM 网关中不存在，请检查后端 .env 的 DEFAULT_MODEL_NAME 与 LITELLM_BASE_URL 是否匹配。"),
    ("model_not_found", "当前模型在 LLM 网关中不存在，请检查后端 .env 的 DEFAULT_MODEL_NAME 与 LITELLM_BASE_URL 是否匹配。"),
    ("invalid_api_key", "LLM 接口的 API Key 无效，请检查 .env 中的 LITELLM_API_KEY。"),
    ("Incorrect API key", "LLM 接口的 API Key 无效，请检查 .env 中的 LITELLM_API_KEY。"),
    ("Connection refused", "无法连接 LLM 网关，请确认 LiteLLM/上游服务已启动。"),
    ("ConnectError", "无法连接 LLM 网关，请确认 LiteLLM/上游服务已启动。"),
    ("ReadTimeout", "LLM 响应超时，请稍后重试或更换模型。"),
    ("Rate limit", "LLM 调用被限流，请稍后再试。"),
    ("rate_limit_exceeded", "LLM 调用被限流，请稍后再试。"),
    ("Insufficient", "LLM 账户余额不足。"),
    ("insufficient", "LLM 账户余额不足。"),
    ("OperationalError", "数据库查询出错，请联系管理员。"),
    ("AttributeError", "服务内部异常（属性错误），请联系管理员。"),
    ("登录态无效", "登录态无效或已过期，请重新登录。"),
    ("登录态已过期", "登录态无效或已过期，请重新登录。"),
]


def _humanize_error_zh(exc: Exception) -> str:
    """把上游 LLM/HTTP/DB 抛出的英文异常转成中文友好提示。"""
    raw = str(exc)
    for needle, zh in _ERROR_PATTERNS_ZH:
        if needle in raw:
            return zh
    # 默认兜底：剥掉调用栈细节，给一句中文 + 极短英文摘要
    summary = raw.splitlines()[0][:120] if raw else "未知错误"
    return f"服务暂时无法处理本次请求（{summary}）。请稍后重试或联系管理员。"


def _build_auth_context(request: ChatRequest, http_request: Request) -> dict[str, Any]:
    """Merge explicit auth context with inbound proxy headers."""
    auth_context: dict[str, Any] = {}

    if isinstance(request.auth_context, dict):
        auth_context.update(request.auth_context)

    authorization = http_request.headers.get("authorization")
    if authorization and "access_token" not in auth_context:
        auth_context["access_token"] = authorization.removeprefix("Bearer ").strip()
        auth_context["token_type"] = "Bearer"

    header_mapping = {
        "x-user-id": "user_id",
        "x-user-account": "account",
        "x-tenant-id": "tenant_id",
        "x-request-origin": "origin",
    }

    for header_name, field_name in header_mapping.items():
        header_value = http_request.headers.get(header_name)
        if header_value and field_name not in auth_context:
            auth_context[field_name] = header_value

    permission_header = http_request.headers.get("x-user-permissions")
    if permission_header and "permissions" not in auth_context:
        auth_context["permissions"] = [
            item.strip() for item in permission_header.split(",") if item.strip()
        ]

    return auth_context


def _store_session_state(session_id: str, context: Any, model_name: str) -> None:
    """Persist both reusable context and the selected model for a chat session."""
    if not isinstance(context, dict):
        return

    _SESSION_CONTEXTS[session_id] = {
        "context": context,
        "model_name": model_name,
    }
