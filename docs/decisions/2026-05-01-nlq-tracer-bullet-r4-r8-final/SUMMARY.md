# NLQ-Agent Tracer-Bullet R4 → R8 — Final Summary

**Date:** 2026-05-01 03:30 → 08:50 CST (~5h, autonomous)
**Status:** ✅ R4 / R5 / R6 / R7 / **R8** all merged on `main`. Tracer bullet complete.
**Verification (post-R8):**
- `pytest tests/ -m "not live_llm and not live_qdrant and not load"` → **215 passed, 1 skipped, 6 deselected**
- `pytest tests/ -m load` → **4 passed, 218 deselected**
- Combined: **219 passing** (207 baseline + 8 verify_env + 4 load).

> Continues `2026-04-30-nlq-tracer-bullet-v2` (R1) and `2026-05-01-nlq-tracer-bullet-r2-r3` (R2 + R3 + Real E2E unblock).

## Rounds at a glance

| Round | KIMI | GLM | Wall | Tests delta |
|---|---|---|---|---|
| **R4** | `IntentType.ROOT_CAUSE` + 合格率_归因 SQL | `IntentType.BY_SHIFT` + 合格率_班次 SQL | ~20 min | +24 unit + e2e |
| **R5** | `IntentType.CONCEPTUAL` + skip-stage2 path + answer | `API.md` + 3× input middleware (size / length / rate) | ~25 min | +18 unit + e2e |
| **R6** | structured JSON logging + `correlation_id` middleware | Prometheus metrics + `/metrics` endpoint | ~22 min | +21 unit |
| **R7** | production `docker-compose.production.yml` + hardened `Dockerfile` + `.env.production.example` | GitHub Actions CI + dependabot + ruff config + `CONTRIBUTING.md` | ~28 min | +20 unit |
| **R8** | `PRODUCTION_CHECKLIST.md` + `verify_env.py` + 8 unit | 4× load tests + `benchmark.py` + load marker | ~7 min KIMI / ~22 min GLM | +12 (8 unit + 4 load) |

5 query intents now ship: **statistical / trend / by_shift / root_cause / conceptual**.

## What landed in `main` (R4 → R7)

### R4 — query type expansion (2026-05-01 04:10 → 04:30)

Two new `IntentType` branches plus stage1 routing + stage2 SQL templates + summary rendering.

- `nlq-agent/src/models/{ddl,schemas}.py`
  - `IntentType.ROOT_CAUSE` + `IntentType.BY_SHIFT`
  - `METRIC_SQL_TEMPLATES["合格率_归因"]` — joins INTERMEDIATE_DATA against the failure-cause column with `GROUP BY` reason, so the user gets a ranked failure-mode breakdown.
  - `METRIC_SQL_TEMPLATES["合格率_班次"]` — partitions by shift code, returning rows × shifts grid for the same metric.
- `nlq-agent/src/pipelines/stage1/semantic_kg_agent.py` — keyword pre-routes (`原因 / 失效 / 不合格原因` → ROOT_CAUSE; `班次 / 早班 / 中班 / 夜班` → BY_SHIFT) + LLM fallback classification.
- `nlq-agent/src/pipelines/stage2/data_sql_agent.py` — branch selection + summary template per intent.
- `nlq-agent/src/utils/prompts.py` — extended classification prompt + per-intent `extracted_entities` schema.
- Tests: `test_root_cause_intent.py`, `test_byshift_intent.py`, `test_root_cause_sql_template.py`, `test_byshift_sql_template.py`, e2e `test_e2e_root_cause_query.py` + `test_e2e_byshift_query.py`.

**Merge incidents:**
1. `ort` strategy union-merged the `METRIC_SQL_TEMPLATES` dict and produced a corrupt SQL string (literal SQL boundaries collided). Lead reverted the auto-merge and rebuilt both dict entries with `Edit`. Captured as a red-line "do not trust ort on dict-of-strings" rule.
2. `IntentType` import — same silent-drop pattern as R2. Manual `sed` patch.

### R5 — conceptual answers + input hardening (2026-05-01 04:30 → 05:00)

Two parallel tracks; both touched `src/` and required careful merge.

