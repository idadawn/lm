# 你是 nlq-agent round-8 的 KIMI 工人 — 生产就绪 checklist + .env 验证脚本

## 工作区（严格）
- worktree：**`/data/project/lm-team/kimi/`**（分支 `omc-team/r8/kimi-prod-checklist`，基于 main `7d2c4cb`）
- **第一件事**：跑 `pwd` + `git rev-parse --abbrev-ref HEAD` 确认当前在你 worktree 的分支上。如果 cwd 不是 `/data/project/lm-team/kimi/...` 或分支不是 `omc-team/r8/kimi-prod-checklist` → 立即 BLOCKER。
- **绝对禁止** 在 `/data/project/lm` 主仓库执行任何 git 操作（switch/checkout/reset/branch）— 主仓库是 lead 持有，被破坏会丢失同步状态！
- **绝对禁止** `cd` 出 `/data/project/lm-team/kimi/` 范围。所有命令必须以你 worktree 为 cwd。
- 主仓库与 GLM worktree 禁止触碰。

## 范围 — 上线前必备的 doc + tooling

### 改动 #1 — `nlq-agent/PRODUCTION_CHECKLIST.md`（新文件，~150 行）
覆盖 5 大类，每条带 ✅/❌/⚠️ 状态 + 修复链接：

**Security:**
- [ ] LLM_API_KEY rotation policy（季度/月度）
- [ ] QDRANT_API_KEY 启用 + read-only role
- [ ] MySQL 只读账号（不许 INSERT/UPDATE/DELETE 权限）
- [ ] `validate_sql` 单元测试覆盖所有禁止操作（INSERT/UPDATE/DELETE/DROP/...）
- [ ] `.env.production` 不入版本控制（已 .gitignore）
- [ ] CORS allow_origins 白名单 (FastAPI middleware 检查)
- [ ] RequestSizeLimit / QueryLengthGuard / RateLimit 中间件启用 (r5 已加)

**Observability:**
- [ ] /health 端点返回所有依赖状态 (Qdrant/MySQL/LLM)
- [ ] /metrics 端点 exposed (r6 已加)
- [ ] structured JSON logging 启用 (LOG_FORMAT=json) (r6 已加)
- [ ] correlation_id 注入 (r6 已加)
- [ ] Sentry / 错误追踪集成（暂未做）

**Performance:**
- [ ] uvicorn workers ≥ 2 (Dockerfile 已设)
- [ ] Qdrant collection 索引优化（HNSW m, ef_construct 默认值）
- [ ] LLM streaming chunk_size 合理
- [ ] 连接池: MySQL aiomysql pool_recycle, Qdrant grpc_port

**Reliability:**
- [ ] 重启策略 restart=unless-stopped (r7 已设)
- [ ] healthcheck 配置 (r7 已设)
- [ ] graceful shutdown lifespan 处理
- [ ] backpressure: active_chat_streams Gauge 监控 (r6 已加)

**Operations:**
- [ ] 部署手册: docs/RUNBOOK_NLQ_E2E.md
- [ ] 文档: nlq-agent/API.md (r5 已写)
- [ ] CI/CD: .github/workflows/nlq-agent-ci.yml (r7 已加)
- [ ] dependabot config (r7 已加)
- [ ] container registry 推送策略（GHCR/ACR/...） — 待加

每个 ❌ 项加一段 1-2 句"如何修复"，带文件路径或 PR 建议。

### 改动 #2 — `nlq-agent/scripts/verify_env.py`（新文件，~100 行）
.env 配置验证脚本：
- 加载 .env (或 .env.production via `--env-file` 参数)
- 检查所有必填 vars: LLM_BASE_URL/LLM_API_KEY/LLM_MODEL/EMBEDDING_BASE_URL/QDRANT_HOST/QDRANT_PORT/MYSQL_*
- 检查 LLM_API_KEY 不是 placeholder（不以 "YOUR_"/"sk-test"/空 开头）
- 实际 ping 各服务（可选 `--no-network` 跳过）：
  - LLM: HEAD `<base>/models` (5s timeout)
  - Embedding: HEAD `<base>/health` (5s timeout)
  - Qdrant: HEAD `http://<host>:<port>/healthz` (5s timeout)
  - MySQL: TCP connect 测试
- 输出彩色报告：`✅ <var> ok` / `❌ <var> missing` / `⚠️ <service> unreachable`
- 退出码 0=全 ok，1=有 ❌，2=有 ⚠️

### 改动 #3 — `tests/unit/test_verify_env.py`（新文件）
- ≥4 用例：所有 vars 齐全 → 退出 0；缺 LLM_API_KEY → 退出 1；placeholder key → 退出 1；网络全失败 + `--no-network` → 退出 0

### 改动 #4 — Update `nlq-agent/CONTRIBUTING.md` 引用 PRODUCTION_CHECKLIST + verify_env

## 执行约束
- **不准** spawn 子代理；只改 nlq-agent/ 下文件
- **不准**改 src/ 下任何 Python 代码（只新建 scripts/verify_env.py + tests/unit/test_verify_env.py + 文档）
- **绝对禁止**主仓库 git 操作
- 跑外部服务（docker / MySQL / Qdrant / LLM）— 不准（verify_env 测试用 mock）
- 现有 e2e 必须仍 passed (207)

## 提交规范
≥2 atomic commits：
- `docs(nlq-agent): production readiness checklist`
- `feat(nlq-agent): verify_env.py + tests`

每条 commit body 写 1-3 句 why。`Co-Authored-By: Claude` attribution。`git status` 干净。

## 输出
1. `git log --oneline 7d2c4cb..HEAD` ≥2 commits
2. `pytest tests/ -m "not live_llm and not live_qdrant"` 全绿（≥211 = 207 + 4 new）
3. `/data/project/lm/.omc/team-nlq-r8/kimi/REPORT.md`：列出新建文件 + 已 ✅/❌ 项目数 + commits

阻塞写到 `/data/project/lm/.omc/team-nlq-r8/kimi/BLOCKER.md`。

## 时间约束
~25 min。优先 #1 + #2 + #3，#4 简短。

开始 — 先 pwd 验证 cwd！
