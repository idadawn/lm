<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# agent-api

## Purpose
FastAPI + LangGraph + LiteLLM 后端。把用户的中文问题路由到 Intent Classifier，再分发给四种 Agent 之一：

| Intent | 入口节点 | 行为 |
|--------|---------|------|
| `query` | `query_agent_node` | 标准指标 / 一次交检合格率 / 判定规则 / 产品规格 — 模板化工具调用 |
| `root_cause` | `root_cause_agent_node` | 单条记录 KG 多跳推理（炉号→规格→规则→条件→等级），流式 `reasoning_step` |
| `chat2sql` | `chat2sql_agent_node` | 兜底：LLM 自己挑表/挑列/写 SQL/校验/执行/总结 — 6 步 reasoning chain |
| `insight` / `hypothesis` | `response_formatter_node` | MVP 不支持，返回引导提示 |

完成后统一汇入 `response_formatter_node` 输出 SSE `text` / `chart` / `reasoning_step` / `response_metadata`。

## Key Files

| File | Description |
|------|-------------|
| `pyproject.toml` | uv / hatchling 项目，3.11+，含 ruff/mypy/pytest 配置；coverage `fail_under = 70` |
| `Dockerfile.dev` | python:3.11-slim + uv，`uvicorn --reload` |
| `uv.lock` | 锁文件（不要手改） |
| `coverage.xml` | 上次 pytest --cov 的结果 |

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `app/` | FastAPI 应用代码：`main.py` 入口 + `agents/` + `api/` + `core/` + `knowledge_graph/` + `models/` + `tools/`（见 `app/AGENTS.md`） |
| `tests/` | pytest 测试：`unit/`（mock LLM/KG/SQL）+ `agent/`（compiled graph 端到端）（见 `tests/AGENTS.md`） |
| `scripts/` | 运维 / 验证脚本（`test_kg_api.py`）（见 `scripts/AGENTS.md`） |

## For AI Agents

### Working In This Directory
- 包管理：只用 `uv add <pkg>` / `uv add --dev <pkg>`；禁止 `pip`、禁止手改 `uv.lock`。
- 启动：`set ASPNETCORE_ENVIRONMENT=` 不需要；用 `uv run uvicorn app.main:app --reload --port 8000`（docker-compose 里默认 8000，前端 `/api/chat` 默认指 18100，按部署改 `NEXT_PUBLIC_AGENT_API_URL`）。
- 任何 SQL 必须 `from app.tools.sql_tools import execute_safe_sql` 走白名单；禁止 raw `session.execute(text(...))` 拼接用户输入。
- 任何外部 LLM 调用走 `app.core.llm_factory.get_llm(model_name=None)`；不要直接构造 `ChatOpenAI(...)`。
- KG 默认禁用（`NEO4J_ENABLED=false`），`get_knowledge_graph()` 返回 `None` 时所有 KG 路径必须降级而非崩溃。

### Testing Requirements
- 全量：`uv run pytest --cov=app --cov-report=term-missing`，覆盖 ≥70%。
- 单文件：`uv run pytest tests/unit/test_query_agent.py -v`。
- Lint：`uv run ruff check . --fix && uv run ruff format .`；类型：`uv run mypy app/`。

### Common Patterns
- LangGraph 节点签名：`async def name(state: dict[str, Any]) -> dict[str, Any]`，返回值只含要 merge 的字段。
- `@tool` 函数：必填中英文 docstring，参数有类型注解，返回 `dict[str, Any]`。
- 推理链：`from langchain_core.callbacks import adispatch_custom_event` → `await adispatch_custom_event("reasoning_step", payload_dict)`，`api/chat.py` 监听 `on_custom_event` 转发为 SSE。
- 错误本地化：上层抛异常前用 `_humanize_error_zh(exc)` 转中文（参见 `api/chat.py`）。

## Dependencies

### Internal
- 通过 `mysql+aiomysql` 直查父项目落库的 `lab_*` 表（11 张白名单表，见 `app/tools/sql_tools.py:ALLOWED_TABLES`）。
- 通过 `app/knowledge_graph/neo4j_graph.py:Neo4jKnowledgeGraph.build` 全量从 MySQL 重建 Neo4j 镜像（`MATCH (n) DETACH DELETE n` → 重新插入）。
- 走 `POXIAO_API_BASE_URL/api/oauth/CurrentUser` 校验上游主系统 token（缓存 `AUTH_CACHE_TTL_SECONDS` 秒）。

### External
- `fastapi>=0.115`、`langgraph>=0.3`、`langchain[-openai,-community]>=0.3`、`litellm>=1.50`、`sqlalchemy[asyncio]+aiomysql`、`redis>=5`、`langgraph-checkpoint-redis`、`networkx>=3.4`、`neo4j>=5.27`、`numpy/scipy/pandas`、`httpx`、`pydantic-settings`、`tenacity`。