**KIMI — `IntentType.CONCEPTUAL`:**
- `nlq-agent/src/utils/prompts.py` — `CONCEPTUAL_ANSWER_SYSTEM` + `CONCEPTUAL_ANSWER_USER` prompts. Stage2 answers from KG context with no SQL.
- `nlq-agent/src/pipelines/stage1/semantic_kg_agent.py` — short-circuit when intent is conceptual: emit reasoning_steps for spec/rule retrieval and skip directly to stage2 answer mode.
- `nlq-agent/src/pipelines/stage2/data_sql_agent.py` — `_render_conceptual_answer` path gated on `intent_type == IntentType.CONCEPTUAL`.
- Tests: `test_conceptual_intent.py` + `test_e2e_conceptual_query.py`.

**GLM — input safety + API docs:**
- `nlq-agent/src/api/middleware.py`
  - `RequestSizeLimitMiddleware` (default 1 MB) → `413 Payload Too Large`
  - `QueryLengthGuardMiddleware` (default 4 KB on the user message) → `400 Bad Request`
  - `RateLimitInMemMiddleware` (token-bucket, default 30 req/min/IP) → `429 Too Many Requests`
- `nlq-agent/API.md` — 280-line reference covering `/api/v1/chat/stream` (SSE), `/api/v1/sync/rules`, `/api/v1/health`, plus error-code matrix.
- Tests: `test_request_size_limit.py`, `test_query_length_guard.py`, `test_rate_limit.py`.

### R6 — observability (2026-05-01 05:00 → 05:30)

**KIMI — structured logging + correlation:**
- `nlq-agent/src/core/logging_config.py` — `JSONFormatter` + `ContextVar` based `correlation_id` binding; pluggable via `LOG_FORMAT=json`.
- `nlq-agent/src/api/middleware.py` — `CorrelationIDMiddleware` reads/generates `X-Correlation-ID`, binds to `ContextVar`, echoes in response header.
- Tests: 11 unit tests in `test_correlation_middleware.py` + `test_json_formatter.py`.

**GLM — Prometheus metrics:**
- `nlq-agent/src/core/metrics.py`
  - `chat_stream_requests_total{intent_type,status}` — Counter
  - `chat_stream_latency_seconds{intent_type}` — Histogram (default buckets)
  - `chat_stream_active_streams` — Gauge (in-flight requests)
- `nlq-agent/src/api/main.py` — `/metrics` endpoint mounted via `prometheus_client.make_asgi_app()`.
- `nlq-agent/src/api/routers/chat.py` — instrumentation around `chat_stream` lifecycle.
- Tests: 10 unit tests + endpoint smoke.

No merge conflicts in this round — the two diffs touched disjoint files except `main.py` where both added imports/middleware mounts at non-overlapping lines.

### R7 — productionization (2026-05-01 05:30 → 07:30, with chaos)

**KIMI track (after recovery):**
- `nlq-agent/Dockerfile` — multi-stage `python:3.13-slim`; `uv sync --frozen --no-dev`; non-root `appuser` (UID 1000); `HEALTHCHECK` curl-on-/health; `CMD uvicorn ... --workers 2`.
- `nlq-agent/docker-compose.production.yml` — `nlq-agent` service with `restart: unless-stopped`, healthcheck (30s), `deploy.resources.limits` (cpus 2.0 / memory 2G), `json-file` log rotation (50m × 3), `env_file: .env.production`, `depends_on: {qdrant: healthy, tei: healthy}`. Plus qdrant + TEI GPU + TEI CPU fallback profiles.
- `nlq-agent/.env.production.example` — placeholder template (no secrets).
- `nlq-agent/API.md` — added "Production Deployment" section.
- `nlq-agent/tests/unit/test_docker_compose_config.py` — 9 structural tests (skips `docker compose config` when `.env.production` is absent — gitignored).

**GLM track:**
- `.github/workflows/nlq-agent-ci.yml` — 3 jobs (lint via ruff / pytest / `docker compose config` validation) on every push to `nlq-agent/**`.
- `.github/dependabot.yml` — pip + GitHub-Actions weekly cadence on the `nlq-agent` ecosystem.
- `nlq-agent/pyproject.toml` — ruff config block.
- `nlq-agent/CONTRIBUTING.md` — local-dev quickstart + commit conventions + branch naming (`omc-team/r<N>/<worker>-<topic>`).
- Tests: `test_ci_workflow_yaml.py` (validates required keys + job names + python-version matrix).

