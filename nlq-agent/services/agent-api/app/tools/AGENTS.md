<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# tools

## Purpose
LangGraph `@tool` 函数集合。三类：
- `sql_tools.py` — SQL 安全层（白名单 + 黑名单 + 危险序列检测 + 表白名单 + 参数化）
- `query_tools.py` — QueryAgent 用：指标元信息、聚合查询、等级规则、产品规格、一次交检合格率
- `graph_tools.py` — RootCauseAgent 用：`traverse_judgment_path` 多跳路径序列化为 ReasoningStep 列表

## Key Files

| File | Description |
|------|-------------|
| `sql_tools.py` | `validate_sql(sql)` 白名单（必须 SELECT 开头）+ 黑名单关键字（INSERT/UPDATE/DELETE/DROP/CREATE/ALTER/TRUNCATE/EXEC/UNION）+ 危险序列（`--` `/* */` `;\w+`）+ 表白名单（11 张 `lab_*` 表）；`execute_safe_sql(sql, params)` 通过 `AsyncSessionLocal` 跑参数化 SELECT；`build_safe_where_clause` / `validate_column_name` / `validate_time_range_sql` 工具函数 |
| `query_tools.py` | `@tool` 函数：`get_formula_definition_tool`（公式 + 单位 + 列名）、`query_metric_tool`（聚合 / 分组 by date / shift filter）、`get_grade_rules_tool`（按 formula_id 取等级规则 + 在 Python 端比较 condition_json）、`get_product_specs_tool`、`get_grade_rules_by_spec_tool`、`get_judgment_types_tool`、`get_first_inspection_config_tool`（读 `lab_report_config.F_LEVEL_NAMES` JSON）、`query_first_inspection_rate_tool`（按重量加权，回落 lab_raw_data） |
| `graph_tools.py` | `traverse_judgment_path(furnace_no, batch_no, target_grade) @tool` —— 走"炉号→规格→规则→条件×N→等级"序列；`_build_judgment_path_steps` 是无副作用核心；`_evaluate_rule_conditions` 把 `conditionJson`（LIST of dict）逐条 vs record 字段比较 → satisfied/failed/unstructured；中文化字段标签 `_FIELD_LABELS`；任何错误返回带 `kind="fallback"` step 的非空列表 |
| `__init__.py` | 包标记 |

## For AI Agents

### Working In This Directory
- **绝不**在工具内部 f-string 拼接用户输入到 SQL；必须 `:param` + `params=dict`。`graph_tools._get_record` 用 f-string 是因为 `where_clause` 由内部硬编码列谓词组成，不含用户输入（注释里有声明）。
- 新增 `@tool` 函数：(1) 装饰 `@tool`、(2) 参数全部带类型 + Args docstring、(3) 返回 `dict[str, Any]`、(4) 在 `app.agents.<x>_agent.py` 显式 import 并 `tool.ainvoke({...})` 调用 — LangGraph 不会自动发现。
- `query_metric_tool` 已经把列名校验内置（`validate_column_name` 拒绝非 `[a-zA-Z_][a-zA-Z0-9_]*$`），但仍要在调用方先把"中文/前端别名"映射到真实列名（如 `db_metric_mapping` 在 `query_agent.py`）。
- `_build_time_range_sql` 在 `query_agent.py`，不是这里 — 那里是"intent + entities → SQL"的业务转换，不归 tools 安全层。

### Testing Requirements
- `tests/unit/test_sql_tools.py`（白名单/黑名单/危险序列/表白名单）、`test_query_tools.py`、`test_graph_tools.py`（含 unstructured condition 的 fallback 路径）。
- 改 SQL 白名单或表名后必跑 `tests/unit/test_sql_tools.py`，并加新 case 覆盖新场景。

### Common Patterns
- `@tool` 函数 docstring 第一段是中文/英文工具用途，第二段是 Args，第三段是 Returns 字段说明 — LLM 选工具靠它，不能省。
- 等级条件比较：`_check_conditions(value, conditions)` / `_compare_condition(actual, condition)` 支持 `<= < >= > == !=` + `min/max` 双边；返回 False 不代表 KG 数据错，可能是 condition_json 里的字符串数组（unstructured），上层用 `unstructured` 列表展示。
- `get_first_inspection_config_tool` 找不到配置时降级到 `["A"]` 单等级合格，避免完全报空。

## Dependencies

### Internal
- `app.core.database.AsyncSessionLocal`（sql_tools 唯一 SQL 出口）。
- `app.knowledge_graph.manager.get_knowledge_graph`（graph_tools 调 KG）。

### External
- `langchain-core.tools.tool`、`sqlalchemy.text`。
