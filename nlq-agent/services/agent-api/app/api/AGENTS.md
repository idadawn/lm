<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# api

## Purpose
FastAPI router 定义。三个 router 由 `app/main.py` 挂到 `/api/v1` 前缀下：
- `chat` — 流式 `/chat/stream`（SSE）+ 非流式 `/chat`（同一 graph，方便测试 + 备用客户端）
- `kg` — 11 个 KG REST（health/init/refresh + specs/metrics/rules/appearance-features/report-configs/first-inspection/rules-search）
- `health` — `/health` 与 `/ready`

## Key Files

| File | Description |
|------|-------------|
| `chat.py` | `POST /chat/stream` 通过 `graph.astream_events(version="v2")` 转 SSE；过滤 `intent_classifier`/`chat2sql_agent` 的中间 LLM stream chunk；监听 `on_custom_event:reasoning_step`；最终 `on_chain_end:response_formatter` 出 `chart` + `response_metadata`；`POST /chat`（非流式）走 `graph.ainvoke`；session-context 内存 dict + `_humanize_error_zh` 中文错误映射 |
| `kg.py` | KG REST：`GET /health`/`POST /init`/`POST /refresh`、规格 CRUD-R 子集、指标公式查询、判定规则查询、外观特性三表、报表配置、规则搜索；KG 不可用时全部 503 |
| `health.py` | 简单 healthcheck（独立于 KG 状态） |
| `__init__.py` | 包标记 |

## For AI Agents

### Working In This Directory
- 所有路径函数必须 `async def`。同步 endpoint 会阻塞 uvicorn worker。
- session 上下文：`_SESSION_CONTEXTS: dict[str, dict]` 内存存储，进程重启即丢；多 worker 部署需挪到 Redis（已在 `pyproject.toml` 引入 `langgraph-checkpoint-redis` 但未接线）。
- 鉴权：`/chat/stream` 与 `/chat` 调用前先 `await validate_chat_auth(_build_auth_context(...))`；`AUTH_REQUIRED=False` 时允许匿名访问，仅 `/api/oauth/CurrentUser` 校验 token 是否过期。
- 错误本地化：上游 LiteLLM/DB 异常用 `_humanize_error_zh(exc)` 映射（含"Model Not Exist"/"Connection refused"/"OperationalError"等模式），不要直接把英文栈漏给前端。

### Testing Requirements
- `tests/unit/test_chat_api.py`（context 持久化）、`test_chat_sse.py`（SSE 事件序列）、`test_chat_nonstream.py`（非流式响应）。
- KG 路由没有专门单测，靠 `scripts/test_kg_api.py` 烟雾测和 web e2e 覆盖。

### Common Patterns
- SSE 帧格式：`data: {json}\n\n`，结束标记 `data: [DONE]\n\n`，由 `_format_event(StreamEvent)` 统一生成。
- `event_type == "on_chat_model_stream"` 的过滤集合 `INTERNAL_LLM_NODES = {"intent_classifier", "chat2sql_agent"}`，新增"内部 LLM 调用节点"（不是用户可见答复节点）必须加进来。
- KG 端点：`get_knowledge_graph()` 为 None 时一律 `raise HTTPException(503)`，不要返回空数组装作正常。

## Dependencies

### Internal
- `app.agents.graph.create_agent_graph`、`app.knowledge_graph.{manager,queries}`、`app.core.{auth,config}`、`app.models.schemas`。

### External
- `fastapi`、`langchain-core.messages`、`pydantic`。
