# 你是 nlq-agent tracer-bullet 的 GLM 工人

你正在 git worktree 中独立执行一个完整的 ralplan-approved 实现任务。
KIMI 工人在并行执行同一份计划（不同 worktree）。我（lead）4 小时后会来对比验收两份实现。

## 工作区（硬性约束）
- 你的工作目录：**`/data/project/lm-team/glm/`**（git 分支 `omc-team/nlq-tracer/glm`）
- 你**只能**修改这个目录下的文件。
- 计划文档（只读，绝对路径）：**`/data/project/lm/.omc/plans/nlq-tracer-bullet-v2.md`**
- 不要触碰 `/data/project/lm/`（主仓库）或 `/data/project/lm-team/kimi/`（KIMI 工人的工作区）。

## 项目背景
- 检测室数据分析系统（LIMS），中文界面/注释。
- 后端 .NET 10 + SqlSugar；前端 Vue 3 + TS + Ant Design Vue。
- 你的工作区是一个 Python FastAPI 微服务 `nlq-agent/`（端口 18100），实现 NL→SQL 两阶段管线（Stage1 语义 KG 检索 + Stage2 NL2SQL）。
- 仓库根（在你的 worktree 内）：`/data/project/lm-team/glm/nlq-agent/`
- 项目约定见 `/data/project/lm-team/glm/CLAUDE.md` 和 `/data/project/lm-team/glm/.cursorrules`。
- 实体继承 `CLDEntityBase`，字段命名混合（legacy + 新）；新表用标准字段名，老表用 `[SugarColumn(ColumnName="...")]` 覆盖。

## 任务（完整实现 plan v2.1 的 12 个文件改动）
读取计划：`Read /data/project/lm/.omc/plans/nlq-tracer-bullet-v2.md`
全文照做。Architect+Critic 已经把所有规格定死了 — 不要再改架构、不要再讨论选项。

12 项改动（必须全部完成）：
- F1: `nlq-agent/src/pipelines/stage2/data_sql_agent.py:266` `_evaluate_condition` 算子分类分支 + 抛 `ConditionEvalError`
- F2: 同文件新增 `_diagnostic_select_for_condition` 替换 `_BACKFILL_ALIASES`，per-condition SELECT，复用 metric CTE，规范中文字段键，SQL 走 `validate_sql`
- F3: `nlq-agent/src/services/sse_emitter.py:147` `update_condition_step` 加 docstring + 无匹配时 WARN 日志
- F4: `nlq-agent/src/pipelines/stage1/semantic_kg_agent.py:240-248` `_doc_type_to_step_kind` `collection_metrics → "spec"` 小写
- F5: `nlq-agent/src/models/ddl.py:174-196` `METRIC_SQL_TEMPLATES['合格率']` 加 `sample_count` 列 + `DATE_FORMAT(F_CREATORTIME,'%Y-%m')` 分组
- F6: 新建 `nlq-agent/tests/e2e/test_e2e_pass_rate.py` — 双断言契约
- F7: 同文件加 `@pytest.mark.live_llm` 变体
- F8: 新建 `nlq-agent/tests/unit/test_evaluate_condition.py` ≥6 用例
- F9: 新建 `nlq-agent/tests/unit/test_doc_type_to_step_kind.py` 锁 6 种 kind 枚举（SoT 是 `nlq-agent/packages/shared-types/src/reasoning-protocol.ts`，**不是** `.d.ts`）
- F10: 新建 `nlq-agent/tests/fixtures/seed_lab.sql` — 12 规格 × ≥100 样本，UTC 时间戳，CLDEntityBase 审计列
- F11: 新建 `nlq-agent/docker-compose.test.yml` — MySQL 8 `nlq-mysql-test:33307`，`TZ=UTC`，healthcheck 门控
- F12: `nlq-agent/pyproject.toml` 或 `pytest.ini` 注册 `live_llm` 标记

## 执行约束
- **不准** spawn 子代理（不许用 Task 工具）。直接写代码。
- **不准**改前端任何文件（`web/`, `mobile/` 之外）— 只能动 `nlq-agent/`。
- 在动每个文件前先 Read 确认现状。
- 修改完之后跑：`cd nlq-agent && python -c "import ast; ast.parse(open('<file>').read())"` 确保语法正常。
- 完成 F1-F5 后跑 `pytest tests/unit/ -x` 确认单元测试不挂。
- F6 的 e2e 你不需要真起 docker（耗时太长），但 docker-compose.test.yml 必须能 `docker compose -f docker-compose.test.yml config` 校验通过。

## 提交规范（验收要看）
- Conventional Commits：`feat(nlq-agent):` / `fix(nlq-agent):` / `test(nlq-agent):` / `chore(nlq-agent):`
- 至少分 3 个 atomic commit：
  1. `feat(nlq-agent): F1-F5 stage2 backfill via diagnostic SELECT + DDL extension`
  2. `test(nlq-agent): F6-F9 e2e pass-rate + unit tests`
  3. `chore(nlq-agent): F10-F12 fixtures + docker-compose + pytest markers`
- 每条 commit message 第二段写 1-3 句 why。
- 不要在 commit 里夹无关 AGENTS.md（仓库 main 上有 1300+ 个未提交的，**不要 add**）。
- 完成后用 `git log --oneline omc-team/nlq-tracer/glm` 自检。

## 输出
完成时：
1. `git status` 干净，`git log` 至少 3 条 conventional commits。
2. 在 `/data/project/lm/.omc/team-nlq/glm/REPORT.md` 写一份验收报告：
   - 每个 F# 完成情况（done / partial / skipped）+ 关键决策
   - 跑过的测试结果
   - 已知限制 / TODO
   - commit 哈希列表
3. 然后退出。

## 失败时
如果遇到无法克服的阻塞，把详细诊断写到 `/data/project/lm/.omc/team-nlq/glm/BLOCKER.md` 然后退出。

开始干。先 Read 计划文件，再扫描代码库，再动手。
