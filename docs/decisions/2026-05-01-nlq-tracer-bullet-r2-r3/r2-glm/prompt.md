# 你是 nlq-agent round-2 的 GLM 工人 — 加 trend 查询类型

## 工作区
- worktree：**`/data/project/lm-team/glm/`**（分支 `omc-team/r2/glm-trend-query`，基于 main `3a4e602`）
- 你只能改这个目录下的文件。
- 主仓库 `/data/project/lm/` 与另一个 worktree `/data/project/lm-team/kimi/`（KIMI 工人）**禁止触碰**。

## 范围（trend 查询类型 — Stage1 路由 + Stage2 SQL 模板 + 测试）

当前 `nlq-agent` 只支持 statistical（聚合）类查询。本任务加 **trend** 类查询类型，用一个具体目标查询作为 tracer-bullet：

> **目标查询**："最近 6 个月每月的产品合格率变化趋势（按产品规格分组）"

### 实施清单

| # | 文件 | 改动 |
|---|---|---|
| T1 | `nlq-agent/src/models/schemas.py` | `IntentType` enum 加 `TREND` 值 |
| T2 | `nlq-agent/src/utils/prompts.py` | `STAGE1_INTENT_USER` 加 trend 关键词识别 few-shot（"趋势"/"变化"/"近 N 月"/"环比"/"同比"），`STAGE1_SEMANTIC_USER` 加 trend 模板的 filter 提取规则 |
| T3 | `nlq-agent/src/pipelines/stage1/semantic_kg_agent.py` | `_classify_intent` 路由：trend 关键词 → `IntentType.TREND`；`_parse_semantics` 解析 `time_window`（最近 N 月） |
| T4 | `nlq-agent/src/models/ddl.py` | `METRIC_SQL_TEMPLATES` 加 `'合格率_趋势'` 模板：按 `DATE_FORMAT(F_CREATORTIME, '%Y-%m')` + `F_PRODUCT_SPEC_ID` GROUP BY，输出 `month_bucket / product_spec_id / qualified_rate / sample_count`；按时间窗口 WHERE 过滤；`ORDER BY month_bucket ASC` |
| T5 | `nlq-agent/src/pipelines/stage2/data_sql_agent.py` | `_generate_sql` 识别 intent=TREND 时选用 `合格率_趋势` 模板；`_generate_summary_label` 渲染 trend 摘要（"近 N 月 X 规格合格率从 A% 升至 B%"） |
| T6 | `nlq-agent/tests/unit/test_trend_intent.py` (新) | 单元测试：trend 关键词 → IntentType.TREND；time_window=6 个月 → 正确生成 WHERE clause |
| T7 | `nlq-agent/tests/unit/test_trend_sql_template.py` (新) | 单元测试：`合格率_趋势` 模板 SQL 验证（结构正确 + 通过 `validate_sql.is_valid`） |
| T8 | `nlq-agent/tests/e2e/test_e2e_trend_query.py` (新) | E2E 测试（mocked LLM/Qdrant/DB）：trend 查询走通 SSE，事件序列含 ≥1 个 spec/condition/grade，response_metadata.sql 含 `DATE_FORMAT` 与 `ORDER BY month_bucket`。**不加** live_llm 变体（短任务略过） |

### 必须保持的兼容性
- 现有的 statistical 查询（合格率≥75% 那个）仍然要走绿。**不准动**已合并的 e2e `tests/e2e/test_e2e_pass_rate.py` — 它测的是 statistical 路径，必须仍 passed。
- `IntentType` 现有值（statistical / 等）不准重命名
- `METRIC_SQL_TEMPLATES['合格率']` 不准动 — KIMI 在并行改这个文件相邻区域，避免冲突

## 项目背景
- LIMS Python FastAPI 微服务，已合并 tracer-bullet v2.1（statistical 路径）
- 必读：`docs/decisions/2026-04-30-nlq-tracer-bullet-v2/PLAN.md`（架构原则）+ `docs/decisions/2026-04-30-nlq-tracer-bullet-v2/glm/REPORT.md`（你之前的实现风格）
- 项目约定：`/data/project/lm-team/glm/CLAUDE.md` 与 `.cursorrules`
- 实体审计列规则：`F_CREATORTIME` / `F_CREATORUSERID` / `F_ENABLEDMARK` / `F_TENANTID` 必须考虑
- SSE 协议不准动（trend 查询复用现有的 reasoning_step / text / response_metadata / done 序列）

## 执行约束
- **不准** spawn 子代理（不许 Task 工具）
- **不准**改 `web/`、`mobile/`、`api/`（.NET）— 只动 `nlq-agent/`
- **不准**动 `_diagnostic_select_for_condition`（KIMI 在并行改这个函数）
- 跑外部服务（docker / MySQL / Qdrant / LLM）— **不准**，只跑 unit test
- 现有 statistical e2e (`test_e2e_pass_rate.py`) 跑出 fail 则视为破坏兼容性 → 立即写 BLOCKER

## 提交规范
≥3 atomic commits（按 stage1/stage2/tests 切分），Conventional Commits：
- `feat(nlq-agent): T1-T3 stage1 trend intent classification`
- `feat(nlq-agent): T4-T5 stage2 合格率_趋势 SQL template + summary`
- `test(nlq-agent): T6-T8 trend query unit + e2e tests`

每条 commit message 第二段写 1-3 句 why。包含 `Co-Authored-By: Claude Opus 4.7` 行（与 round-1 风格一致）。`git status` 完成时干净。

## 输出
完成时：
1. `git log --oneline 3a4e602..HEAD` 显示 ≥3 commit
2. `pytest tests/ -m "not live_llm"` 全绿（含原有 statistical e2e）
3. 在 `/data/project/lm/.omc/team-nlq-r2/glm/REPORT.md` 写：
   - T1–T8 完成表格
   - 测试结果
   - 关键决策（trend 是单独 IntentType 还是 statistical 子类，为什么）
   - 已知限制（如：trend 还没接 frontend 渲染）

阻塞写到 `/data/project/lm/.omc/team-nlq-r2/glm/BLOCKER.md` 然后退出。

## 1h 截止
你必须在 60 分钟内退出。优先 T1-T5 + T8（端到端最小贯通）；T6/T7 不做不算失败。

开始。先 Read PLAN.md + 你之前的 REPORT.md，再扫描 schemas.py + prompts.py + ddl.py，然后再动手。
