<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# app

## Purpose
FastAPI 应用源码包（`hatchling` 打包目标）。`main.py` 是进程入口：注册三个 router（`chat` / `health` / `kg`），lifespan 钩子里初始化 KG（按 `NEO4J_ENABLED`）+ schema cache（best-effort），关闭时 dispose 引擎。

## Key Files

| File | Description |
|------|-------------|
| `main.py` | FastAPI 实例 + CORS + lifespan + 路由挂载（`/api/v1/chat`、`/api/v1/health`、`/api/v1/kg`） |
| `__init__.py` | 包标记（无导出） |

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `agents/` | LangGraph 节点：`graph.py`（图编排+intent_classifier+response_formatter）、`query_agent.py`、`root_cause_agent.py`、`chat2sql_agent.py`（见 `agents/AGENTS.md`） |
| `api/` | FastAPI 路由器：`chat.py`（SSE + 非流式）、`kg.py`（11 个 KG REST）、`health.py`（见 `api/AGENTS.md`） |
| `core/` | 横切关注点：`config.py`（pydantic-settings）、`database.py`（async engine）、`llm_factory.py`、`auth.py`（上游 token 校验）、`logger.py`（见 `core/AGENTS.md`） |
| `knowledge_graph/` | KG 后端：`base.py`（接口）、`neo4j_graph.py`（Cypher 实现）、`manager.py`（生命周期）、`queries.py`（高层查询）、`schema_loader.py`（Chat2SQL 用 information_schema 缓存）（见 `knowledge_graph/AGENTS.md`） |
| `models/` | Pydantic 请求/响应 schema（`ChatRequest`、`StreamEvent`、`ChatMessage` 等）（见 `models/AGENTS.md`） |
| `tools/` | LangGraph `@tool` 函数：`sql_tools.py`（白名单+参数化）、`query_tools.py`（指标 / 公式 / 等级规则 / 一次交检）、`graph_tools.py`（KG 多跳 traverse_judgment_path）（见 `tools/AGENTS.md`） |

## For AI Agents

### Working In This Directory
- 启动器为 `app.main:app`；新增 router 在 `main.py` 注册时统一加 `/api/v1` 前缀。
- 全局单例：`_knowledge_graph`（manager.py）、`_SESSION_CONTEXTS`（chat.py）、`_CACHE`（schema_loader.py）、`_auth_cache`（auth.py）。这些靠 lifespan + module-level dict 维护，没有持久化。多 worker 部署需要替换为 Redis（已规划，未实现）。
- `app/` 是 hatchling 唯一打包目录（`tool.hatch.build.targets.wheel.packages = ["app"]`），新增模块直接在 `app/` 下加；外部脚本进 `scripts/`。

### Testing Requirements
- 覆盖率 `pyproject.toml` 配 `omit = ["tests/*", "app/main.py"]`，`fail_under = 70`。新增子模块需补 `tests/unit/` 单测。

### Common Patterns
- 模块级日志用 `from app.core.logger import logger`（`name="nlq-agent"`），不要 `import logging` 自建。
- 配置靠 `from app.core.config import settings`，settings 是模块级单例，加新字段直接 `Settings` 类里加 + `.env`。

## Dependencies

### Internal
- 子模块之间依赖关系（手画）：`api → agents + core + models`、`agents → tools + core`、`tools → core (database/sql_tools) + knowledge_graph (graph_tools)`、`knowledge_graph → core (config/logger)`、`models → 无（叶子）`、`core → 无（叶子）`。
- `core/database.py` 的 engine 被 `tools/sql_tools.py` 与 `knowledge_graph/schema_loader.py` 共享。

### External
- 见 `pyproject.toml` 顶层依赖列表。
