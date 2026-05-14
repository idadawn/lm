<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# models

## Purpose
nlq-agent 数据契约层。包含 Pydantic v2 schemas（SSE 事件、ReasoningStep、AgentContext、ChatRequest 等）以及 LLM 暴露的数据库 DDL 与 SQL 模板常量。严格与前端 `mobile/types/reasoning-protocol.d.ts` / `web/src/api/nlqAgent.ts` 对齐。

## Key Files
| File | Description |
|------|-------------|
| `schemas.py` | `ReasoningStepKind`、`ReasoningStep`、`SSEEventType`、`SSEEvent`、`IntentType`、`AgentContext`、`FilterCondition`、`MetricDefinition`、`ChatRequest/Response`、`HealthResponse` |
| `ddl.py` | 暴露给 LLM 的核心表 DDL 字符串（`DDL_INTERMEDIATE_DATA`、`DDL_PRODUCT_SPEC`、`DDL_JUDGMENT_LEVEL` 等）+ `METRIC_SQL_TEMPLATES` 预定义模板 + `get_all_ddl()` |
| `__init__.py` | 子包标记 |

## For AI Agents

### Working in this directory
- `ReasoningStepKind` 枚举值（`record`/`spec`/`rule`/`condition`/`grade`/`fallback`）是跨语言协议常量，**改动需同步前端类型与 SKILL 文档**。
- 字段命名遵循 snake_case，序列化为 JSON 后给前端消费；不要切换 camelCase。
- DDL 字符串只暴露 LLM 需要的列，省略审计字段；每列必带中文 COMMENT，帮助 LLM 理解业务。
- `METRIC_SQL_TEMPLATES` 中的模板 key 与 `MetricDefinition.metric_id` 对应，新增模板需同时更新两处。
- 数值字段使用 `int | float | str | None`，避免 strict 模式因 LLM 返回字符串数值而验证失败。

### Common patterns
- triple-quote 原始字符串保存 DDL，便于直接注入 prompt。
- enum 值小写字符串，与 Python 默认 `str(Enum)` 行为一致。

## Dependencies
### Internal
- 消费方：`src/pipelines/*`、`src/services/sse_emitter.py`、`src/api/routes.py`
- 上游真值：`nlq-agent/packages/shared-types/`

### External
- `pydantic`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
