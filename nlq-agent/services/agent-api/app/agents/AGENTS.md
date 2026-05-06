<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# agents

## Purpose
LangGraph 节点实现。`graph.py` 定义状态机骨架（intent_classifier → 条件路由 → query/root_cause/chat2sql → response_formatter → END），三个领域 Agent 各一个文件。所有节点都是 `async def node(state: dict) -> dict`，仅返回要更新的字段。

## Key Files

| File | Description |
|------|-------------|
| `graph.py` | `create_agent_graph()` 编译入口 + `intent_classifier_node`（LLM 单条 user prompt 抽 intent + entities + time_range JSON） + `route_by_intent` 条件函数 + `response_formatter_node`（聚合返回字段，包括 `chart_config` / `calculation_explanation` / `grade_judgment` / `reasoning_steps`） |
| `query_agent.py` | 模板化指标查询：识别一次交检合格率/判定规则/产品规格/普通指标查询四种模式；时间范围解析（today/yesterday/this_week/last_month/year_month/recent_days/...）；图表类型选择（line/bar/pie）；多轮上下文 merge |
| `root_cause_agent.py` | 单条记录 KG 推理：抽炉号/批次号/等级 → 调 `traverse_judgment_path` @tool → 每步 `adispatch_custom_event("reasoning_step", step)` 流式 + state 镜像 → markdown 摘要回包 |
| `chat2sql_agent.py` | 兜底 Chat2SQL 6 步链：schema_pick → column_pick → sql_draft → sql_validate（白名单 + AST + 一次重试）→ execute_sql（一次 LLM 修正重试）→ result_summary；自动建图（line/bar，最多 3 列）；中间 LLM stream 不漏到 SSE text |
| `__init__.py` | 包标记 |

## For AI Agents

### Working In This Directory
- 节点签名固定 `async def name(state: dict[str, Any]) -> dict[str, Any]`；返回字典只含要变更的字段（LangGraph reducer auto-merge）。
- intent 路由依据 `state["intent"]`：新 intent 加到 `route_by_intent` 的 dict + `INTENT_CLASSIFICATION_PROMPT` 列表 + 条件路由 mapping。
- `query`/`root_cause`/`chat2sql` 的字段集要齐：response、chart_config、entities、context、calculation_explanation、grade_judgment、reasoning_steps（缺哪个 formatter 直接 `state.get(...)` 默认 None/[]）。
- `chat2sql_agent` 的中间 LLM 调用必须在 `api/chat.py` 的 `INTERNAL_LLM_NODES` 集合里（`intent_classifier`、`chat2sql_agent`），否则 stream chunk 漏到聊天正文。

### Testing Requirements
- 单测：`tests/unit/test_graph.py`（路由）、`tests/unit/test_query_agent.py`、`tests/unit/test_root_cause_agent.py`、`tests/unit/test_graph_tools.py`。
- 端到端：`tests/agent/test_root_cause_flow.py`、`tests/agent/test_query_agent_regression.py`（compiled graph 跑全图，mock LLM/SQL/KG）。

### Common Patterns
- 时间范围二级映射：`_build_time_range_sql` 出 SQL 片段，`_compute_time_range_absolute` 出 Python `(date, date)` 用于 LLM narrative。两者必须语义一致（YEARWEEK mode 1 = 周一为起点）。
- 推理链发射：`from langchain_core.callbacks import adispatch_custom_event` → `await adispatch_custom_event("reasoning_step", step_dict)`，同时把整个列表写 `state["reasoning_steps"]`（双通道）。
- 元问答兜底：`query_agent_node` 在没匹配到 metric 时把今天日期 + 可用指标列表注入 system prompt，让 LLM 自由回答（"今天几号"等）。

## Dependencies

### Internal
- `app.tools.query_tools`（指标查询工具集）、`app.tools.graph_tools`（KG 多跳 @tool）、`app.tools.sql_tools`（chat2sql 校验 + 执行）。
- `app.knowledge_graph.schema_loader`（chat2sql 取 information_schema 缓存）。
- `app.core.llm_factory.get_llm`（统一 LLM 入口）。

### External
- `langgraph`、`langchain-core`、`langchain-openai`。
