<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# knowledge_graph

## Purpose
两类"图"放一起：(1) Neo4j 元数据知识图谱（产品规格 / 指标 / 判定规则 / 外观特性 / 报表配置）— 用于 RootCauseAgent 多跳推理 + `/api/v1/kg/*` REST；(2) Chat2SQL 用的 information_schema 内存缓存（`schema_loader.py`）— 与 Neo4j 故意分开，避免列级 schema 把 KG 变噪。

## Key Files

| File | Description |
|------|-------------|
| `base.py` | `BaseKnowledgeGraph(ABC)` — `build/refresh/get_node/get_neighbors/query/get_specs/get_metrics/get_judgment_rules` 抽象接口；为未来 networkx 后端预留 |
| `neo4j_graph.py` | `Neo4jKnowledgeGraph` —— 唯一的具体实现；`build()` 全量重建：specs → metrics → judgment_rules（关 spec/metric）→ spec_attributes（仅 IS_CURRENT 版本）→ appearance_features（三表）→ report_configs（关 metric）；只用异步 `query_async(cypher, **params)` |
| `manager.py` | 全局 `_knowledge_graph` 单例 + `init/close/refresh/get/is_ready/get_status`；`NEO4J_ENABLED=False` 时 init 直接返 None；任何失败都把单例置 None 让上游降级 |
| `queries.py` | 高层 Cypher 查询函数：`get_spec_judgment_rules` / `get_metric_formulas` / `get_related_metrics_by_spec` / `get_judgment_types_for_spec` / `get_first_inspection_config` / `get_spec_attributes` / `get_all_specs_with_attributes` / `find_rules_by_condition` / `get_appearance_feature{,s,_categories,_levels}` / `get_report_configs` / `get_report_config_by_formula` |
| `schema_loader.py` | Chat2SQL 用：`refresh_schema_cache()` 从 information_schema 拉 `lab_*` 所有表 + 列 + 注释 + COLUMN_KEY；按命名约定（`F_<ENT>_ID` → `lab_<ent>.F_Id`）推断 FK；同时拉 formula glossary（中文术语 → DB 列）；`SchemaCache.find_tables/relevant_columns` 服务于 LLM prompt 注入 |
| `__init__.py` | 重导出公开 API |

## For AI Agents

### Working In This Directory
- KG 构建是 destructive：`MATCH (n) DETACH DELETE n` 全清后重建。生产环境刷新前要确认无并发查询，否则前端短时间会拿空。
- `neo4j_graph.py` 的同步方法（`get_node`、`query`、`get_specs` 等）都 `raise NotImplementedError`，调用方必须用 `query_async`。
- KG 节点属性命名遵循 Cypher 习惯（camelCase：`columnName/qualityStatus/conditionJson/parentId`），与 SQL 列（`F_*` 大写）和 Pydantic（snake_case）三处都不一样 — 改字段时三边都要查。
- `schema_loader._infer_fk_target` 只识别 `F_<ENTITY>_ID` 形式；新表如果用别的约定（如 `F_RELATED_KEY`）需要手工补 FK，否则 chat2sql LLM 找不到 JOIN 路径。

### Testing Requirements
- `tests/unit/test_knowledge_graph.py` — Cypher 查询函数（mock graph.query_async）。
- 真集成靠 `scripts/test_kg_api.py`（拉真实 Neo4j 跑 6 个端点）。
- schema_loader 没专门单测，通过 `tests/unit/test_chat_api.py` 间接覆盖。

### Common Patterns
- 接口 `BaseKnowledgeGraph` 故意保留同步签名 — 未来 networkx 实现可填进去；当前 Neo4j 实现用 NotImplementedError 强迫调用方走 `query_async`。
- `manager._knowledge_graph` 是 module-level globals + `global` 关键字（不抽 class），简单清晰；多 worker 时每 worker 各持一份连接，OK。

## Dependencies

### Internal
- `app.core.config.settings`（NEO4J_*、数据库 URL）、`app.core.logger.logger`、`app.tools.query_tools.execute_safe_sql`（KG 构建期反查 MySQL）、`app.core.database.AsyncSessionLocal`（schema_loader）。

### External
- `neo4j>=5.27`、`sqlalchemy[asyncio]+aiomysql`、`networkx>=3.4`（仅作为依赖保留，未使用）。