**Chaos:** R7-KIMI worker executed `git switch` inside `/data/project/lm` (the lead's main worktree) instead of its own `/data/project/lm-team/kimi/`, destroying the worktree directory. Recovery:
1. `git switch main` + `git checkout HEAD --` to restore working tree.
2. The worker had committed nothing remote yet, but **the untracked production files were salvageable from main's worktree.**
3. Lead salvaged `Dockerfile` diff, `docker-compose.production.yml`, `.env.production.example` and cherry-picked them as `1297685` directly on `main`, skipping the broken worker branch.
4. Stronger guard rails added to all subsequent prompts: explicit "first run `pwd` + `git rev-parse --abbrev-ref HEAD`; if cwd or branch wrong → BLOCKER" + "**绝对禁止**主仓库 git 操作 / cd 出 worktree".

### R8 — production polish (2026-05-01 08:20 → 08:50)

Strengthened-isolation prompt held this time — both workers stayed inside their worktree, no main-repo damage.

**KIMI track (~7 min, fastest round of all 16 dispatches):**
- `nlq-agent/PRODUCTION_CHECKLIST.md` — 127-line ✅/❌/⚠️ matrix. Self-scored 14 ✅ / 5 ⚠️ / 6 ❌ across Security / Observability / Performance / Reliability / Operations.
- `nlq-agent/scripts/verify_env.py` — 206-line `.env` / `.env.production` validator. Reads required vars, detects placeholder secrets (`YOUR_`, `sk-test`, empty), and pings LLM `/models` / Embedding `/health` / Qdrant `/healthz` / MySQL TCP. `--no-network` skips pings; exit code 0/1/2 = ok / missing-required / unreachable.
- `nlq-agent/tests/unit/test_verify_env.py` — 8 unit tests (132 lines) covering exit codes / placeholder detection / `--no-network` / network mocking.

**GLM track (~22 min):**
- `nlq-agent/tests/load/{__init__,conftest}.py` — `mock_full_stack` fixture using `httpx.AsyncClient` + `ASGITransport` (no real uvicorn) and a `MockOrchestrator` that emits a controlled SSE script.
- `nlq-agent/tests/load/test_concurrent_streams.py` — 4× `@pytest.mark.load`:
  - `test_10_concurrent_streams` — 10 parallel `chat/stream`, all 200 + `done` received.
  - `test_response_metadata_order_under_load` — 5 concurrent; `response_metadata` always before `done`, `done` is always last.
  - `test_no_event_loss` — single stream with 100 `reasoning_step` events, all 100 delivered.
  - `test_rate_limit_under_burst` — 50 concurrent same-IP, RateLimit middleware rejects >30/min (429).
- `nlq-agent/scripts/benchmark.py` — 189-line CLI: `python -m scripts.benchmark --concurrency 20 --requests 200 --output benchmark.md`. Measures per-request latency (p50 / p95 / p99) + throughput (req/s) + error_rate; rotates 5 query templates; mocks LLM/Qdrant/DB via `BenchmarkOrchestrator`; emits a timestamped markdown report.
- `nlq-agent/pyproject.toml` — registers `load` marker; default test run excludes load via `-m "not load"` convention.

**Merge:** ort auto-merged the only conflict (`CONTRIBUTING.md`, both added a section) cleanly without dropping content this time. Both KIMI's "Production Readiness" and GLM's "Load Testing & Benchmark" sections preserved.

## Worker performance trend

```
R1 ~25 min   (cold start, first slice)
R2 ~17 min   (incremental)
R3 ~6 min    (focused bug-fix, lead-diagnosed)
R4 ~20 min   (two new query types)
R5 ~25 min   (two large tracks: conceptual + middleware)
R6 ~22 min   (observability)
R7 ~28 min   (chaos absorbed: ~10 min lead recovery + ~18 min rework)
R8  ~7 min KIMI / ~22 min GLM  (production polish — strengthened isolation held)
```

Net: ~3 worker-hours of total exec spread across ~5.5 wall-hours of lead-driven coordination.

## Process highlights & captured rules

1. **Worktree isolation is non-negotiable.** R7-KIMI's `git switch` in `/data/project/lm` proved a single `cd` slip can destroy multiple worktrees. The R8 prompt template now opens with a hard pwd-and-branch verification step.
2. **`ort` merge silently drops imports.** Confirmed twice (R2 + R4). Always grep imports post-merge before committing — better yet, prefer cherry-pick when both branches touch the same import block.
3. **Lead direct-commit pattern works for emergency recovery.** R7 chaos was unblocked by salvaging untracked files from the destroyed worktree and committing them directly to main as a single recovery commit (`1297685`). The lost worker branch was force-deleted to keep history clean.
4. **Test markers are the right hygiene tool.** `live_llm` (R1), `live_qdrant` (R3), `load` (R8) all keep the default test run hermetic while letting opt-in suites exist in the same `tests/` tree.
5. **Worker time scales with conflict surface, not LOC.** R3 was the fastest round despite touching the most production-critical code, because the lead pre-diagnosed each bug and pre-scoped each prompt. R7 was the slowest because two parallel tracks both touched `nlq-agent/Dockerfile` adjacents and one of them blew up.

## Out of scope (intentionally deferred)

The remaining items hit the lead's **stop-signal** red lines (cross-stack risk / production schema dependency / human judgment required). They will not be auto-dispatched:

| Tag | Item | Why deferred |
|---|---|---|
| **F1** | Frontend `<KgReasoningChain>` real wiring (Vue) | Cross-stack: `web/src/api/nlqAgent.ts` exists but no view consumes it. Needs UX design call. |
| **F3** | .NET event-bus → `/api/v1/sync/rules` callback | Cross-stack: live Qdrant rule sync from the .NET monolith — needs API contract + transactional outbox decision. |
| **E** | Real `init_schema.sql` + real `init_semantic_layer.py` run | Production schema dependency: needs real MySQL dump or hand-crafted DDL for ≥3 tables. |

These are documented for the next contributor — see `nlq-agent/CONTRIBUTING.md` § "Roadmap" and the matching TODOs in source.

## Verification commands

```bash
cd /data/project/lm/nlq-agent
uv run pytest tests/ -m "not live_llm and not live_qdrant and not load"
# → 215 passed, 1 skipped, 6 deselected  (post-R8 default)
uv run pytest tests/ -m load
# → 4 passed, 218 deselected
# Combined: 219 passing

# Real-stack E2E (requires live LLM + Qdrant + MySQL — see docs/RUNBOOK_NLQ_E2E.md)
curl -N -X POST http://127.0.0.1:18100/api/v1/chat/stream \
  -d '{"messages":[{"role":"user","content":"50W470 牌号硅钢片样品的铁损 P17/50 合格率"}]}'

# Production smoke (after R7)
docker compose -f nlq-agent/docker-compose.production.yml --env-file .env.production config
docker compose -f nlq-agent/docker-compose.production.yml --env-file .env.production up -d
curl http://localhost:18100/metrics  # Prometheus exposition
curl http://localhost:18100/health   # liveness
```

## Layout

```
docs/decisions/2026-05-01-nlq-tracer-bullet-r4-r8-final/
└── SUMMARY.md           ← this file
```

Round-by-round prompts and worker REPORTs live under `.omc/team-nlq-r{4..8}/{kimi,glm}/{prompt,REPORT}.md` (gitignored intermediate state — promoted summary in this file).

## Closing

After 8 rounds + 16 worker dispatches + ~3 worker-hours + ~5 lead-hours, `nlq-agent` is ready for staging:

- ✅ 5 query intents end-to-end with real SSE proof on at least one
- ✅ Production observability (`correlation_id`, JSON logs, `/metrics`)
- ✅ Production hardening (input limits, rate limits, non-root container, healthchecks, resource limits)
- ✅ Production CI (ruff lint, pytest, compose validate, dependabot)
- ✅ Production runbook (`docs/RUNBOOK_NLQ_E2E.md`) + production checklist (R8) + env validator (R8)
- ❌ Frontend integration (F1), .NET event sync (F3), real schema bootstrap (E) — explicitly deferred

The remaining work is human-judgment territory and will not be auto-dispatched.
