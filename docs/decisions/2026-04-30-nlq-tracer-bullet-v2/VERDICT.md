# NLQ-Agent Tracer-Bullet — 4h 验收 VERDICT

**Verified:** 2026-04-30 09:53 CST (cron-fired)
**Plan:** `/data/project/lm/.omc/plans/nlq-tracer-bullet-v2.md`
**Workers ran:** KIMI 13 min (06:02 完成) / GLM 17 min (06:06 完成)
**Both exited 0**, no `BLOCKER.md`, both wrote `REPORT.md`.

## TL;DR — 推荐方案

**主推 GLM 分支整体合并；从 KIMI 摘录 F4 注释作为可选补丁。**

GLM 在测试覆盖、Python 模块卫生（`__init__.py`）和 fallback 路径设计上更扎实；KIMI 在 F2 实现细节叙述与 commit message 简洁度上更紧。两者都 ralplan-spec compliant，但 **GLM 的测试数 (33) 是 KIMI 的 (19) 的 1.74 倍**，且 e2e 多了一个结构化断言。

---

## F1–F12 实施完整性

| # | 文件 | KIMI | GLM | 备注 |
|---|---|---|---|---|
| F1 | `data_sql_agent.py` `_evaluate_condition` 算子分支 | ✅ | ✅ | 两者都抛 `ConditionEvalError`；分支结构等价 |
| F2 | `_diagnostic_select_for_condition` per-cond SELECT | ✅ | ✅ | KIMI 533 行；GLM 526 行；GLM 多了 fallback 到 first-row 的二级路径 |
| F3 | `sse_emitter.update_condition_step` docstring + WARN | ✅ | ✅ | 两者一致 |
| F4 | `semantic_kg_agent` `collection_metrics → "spec"` | 🔧 cosmetic | 🔧 no-op | 现有枚举值已是 `"spec"` 小写。KIMI 加注释；GLM 不动。两者都符合 spec 行为，**两者都正确** |
| F5 | `ddl.py METRIC_SQL_TEMPLATES['合格率']` 加 `sample_count` + `DATE_FORMAT` | ✅ | ✅ | 等价 |
| F6 | `tests/e2e/test_e2e_pass_rate.py` 双断言契约 | ✅ 1 case | ✅ 2 cases | GLM 多一个 e2e 用例 |
| F7 | `@pytest.mark.live_llm` 变体 | ✅ | ✅ | 两者都加 skip 守门 |
| F8 | `test_evaluate_condition.py` ≥6 用例 | ✅ **13** | ✅ **20** | GLM 多 7 个用例 |
| F9 | `test_doc_type_to_step_kind.py` 6-kind 枚举锁 | ✅ **5** | ✅ **11** | GLM 多 6 个用例（含 7 个 parametrized + 4 mapping） |
| F10 | `tests/fixtures/seed_lab.sql` 12×≥100 + UTC + audit | ✅ 210 行 | ✅ 178 行 | KIMI 显式列举；GLM 用存储过程更紧凑 |
| F11 | `docker-compose.test.yml` MySQL :33307 + TZ=UTC + healthcheck | ✅ 23 行 | ✅ 33 行 | 都 `docker compose config` 通过；GLM 多了网络/卷的显式声明 |
| F12 | `pyproject.toml` `live_llm` marker 注册 | ✅ | ✅ | 等价 |

---

## 代码质量

| | KIMI | GLM |
|---|---|---|
| pytest `-m "not live_llm"` (新测试) | **19 passed, 1 deselected** | **33 passed, 1 deselected** |
| 测试断言密度 | F6 1 case | F6 2 cases |
| Python 模块卫生 | ❌ 缺 `tests/e2e/__init__.py` 与 `tests/unit/__init__.py` | ✅ 两个 `__init__.py` 都建了 |
| 静态检查（ruff/mypy） | 工具未在系统 PATH | 工具未在系统 PATH |
| docker compose config 校验 | ✅ | ✅ |
| F2 设计 | 单路径 diagnostic SELECT | diagnostic + first-row fallback（更鲁棒） |

GLM 的 fallback 路径在主路径无匹配指标时仍能回填，更稳。

---

## 文档

| | KIMI | GLM |
|---|---|---|
| REPORT.md 字数 | ~1700 (中英混合) | ~2300 (英文为主) |
| F# 表格 | 详细，每行注明关键决策 | 详细 + 单独"Key Decisions"段落讨论 fallback / 字段匹配策略 |
| Known Limitations | 4 项（含 uv.lock 未提交） | 4 项 |
| 自检 checklist | ✅ 列出 5 项自验 | ❌ 没有自检 checklist 段 |
| 内联代码注释 | KIMI 在 F4 加了一行 inline comment | GLM 决策依据写进了 commit body 与 REPORT |

略微平局，KIMI 的"自检"小节是个加分项，但 GLM 的"Key Decisions"段可读性更好。

---

## 提交规范

| | KIMI | GLM |
|---|---|---|
| commits | 3 | 3 |
| Conventional Commits | ✅ | ✅ |
| atomic（按 F# 段落分） | ✅ F1-F5 / F6-F9 / F10-F12 | ✅ 同 |
| why-paragraph (≥1 句) | ✅ 4-5 行中文 why | ✅ 4-5 行中文 why |
| `Co-Authored-By:` 行 | ❌ 无 | ✅ `Claude Opus 4.7` |
| working tree 干净 | ⚠️ `?? uv.lock` 未跟踪 | ✅ 全干净 |

GLM 略胜 — clean working tree + co-author 归属规范。KIMI 的 `uv.lock` 不提交不一定算错（uv lock 文件团队决定是否纳管），但 spec 要求"git status 干净"，KIMI 严格说违规一档。

---

## 综合评分

| 维度 | KIMI | GLM | 优胜 |
|---|---|---|---|
| Spec 完整性 | 12/12 ✅ | 12/12 ✅ | 平 |
| 代码质量 | 19 tests, 缺 `__init__.py` | 33 tests, fallback 更稳 | **GLM** |
| 文档 | 自检 checklist 是亮点 | Key Decisions 段更深入 | 平 |
| 提交规范 | working tree 不干净（uv.lock） | 全干净 + co-author | **GLM** |

---

## 推荐合并方案

### 方案 A（推荐）— 整体采用 GLM
```bash
cd /data/project/lm
git merge --no-ff omc-team/nlq-tracer/glm
```
- 拿到 33 测试 + fallback 路径 + 干净 working tree
- 放弃 KIMI 的 F4 inline comment（无功能损失，注释本可后续 PR 补）

### 方案 B — GLM 主体 + KIMI F4 cherry-pick
```bash
cd /data/project/lm
git merge --no-ff omc-team/nlq-tracer/glm
git cherry-pick -n 003f5116~..003f5116 -- nlq-agent/src/pipelines/stage1/semantic_kg_agent.py
git commit -m "docs(nlq-agent): annotate collection_metrics → spec mapping"
```
- 多一行注释，几乎没风险

### 方案 C — KIMI 主体（不推荐）
仅当你强偏好 KIMI 风格时。需手补 `__init__.py`、`uv.lock` 决策、加测试。

---

## 待你授权才执行的动作
1. **合并哪个分支到 main？**（推荐 A 或 B）
2. **删除败者 worktree 与分支？**（`git worktree remove --force /data/project/lm-team/<loser>` + `git branch -D omc-team/nlq-tracer/<loser>`）
3. **保留两份 worktree 你自己再 review？**（现状）

我**不会**自作主张 merge / delete。等你点头。
