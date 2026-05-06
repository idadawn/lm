<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# models

## Purpose
Pydantic v2 请求/响应 schema。FastAPI router 用它做参数校验 + OpenAPI 生成；与 `packages/shared-types` 是镜像关系，字段命名和 union 必须双向一致。

## Key Files

| File | Description |
|------|-------------|
| `schemas.py` | `ChatMessage`（role/content）、`ChatRequest`（messages/session_id/model_name/auth_context）、`StreamEvent`（type union + 全部可选 payload 字段）、`MetricDefinition`、`GradeRule`、`QueryResult` |
| `__init__.py` | 包标记 |

## For AI Agents

### Working In This Directory
- 加新 SSE 事件：在 `StreamEvent.type` Literal union 加值 → 加对应可选字段 → 同步 `packages/shared-types/src/index.ts` 的 `StreamEventType` 与 `StreamEvent`。两边不同步会让前端 TS 编译过但运行时拿不到字段。
- 字段命名一律 `snake_case`（与后端 JSON 输出一致）；不要混 camelCase。
- 不要在这里写运行时逻辑：仅 `Field(...)` 默认值 + `description`。

### Testing Requirements
- 由调用方间接覆盖：`tests/unit/test_chat_api.py`、`tests/unit/test_chat_sse.py` 通过 `ChatRequest`/`StreamEvent` 构造测试输入。

### Common Patterns
- 可选字段统一 `default=None`（不是 `default_factory`），让 `model_dump(exclude_none=True)` 干净。
- `StreamEvent.type` 是 Literal union，新增 type 必须前后端 + 文档（`docs/TDD.md` SSE 协议章节）三处一起改。

## Dependencies

### Internal
- 无 — 叶子模块。

### External
- `pydantic>=2.0`。
