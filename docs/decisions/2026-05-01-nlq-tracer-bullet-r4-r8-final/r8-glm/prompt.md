# 你是 nlq-agent round-8 的 GLM 工人 — Load test 套件 + benchmark 报告

## 工作区（严格）
- worktree：**`/data/project/lm-team/glm/`**（分支 `omc-team/r8/glm-load-test`，基于 main `7d2c4cb`）
- **第一件事**：跑 `pwd` + `git rev-parse --abbrev-ref HEAD` 确认当前在你 worktree 的分支上。如果 cwd 不是 `/data/project/lm-team/glm/...` 或分支不是 `omc-team/r8/glm-load-test` → 立即 BLOCKER。
- **绝对禁止** 在 `/data/project/lm` 主仓库执行任何 git 操作（switch/checkout/reset/branch）。
- **绝对禁止** `cd` 出 `/data/project/lm-team/glm/` 范围。
- 主仓库与 KIMI worktree 禁止触碰。

## 范围 — 并发 Load Test

### 改动 #1 — `nlq-agent/tests/load/__init__.py` + `nlq-agent/tests/load/conftest.py`（新）
- 用 `httpx.AsyncClient` 对 `app` 直发请求（不用真启 uvicorn）
- 提供 fixtures: `mock_full_stack`（mock LLM+Qdrant+DB，用 round-2 e2e 同款 mock）

### 改动 #2 — `nlq-agent/tests/load/test_concurrent_streams.py`（新文件）
≥4 测试，全部 `@pytest.mark.load` (新 marker)：
- `test_10_concurrent_streams`: 并发 10 个 chat/stream 请求，所有 200 + 收到 done event
- `test_response_metadata_order_under_load`: 并发 5 个，每个最后一个事件必须是 done，response_metadata 在 done 前
- `test_no_event_loss`: 单个流 100 条 reasoning_step（mock LLM 多发），检查 emitter 全部递交
- `test_rate_limit_under_burst`: 并发 50 个 from same IP，检查 RateLimit middleware 拒掉超 30/min 的

### 改动 #3 — `nlq-agent/scripts/benchmark.py`（新，~150 行）
真 benchmark 脚本（默认不在 pytest 跑，要命令行触发）：
- 起 `app` via `httpx.ASGITransport`，并发 N（默认 20）个 chat/stream（预设 5 类查询轮换）
- 测量：单次 latency p50/p95/p99 + throughput req/s + error_rate
- 输出 markdown 报告：`benchmark_<timestamp>.md`
- 跑法：`python -m scripts.benchmark --concurrency 20 --requests 200 --output benchmark.md`
- 同样 mock 外部服务（不打真 LLM/Qdrant/DB）

### 改动 #4 — Update `nlq-agent/pyproject.toml` 注册 `load` marker
```toml
[tool.pytest.ini_options]
markers = [
    "live_llm: ...",
    "live_qdrant: ...",
    "load: 并发负载测试，可单独跑 pytest -m load",
]
```

### 改动 #5 — Update `nlq-agent/CONTRIBUTING.md` 引用 load marker（如何跑/不跑 load test）

## 执行约束
- **不准** spawn 子代理；只改 nlq-agent/ 下文件
- **不准**改 src/ 下任何 Python 代码
- **绝对禁止**主仓库 git 操作
- 跑外部服务 — 不准
- 现有 e2e 必须仍 passed (207)，其中包括 `pytest -m "not live_llm and not live_qdrant and not load"` 默认排除 load
- KIMI 在并行加 PRODUCTION_CHECKLIST + verify_env，不会与你撞文件

## 提交规范
≥2 atomic commits：
- `test(nlq-agent): concurrent stream load tests`
- `feat(nlq-agent): benchmark.py script + load marker registration`

每条 commit body 写 1-3 句 why。`Co-Authored-By: Claude Opus 4.7` attribution。`git status` 干净。

## 输出
1. `git log --oneline 7d2c4cb..HEAD` ≥2 commits
2. `pytest tests/ -m "not live_llm and not live_qdrant and not load"` 全绿（≥207）
3. `pytest tests/ -m load` 全绿（≥4 新）
4. `/data/project/lm/.omc/team-nlq-r8/glm/REPORT.md`：load 用例列表 + benchmark.py 接口 + 真跑 benchmark 输出片段（如果时间允许）+ commits

阻塞写到 `/data/project/lm/.omc/team-nlq-r8/glm/BLOCKER.md`。

## 时间约束
~25-30 min。优先 #1+#2+#4，#3 大可简短，#5 最后。

开始 — 先 pwd 验证 cwd！
