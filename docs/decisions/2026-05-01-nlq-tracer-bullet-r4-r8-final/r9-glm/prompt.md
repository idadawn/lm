# 你是 nlq-agent round-9 GLM 工人 — benchmark baseline + checklist 收尾

## 工作区（严格）
- worktree：**`/data/project/lm-team/glm/`**（分支 `omc-team/r9/glm-benchmark-baseline`，基于 main `3a86c86`）
- **第一件事**：`pwd` + `git rev-parse --abbrev-ref HEAD` 确认 cwd 与分支。错则 BLOCKER（写到 `/data/project/lm/.omc/team-nlq-r9/glm/BLOCKER.md`）。
- **绝对禁止** 在 `/data/project/lm` 主仓库执行任何 git 操作（switch/checkout/reset/branch/add/commit）。
- **绝对禁止** `cd` 出 `/data/project/lm-team/glm/` 范围。
- 主仓库与 KIMI worktree 禁止触碰。

## 范围 — 两件事

### 改动 #1 — 真跑 benchmark.py 出 baseline 报告（~10 min）
你 R8 写的 `nlq-agent/scripts/benchmark.py` 还没真跑过；现在跑一次留 baseline。

```bash
cd /data/project/lm-team/glm/nlq-agent
uv sync --frozen 2>&1 | tail -3
uv run python -m scripts.benchmark --concurrency 10 --requests 50 --output benchmark_baseline.md
# 看输出文件
cat benchmark_baseline.md | head -40
```

- 提交 `nlq-agent/benchmark_baseline.md` 到分支
- 在文件头加一段 metadata：
  ```markdown
  > **Date:** 2026-05-01
  > **Hardware:** <从 `lscpu | head -3` 取 model name>
  > **Concurrency:** 10, **Requests:** 50, **Mock stack** (no real LLM/Qdrant/DB)
  > **Purpose:** baseline for regression alerts; re-run before each release.
  ```
- 如果跑失败：在 BLOCKER.md 写明 stderr 头 30 行，跳到改动 #2

### 改动 #2 — PRODUCTION_CHECKLIST 至少 2 项 ❌→✅（~15 min）
读 `nlq-agent/PRODUCTION_CHECKLIST.md`，挑 2-3 个 ❌ 项目实做：

**优先候选**（任选其二）：
1. **CORS allow_origins 白名单中间件**：在 `nlq-agent/src/api/middleware.py` 加 `CorsAllowOriginsFromEnvMiddleware` 或直接用 fastapi 的 `CORSMiddleware`，从 `settings.cors_allow_origins`（`.env` 配置 `CORS_ALLOW_ORIGINS=https://a.com,https://b.com`，逗号分隔）；空则默认 `["*"]` 但日志告警。`nlq-agent/src/core/settings.py` 加字段 + 默认值。tests/unit 加一个 `test_cors_middleware.py` 验证白名单生效。
2. **Sentry/错误追踪集成（可选启用）**：`nlq-agent/src/core/sentry_integration.py` 新建（~30 行），读 `SENTRY_DSN` env，存在则 `sentry_sdk.init(...)` 否则 no-op + 日志。`src/main.py` 引入并在 lifespan startup 调用。`tests/unit/test_sentry_integration.py` 验证 no DSN 时 init 不被调，有 DSN 时被调（mock sentry_sdk）。
3. **container registry 推送策略文档**：`docs/CONTAINER_REGISTRY.md` 写 100-150 行，覆盖 GHCR / ACR / ECR 选项 + 推送命令 + tag 命名（`vMAJOR.MINOR.PATCH` + `:latest` + `:sha-<short>`）+ retention policy。无代码改动。

挑 2 个改完后，把 `nlq-agent/PRODUCTION_CHECKLIST.md` 对应行从 ❌ 翻成 ✅，并附上"修复 commit/文件"的指针。

### 改动 #3 — Update CONTRIBUTING.md（如适用）
如果你新增了 `benchmark_baseline.md` 或 `CONTAINER_REGISTRY.md`，在 `nlq-agent/CONTRIBUTING.md` 加一行 cross-link。

## 执行约束
- **不准** spawn 子代理
- **不准**改 `web/` 下任何代码（KIMI 的领域）
- **不准**改 `api/` 下任何 .NET 代码
- **不准**改 `nlq-agent/scripts/benchmark.py` 自身（除非跑不起来要 hotfix；hotfix 单独 atomic commit）
- 跑 benchmark 必须 mock（`BenchmarkOrchestrator` 已是 mock，无需真服务）
- **绝对禁止**主仓库 git 操作
- 现有 e2e 必须仍 passed (215 default + 4 load = 219)

## 提交规范
≥2 atomic commits：
- `chore(nlq-agent): benchmark.py baseline run + report`
- `feat(nlq-agent): <CORS or Sentry or registry doc title>`（按你选的两项各一条 commit）

每条 commit body 写 1-3 句 why。`Co-Authored-By: Claude Opus 4.7` attribution。`git status` 干净。

## 输出
1. `git log --oneline 3a86c86..HEAD` ≥2 commits
2. `pytest tests/ -m "not live_llm and not live_qdrant and not load"` 全绿（≥215）
3. `pytest tests/ -m load` 全绿（4）
4. `cat nlq-agent/benchmark_baseline.md | head -20` 输出片段
5. `/data/project/lm/.omc/team-nlq-r9/glm/REPORT.md`：
   - benchmark baseline 结果（p50/p95/p99 / throughput / error_rate）
   - 选了哪 2 项 checklist + 实作摘要
   - PRODUCTION_CHECKLIST 翻 ❌→✅ 行号清单
   - commits

阻塞写到 `/data/project/lm/.omc/team-nlq-r9/glm/BLOCKER.md`。

## 时间约束
~25 min。改动 #1 优先（10min），#2 选 2 项（15min）。#3 最后 2 分钟。

开始 — 先 pwd 验证 cwd！
