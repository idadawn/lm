<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# services

## Purpose
后端进程容器。当前只有 `agent-api/` —— FastAPI + LangGraph + LiteLLM 实现的 NLQ 智能体 API，对外提供 SSE 流式 `/api/v1/chat/stream` + 非流式 `/api/v1/chat` + 知识图谱 REST `/api/v1/kg/*`。

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `agent-api/` | Python 3.11 FastAPI 应用（LangGraph 多 Agent 编排 + Neo4j KG + Chat2SQL）（见 `agent-api/AGENTS.md`） |

## For AI Agents

### Working In This Directory
- 后端使用 `uv` 管理依赖，禁止 `pip install` / 手改 `uv.lock`。新增依赖必须 `cd services/agent-api && uv add <pkg>`。
- 每个 service 必须独立 `pyproject.toml` + `Dockerfile.dev`，根 docker-compose 通过 `build.context` 引用。

### Testing Requirements
- `cd services/agent-api && uv run pytest`，覆盖率 ≥70%（`pyproject.toml` `fail_under`）。
- 单测分两层：`tests/unit/`（mock LLM/KG/SQL 的纯单元）与 `tests/agent/`（compiled LangGraph 端到端 mock 流）。

### Common Patterns
- LangGraph 节点 `async def node(state: dict) -> dict`，只返回要更新的字段；不要返回完整 state。
- 工具函数：`@tool async def name(...) -> dict`，docstring 必填且要描述工具用途+参数+返回结构（LLM 选工具靠它）。
- 流式推理链通过 `langchain_core.callbacks.adispatch_custom_event("reasoning_step", payload)` 发送，agent.api.chat 监听 `on_custom_event` 转 SSE。

## Dependencies

### Internal
- 通过 SQL 直查父项目（`lm/api`）落库的 `lab_*` 表（`lab_intermediate_data`、`lab_intermediate_data_formula`、`lab_intermediate_data_judgment_level`、`lab_product_spec*`、`lab_appearance_feature*`、`lab_report_config`、`lab_raw_data`）。
- 通过 KG 镜像同一组表的元数据（`Neo4jKnowledgeGraph.build` 全量重建）。

### External
- `fastapi`、`langgraph`、`langchain-openai`、`litellm`、`neo4j`、`sqlalchemy[asyncio]+aiomysql`、`pydantic-settings`、`httpx`、`tenacity`。
