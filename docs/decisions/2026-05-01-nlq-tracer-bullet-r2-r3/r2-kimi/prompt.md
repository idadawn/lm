# 你是 nlq-agent round-2 的 KIMI 工人 — 诊断 SELECT 泛化 + F2 短路化

## 工作区
- worktree：**`/data/project/lm-team/kimi/`**（分支 `omc-team/r2/kimi-diag-select-generalize`，基于 main `3a4e602`）
- 你只能改这个目录下的文件。
- 主仓库 `/data/project/lm/` 与另一个 worktree `/data/project/lm-team/glm/`（GLM 工人）**禁止触碰**。

## 范围（两个改动 + 测试）

### 改动 #4 — 诊断 SELECT 泛化（metadata-driven 注册）
当前 `nlq-agent/src/pipelines/stage2/data_sql_agent.py::_diagnostic_select_for_condition` 用硬编码 if-else 识别 `合格率/抽样数量/qualified_rate/sample_count` 四个字段键。改为**注册表驱动**：

```python
# 期望成形（示意，实际你按风格调整）
DIAGNOSTIC_FIELD_REGISTRY: dict[str, DiagnosticSpec] = {
    "合格率":     DiagnosticSpec(metric_template="合格率", projection="qualified_rate"),
    "抽样数量":   DiagnosticSpec(metric_template="合格率", projection="sample_count"),
    "qualified_rate": DiagnosticSpec(metric_template="合格率", projection="qualified_rate"),
    "sample_count":   DiagnosticSpec(metric_template="合格率", projection="sample_count"),
}
```

要求：
- 新加 metric template 时只需要往 registry 加一行，不改 `_diagnostic_select_for_condition` 函数体
- `DiagnosticSpec` 是 dataclass / NamedTuple / Pydantic 你选一个，但要类型化
- 注册逻辑保留对未注册字段的 graceful skip（与现状一致）
- 与 plan v2.1 的 canonical Chinese key + validate_sql 通过同样一致

### 改动 #7 — F2 短路化（性能优化）
在 `_backfill_conditions` 调用 `_diagnostic_select_for_condition` 之前，先看主 metric SELECT 的 row 是否已经含目标字段：

```python
# 如果 first_row 已经有 condition.field 对应的 actual 值，跳过诊断 query
if main_query_row and (canonical_field_key(cond.field) in main_query_row):
    actual = main_query_row[canonical_field_key(cond.field)]
    self._emitter.update_condition_step(field=cond.field, actual=actual)
    continue  # 短路成功，不发诊断 query
# 否则跑诊断 query
```

要求：
- 短路逻辑要有 unit test 覆盖（短路路径 + 不短路路径各一）
- 短路触发时不应该 spam INFO log（DEBUG 即可），无匹配时仍走 WARN
- 行为等价：原来发 N 次诊断 query，现在最多 N 次，可能 0 次

### 测试（必须）
- `tests/unit/test_diagnostic_registry.py` (新)：
  - 注册表查不到 → graceful skip
  - 注册表命中 → 生成正确诊断 SELECT
  - 添加新 metric 项 → 不需要改函数体即可工作（用 monkeypatch 注入测试 spec）
- `tests/unit/test_backfill_short_circuit.py` (新)：
  - 主行已含字段 → 短路，无诊断 query 发出
  - 主行不含字段 → 走诊断 query，结果回填
- 运行 `uv run pytest tests/unit/ -m "not live_llm" --tb=short` 必须全绿

## 项目背景
- LIMS Python FastAPI 微服务（端口 18100），Stage1 KG 检索 + Stage2 NL2SQL。
- Plan v2.1 已合并：`docs/decisions/2026-04-30-nlq-tracer-bullet-v2/PLAN.md`（必读）
- 当前 stage2 实现在 `nlq-agent/src/pipelines/stage2/data_sql_agent.py:_diagnostic_select_for_condition`（先 Read 它再动手）
- 项目约定：`/data/project/lm-team/kimi/CLAUDE.md` 与 `.cursorrules`

## 执行约束
- **不准** spawn 子代理（不许 Task 工具）
- **不准**改 `web/`、`mobile/`、`api/`（.NET）— 只动 `nlq-agent/`
- 不准动 stage1（语义检索）
- 不准动 e2e 测试（保持 GLM round-1 那条 e2e 不变）
- 跑外部服务（docker / MySQL / Qdrant / LLM）— **不准**，只跑 unit test

## 提交规范
≥2 个 atomic commits（短任务允许更少 commit），Conventional Commits：
- `refactor(nlq-agent): generalize _diagnostic_select_for_condition via registry`
- `perf(nlq-agent): short-circuit backfill when main row already has field`

每条 commit message 第二段写 1-3 句 why。`git status` 完成时干净。

## 输出
完成时：
1. `git log --oneline 3a4e602..HEAD` 显示 ≥2 commit
2. `pytest tests/unit/ -m "not live_llm"` 全绿
3. 在 `/data/project/lm/.omc/team-nlq-r2/kimi/REPORT.md` 写：
   - 改动 #4 / #7 各自完成情况
   - 测试结果（passed 数、time）
   - 关键设计决策（DiagnosticSpec 是 dataclass 还是 NamedTuple，为什么）
   - 已知限制 / TODO

阻塞写到 `/data/project/lm/.omc/team-nlq-r2/kimi/BLOCKER.md` 然后退出。

## 1h 截止
你必须在 60 分钟内退出。剩余时间不够时优先做 #4（更核心），#7 不做也不算失败。

开始。先 Read PLAN.md，再 Read 当前 `_diagnostic_select_for_condition`，然后再动手。
