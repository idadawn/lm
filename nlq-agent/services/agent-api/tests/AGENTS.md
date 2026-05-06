<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# tests

## Purpose
pytest 测试根。两层：`unit/`（mock 依赖的纯单元）和 `agent/`（compiled LangGraph 端到端，仍 mock LLM/SQL/KG）。pyproject 配置 `asyncio_mode = "auto"`，所有 `async def test_*` 自动挂上 anyio。

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `unit/` | 纯单元：sql_tools、query_tools、graph_tools、knowledge_graph、graph、root_cause_agent、query_agent、chat_api、chat_sse、chat_nonstream（见 `unit/AGENTS.md`） |
| `agent/` | LangGraph 全图端到端：`test_root_cause_flow.py`（intent_classifier→root_cause_agent→formatter）、`test_query_agent_regression.py`（见 `agent/AGENTS.md`） |

## For AI Agents

### Working In This Directory
- 不要在测试里真连 MySQL/Neo4j/LiteLLM；统一 `unittest.mock.AsyncMock` + `patch`。需要真集成时去 `scripts/test_kg_api.py` 或 web 端 e2e。
- 新模块入 `app/<x>.py` → 必须同时加 `tests/unit/test_<x>.py`，否则覆盖率掉到 70 以下 CI 失败（`pyproject.toml` `fail_under = 70`）。
- 共享 fixture 放 `conftest.py`（按需新建），不要在多个 test 里复制 mock 构造。

### Testing Requirements
- 全量：`uv run pytest`。
- 覆盖率：`uv run pytest --cov=app --cov-report=term-missing` 默认输出 + xml。

### Common Patterns
- LangGraph 节点测试模式：构造 `state = {"messages": [...], "entities": {...}, ...}` → `await node(state)` → 断言返回字典字段。
- KG 推理链断言：检查 `result["reasoning_steps"]` 顺序（record → spec → rule → condition × N → grade）+ 每步 `kind/label/satisfied/expected/actual` 字段。

## Dependencies

### Internal
- 通过 `from app.xxx import ...` 直接导入被测模块。

### External
- `pytest>=8`、`pytest-asyncio>=0.24`、`pytest-cov>=5`、`httpx>=0.27`（用于 FastAPI TestClient）。
