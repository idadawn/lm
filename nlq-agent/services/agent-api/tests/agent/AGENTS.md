<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# agent

## Purpose
Compiled LangGraph 端到端测试。仍 mock LLM/SQL/KG 三个外部依赖，但走真实的 `create_agent_graph()` → `intent_classifier_node → 路由 → <X>_agent_node → response_formatter_node` 完整 state 流，用来抓"节点字段拼接 / 路由条件分支 / state merge"层面的回归。

## Key Files

| File | Description |
|------|-------------|
| `test_root_cause_flow.py` | 根因路径全图：mock classifier 输出 `intent=root_cause` + entities → mock KG 返回 `_RECORD` + `_RULE` → 跑全图 → 断言 `result["response"]` 非空 + `result["reasoning_steps"]` ≥3 步 |
| `test_query_agent_regression.py` | 普通指标查询路径全图：覆盖 first_inspection_rate / metric trend / multi-turn context 等真实回归场景 |
| `__init__.py` | 包标记 |

## For AI Agents

### Working In This Directory
- 端到端测试必须 mock `app.agents.graph.get_llm` + `app.tools.sql_tools.execute_safe_sql` + `app.knowledge_graph.manager.get_knowledge_graph`，否则会真连 LiteLLM/MySQL/Neo4j。
- 不要把节点级 mock 写得太具体（如 mock `query_agent_node` 直接返回结果）— 那种属于 unit 测，应该放 `tests/unit/`。本目录测试要保留节点的真实执行路径。
- 引入新 intent 必须新增对应端到端 spec（建议命名 `test_<intent>_flow.py`），覆盖路由 + state 字段完整性。

### Testing Requirements
- `uv run pytest tests/agent/ -v` 单独运行；CI 总跑 `uv run pytest`（含 unit + agent）。
- 期望执行时间 <5s/test（全 mock，没有真 IO）。慢于此通常是 mock 没接好导致真请求外发。

### Common Patterns
- 用 `with patch("app.agents.graph.get_llm", return_value=classifier_llm), patch(...)` 嵌套多个 patch；或者用 `unittest.mock.patch.multiple`。
- 断言推理链顺序：`kinds = [s["kind"] for s in result["reasoning_steps"]]; assert kinds[0] == "record" and kinds[-1] in ("grade", "fallback")`。
- 多轮上下文测试：连发两条 `chat()` 请求复用 `session_id`，断言第二次 entities 沿用了第一次 context（已在 `tests/unit/test_chat_api.py` 验过 chat 层；端到端层加路由维度的）。

## Dependencies

### Internal
- 通过 `from app.agents.graph import create_agent_graph` 入口跑全图；间接覆盖 `agents/`、`tools/`、`knowledge_graph/` 三个子包的整合。

### External
- `pytest>=8`、`pytest-asyncio>=0.24`、`unittest.mock`。
