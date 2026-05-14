<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# nlq-sql-debug

## Purpose
OMC skill (level 3)：诊断与修复 NLQ Agent **Stage 2（Data & SQL Agent）** 中的 NL2SQL 问题。覆盖预定义模板未命中、字段名错配、安全白名单拦截、SQL 修正循环（最多 2 次重试）等典型故障路径，并指导优化 `SQL_GENERATION_PROMPT` 与 `SQL_CORRECTION_PROMPT`。

## Key Files
| File | Description |
|------|-------------|
| `SKILL.md` | Stage 2 流程图、核心数据表 DDL 摘要、常见错误分类、Prompt 调优 checklist |

## For AI Agents

### Working in this directory
- 修改 SKILL 时必须保持与 `src/models/ddl.py` 的列定义一致（特别是 `LAB_INTERMEDIATE_DATA` 的 `F_PERF_PS_LOSS` / `F_DETECTION_DATE` 等核心列名）。
- 新增预定义模板的指引必须强调：模板键名要与 `METRIC_SQL_TEMPLATES` 一致，并在 `_match_template()` 中注册。
- "SQL 修正循环"上限固定为 `MAX_SQL_RETRIES = 2`（见 `data_sql_agent.py`），文档不应建议无限重试。
- 安全检查白名单（仅 SELECT、`_FORBIDDEN_PATTERNS`）不能放宽；如确需新增写操作，请先在 `database.py` 评审。
- 调试用例的字段名一律使用真实 DDL 列（大写 + 下划线，例如 `F_PRODUCT_MODEL`），避免 LLM 学到错误样例。

### Common patterns
- SKILL 把"症状 → 定位文件 → 修复点"组织为三段式排错指南。
- 每个 Prompt 调整建议都附带"如何回归测试"。

## Dependencies
### Internal
- `nlq-agent/src/pipelines/stage2/data_sql_agent.py`、`src/models/ddl.py`、`src/utils/prompts.py`
- 执行层 `nlq-agent/src/services/database.py`

### External
- LLM、MySQL

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
