# NLQ-Agent Stage1+Stage2 Tracer-Bullet — Decision Record

**Date:** 2026-04-30 / 2026-05-01 (派发→验收→合并→归档)
**Merge commit:** [`2fe0fd8`](../../../) `Merge branch 'omc-team/nlq-tracer/glm' into main`
**Test fix follow-up:** [`aafc275`](../../../) `test(nlq-agent): de-pin test_reject_insert error-message keyword`
**Status:** ✅ Shipped to `origin/main`

## Background

`nlq-agent/` 是 LIMS 的 Python FastAPI 微服务（端口 18100），实现 NL→SQL 两阶段管线（Stage1 语义 KG 检索 + Stage2 NL2SQL）。本次任务是**端到端贯通一个查询**的 tracer-bullet 实现，目标查询：

> "合格率 ≥ 75% 且 抽样数量 ≥ 100 的产品规格"

之所以选这个查询：它强制覆盖**多条件 back-fill 路径**（两个不同算子、两个不同字段），是检验整条管线最有信号量的最小切片。

## Process

1. **Planning** — `/oh-my-claudecode:ralplan nlq-agent`
   Planner → Architect → Critic 共识循环 2 轮，第 2 轮 Critic APPROVE。最终 [PLAN.md](PLAN.md)（v2.1，12 项 F1–F12 文件改动 + RALPLAN-DR 短模式总结 + ADR 骨架）。

2. **Dispatch** — `/oh-my-claudecode:team` 派发给两个独立 CLI worker：
   - `claude-kimi`（Moonshot Kimi 后端）→ git worktree `omc-team/nlq-tracer/kimi`
   - `claude-glm`（智谱 GLM 后端，opus tier）→ git worktree `omc-team/nlq-tracer/glm`
   - 两个 worker 用 tmux detached pane 跑 `claude -p` headless，并行实施完整 12 项改动。

3. **Verification** — 4 小时后 cron 触发验收 agent，按 spec 完整性 + 代码质量 + 文档 + 提交规范四维评分。

## Verdict (Why GLM)

详见 [VERDICT.md](VERDICT.md)。摘要：

| 维度 | KIMI | GLM | 优胜 |
|---|---|---|---|
| F1–F12 完整性 | 12/12 ✅ | 12/12 ✅ | 平 |
| 新测试用例数 | 19 passed | **33 passed** | **GLM** |
| Python 模块卫生（`__init__.py`）| ❌ 缺 | ✅ | **GLM** |
| F2 设计 | 单路径 | diagnostic + first-row fallback | **GLM** |
| working tree 清洁 | ⚠️ uv.lock 未跟踪 | ✅ | **GLM** |
| Commit 规范 | 3 atomic Conventional ✅ | 3 atomic Conventional + co-author ✅ | 平 |
| REPORT 文档质量 | 自检 checklist 是亮点 | Key Decisions 段更深入 | 平 |

**结论：整体合并 GLM 分支**（方案 A）。KIMI 的 F4 cosmetic 注释无功能价值，未 cherry-pick。

## Layout

```
docs/decisions/2026-04-30-nlq-tracer-bullet-v2/
├── README.md             ← 你正在读
├── PLAN.md               ← Plan v2.1（ralplan-approved）
├── VERDICT.md            ← 4h 验收对比评分卡
├── kimi/
│   ├── prompt.md         ← lead 派给 KIMI worker 的指令
│   └── REPORT.md         ← KIMI worker 自报实施情况
└── glm/
    ├── prompt.md         ← lead 派给 GLM worker 的指令（与 kimi 几乎相同，仅工人身份字段不同）
    └── REPORT.md         ← GLM worker 自报实施情况（最终被采纳）
```

未归档：每个 worker 的 `output.log`（仅含退场摘要，与 REPORT.md 重复）。

## What landed in main

11 files changed, 888 insertions(+), 30 deletions(-)：

```
nlq-agent/docker-compose.test.yml                 (new, +33)   F11
nlq-agent/pyproject.toml                          (+3)         F12 live_llm marker
nlq-agent/src/models/ddl.py                       (+4, -2)     F5  合格率 + sample_count + DATE_FORMAT
nlq-agent/src/pipelines/stage2/data_sql_agent.py  (+187, -12)  F1+F2 算子分支 + 诊断 SELECT
nlq-agent/src/services/sse_emitter.py             (+13, -4)    F3  docstring + WARN log
nlq-agent/tests/e2e/__init__.py                   (new, 0)
nlq-agent/tests/e2e/test_e2e_pass_rate.py         (new, +272)  F6+F7 双断言契约 + live_llm
nlq-agent/tests/fixtures/seed_lab.sql             (new, +178)  F10 12 specs × 100+ samples
nlq-agent/tests/unit/__init__.py                  (new, 0)
nlq-agent/tests/unit/test_doc_type_to_step_kind.py (new, +105) F9
nlq-agent/tests/unit/test_evaluate_condition.py   (new, +111)  F8
```

加 1 条独立提交 `aafc275`：修预先存在的 `test_reject_insert`（`validate_sql` 先以"仅允许 SELECT"门禁拦截非-SELECT，错误消息不含"INSERT"，断言去掉关键字 pin）。

## Verification

```
$ uv run pytest tests/ -m "not live_llm" --tb=short
48 passed, 1 deselected (live_llm) in 0.57s
```

`docker compose -f nlq-agent/docker-compose.test.yml config` 校验通过（未实跑容器；e2e 当前用 mock）。

## Future work (out of scope of this slice)

- **`live_llm` advisory lane**：F7 已留接口（`@pytest.mark.live_llm`），但需要真正配置 LLM 后端 + docker-compose 启动 MySQL 才能跑。建议作 nightly CI 单独 lane，非 merge-gating。
- **诊断 SELECT 泛化**：当前 `_diagnostic_select_for_condition` 仅识别 `合格率/抽样数量/qualified_rate/sample_count` 四个字段键。下一个 metric 模板（铁损均值、叠片系数等）落地时再扩展。
- **SKILL.md 路由更新**：`nlq-agent/skills/nlq-two-stage/SKILL.md` 里写的端点是 `/api/v1/query`，实际是 `/api/v1/chat/stream`。Architect 已识别但留作独立 doc-only PR。
- **F2 短路化（Architect 可选合成）**：若主 metric SELECT 已包含目标字段，`_diagnostic_select_for_condition` 可短路跳过额外 query；当前实现是无脑跑两次。

## Tooling stack used

- `/oh-my-claudecode:ralplan` — 共识规划（Planner/Architect/Critic 各 ~1 轮，最终 APPROVE-WITH-DOC-EDITS）
- `/oh-my-claudecode:team` — 多 CLI worker 派发（tmux detached + git worktree 隔离）
- `claude-kimi` / `claude-glm` — 用户本地 alias（`CLAUDE_CONFIG_DIR=$HOME/.claude-{kimi,glm} claude --dangerously-skip-permissions`），分别走 Kimi / GLM 后端
- `CronCreate` — 4h 后 cron 自动触发验收 agent

可作未来类似切片的范式：plan → 双 worker 派发 → 4h 验收 → 选优合并。
