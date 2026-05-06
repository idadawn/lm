<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# unit

## Purpose
模块级单元测试。一个测试文件对应一个被测模块，全部依赖 mock，运行毫秒级。覆盖率门槛由 `pyproject.toml` `fail_under = 70` 强制（omit `tests/*` 与 `app/main.py`）。

## Key Files

| File | Description |
|------|-------------|
| `test_sql_tools.py` | SQL 白名单 / 黑名单 / 危险序列 / 表白名单 / `validate_column_name` / `validate_time_range_sql` |
| `test_query_tools.py` | `get_formula_definition_tool` / `query_metric_tool` / `get_grade_rules_tool` / 一次交检合格率（含按重量加权回落 lab_raw_data） |
| `test_graph_tools.py` | `traverse_judgment_path` 多跳：record/spec/rule/condition/grade 全路径 + fallback（无炉号 / 记录不存在 / KG 不可用 / Cypher 异常 / 规则缺失 / 未结构化条件） |
| `test_knowledge_graph.py` | `queries.py` 高层 Cypher 函数（mock `graph.query_async`） |
| `test_graph.py` | `intent_classifier_node` / `route_by_intent` / `response_formatter_node` 的字段拼接与 fallback |
| `test_query_agent.py` | `query_agent_node` 各分支：判定规则查询 / 产品规格 / 一次交检 / 元问答（无 metric） / 普通指标 + 图表选择 |
| `test_root_cause_agent.py` | `root_cause_agent_node` 抽实体 / 调 tool / dispatch reasoning_step / state 字段更新 |
| `test_chat_api.py` | `/chat` 非流式 + 多轮 session context |
| `test_chat_sse.py` | `/chat/stream` 各类 SSE 事件序列（text / tool_start / tool_end / chart / reasoning_step / response_metadata / done / error 中文化） |
| `test_chat_nonstream.py` | `/chat` 端点直接 `graph.ainvoke` + 完整响应字段 |

## For AI Agents

### Working In This Directory
- 一个 `app/<x>.py` ↔ 一个 `tests/unit/test_<x>.py`，名字保持对齐方便定位。
- mock 顺序：`AsyncMock(side_effect=[r1, r2, ...])` 表示连续多次调用返回不同结果（对 LLM 多轮调用很有用）。
- 不要在 unit 里跑真实 graph compile；那归 `tests/agent/`。这里直接 `await query_agent_node({...})` 即可。
- 断言要看实际字段而非"是否调了 mock"：检查 `result["response"]` 含中文关键字、`result["chart_config"]["type"] == "line"` 这种业务断言。

### Testing Requirements
- 单文件：`uv run pytest tests/unit/test_<x>.py -v`。
- 单测试：`uv run pytest tests/unit/test_x.py::TestClass::test_method -v`。
- 覆盖率：`uv run pytest tests/unit/ --cov=app --cov-report=term-missing` — 看哪个分支没覆盖。

### Common Patterns
- `@pytest.mark.asyncio` 可省（`pyproject.toml` 配了 `asyncio_mode = "auto"`）。
- `with patch("app.api.chat.create_agent_graph", return_value=graph): chat_api._SESSION_CONTEXTS.clear()` — 每个测试前清空模块级缓存，避免互相污染。
- 用 `MagicMock(content="...")` 模拟 LangChain `AIMessage` / LLM 响应，注意 `.content` 不是 `.text`。

## Dependencies

### Internal
- 直接 `from app.<x> import ...` 导入被测模块。

### External
- `pytest`、`pytest-asyncio`、`unittest.mock.AsyncMock/MagicMock/patch`。
