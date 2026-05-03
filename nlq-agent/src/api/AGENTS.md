<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# api

## Purpose
FastAPI HTTP 接口层。定义对外端点（SSE 流式问答、健康检查、规则/规格变更回调）并通过 `Depends` 注入服务单例。是前端 `web/src/api/nlqAgent.ts` 与 `mobile/utils/sse-client.js` 的服务端契约。

## Key Files
| File | Description |
|------|-------------|
| `routes.py` | 路由定义：`POST /api/v1/chat/stream`（SSE）、`GET /health`、`POST /api/v1/sync/rules`、`POST /api/v1/sync/specs` |
| `dependencies.py` | 服务单例容器：`init_services` / `shutdown_services` + `get_orchestrator` / `get_services` 工厂 |
| `__init__.py` | 子包标记 |

## For AI Agents

### Working in this directory
- SSE 端点必须设置响应头 `Cache-Control: no-cache` 和 `Connection: keep-alive`，否则部分网关会缓冲输出。
- 任何新端点都通过 `Depends(get_orchestrator)` 或 `get_services` 获取依赖；禁止在路由内 `import` 后直接 new 服务。
- `init_services` 中初始化顺序固定为 `LLMClient → EmbeddingClient → QdrantService(embedding) → DatabaseService → ensure_collections + init_pool → PipelineOrchestrator`；不要打乱。
- `/api/v1/sync/*` 是 .NET 后端的回调钩子，请保留即使路美还未启用——它是 Qdrant 增量更新的入口。
- 错误响应必须以 SSE `error` 事件输出，不要中途抛 HTTPException 打断流。

### Common patterns
- 端点返回 `StreamingResponse(event_generator(), media_type="text/event-stream")`。
- 全局单例使用模块级 `_xxx: T | None = None`，由 lifespan 写入。

## Dependencies
### Internal
- `src/pipelines/orchestrator.py`
- `src/services/*`、`src/models/schemas.py`

### External
- `fastapi`、`starlette` (StreamingResponse)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
