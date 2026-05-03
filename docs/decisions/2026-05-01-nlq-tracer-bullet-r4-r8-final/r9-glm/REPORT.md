# R9 GLM Worker Report — Benchmark Baseline + Checklist

**Branch:** `omc-team/r9/glm-benchmark-baseline`
**Date:** 2026-05-01

---

## 1. Benchmark Baseline Results

| Metric | Value |
|--------|-------|
| p50 latency | 8.9 ms |
| p95 latency | 15.7 ms |
| p99 latency | 24.1 ms |
| Throughput | 977.0 req/s |
| Error rate | 40.0% (20/50 non-200) |
| Concurrency | 10 |
| Total requests | 50 |
| Wall time | 0.051s |

**Note:** 40% error rate in mock stack is the captured baseline. The mock `BenchmarkOrchestrator` may need investigation into SSE streaming response handling for the non-200 responses. This baseline serves as the regression anchor — any future run should not exceed these numbers without documented reason.

**Hotfix applied:** `scripts/benchmark.py` now stubs `qdrant_client`, `aiomysql`, `uvicorn` in `sys.modules` before `src.*` imports, so the script runs without real services installed.

---

## 2. PRODUCTION_CHECKLIST Items Completed (2 of ❌ → ✅)

### 2a. CORS allow_origins whitelist
- **Files changed:** `src/core/settings.py` (added `cors_allow_origins` field), `src/main.py` (new `_resolve_cors_origins()` + wired into `create_app()`)
- **Config:** `CORS_ALLOW_ORIGINS=https://a.com,https://b.com` (comma-separated); empty → `["*"]` with warning log
- **Tests:** `tests/unit/test_cors_middleware.py` — 7 tests (parsing, wildcard, whitelist rejection via httpx)
- **Commit:** `a82d439`

### 2b. Sentry error tracking integration
- **Files changed:** `src/core/sentry_integration.py` (new, ~30 lines), `src/core/settings.py` (added `sentry_dsn`), `src/main.py` (import + call in lifespan)
- **Config:** `SENTRY_DSN=https://xxx@sentry.io/123`; unset or SDK missing → no-op + log
- **Tests:** `tests/unit/test_sentry_integration.py` — 3 tests (no-DSN, DSN-present, missing-SDK)
- **Commit:** `a9c6350`

### Checklist rows flipped (❌→✅)
- **Security / CORS allow_origins 白名单** → ✅
- **Observability / Sentry 错误追踪集成** → ✅

### Updated stats: 16 ✅, 5 ⚠️, 4 ❌

---

## 3. Additional Fixes
- **Missing dev deps:** Added `qdrant-client`, `pyyaml`, `aiomysql`, `uvicorn` to dev dependency group — resolves 13 test collection errors. Commit: `870e205`.
- **CONTRIBUTING.md:** Added cross-link to `benchmark_baseline.md`. Commit: `ad3d83e`.

---

## 4. Test Results

| Suite | Result |
|-------|--------|
| Default (`not live_llm/qdrant/load`) | 219 passed, 6 failed (pre-existing SOCKS proxy env issue), 1 skipped |
| Load (`-m load`) | 4 passed |
| New tests (CORS + Sentry) | 10 passed |

The 6 failures in `test_chat_stream_defensive.py` are pre-existing (httpx SOCKS proxy error in this environment) — not caused by r9 changes.

---

## 5. Commits (3a86c86..HEAD)

```
870e205 chore(nlq-agent): add missing dev deps (qdrant-client, pyyaml, aiomysql, uvicorn)
ad3d83e docs(nlq-agent): flip CORS + Sentry to ✅ in checklist, add benchmark cross-link
a9c6350 feat(nlq-agent): optional Sentry error tracking via SENTRY_DSN
a82d439 feat(nlq-agent): configurable CORS allow_origins from env
cd3befc chore(nlq-agent): benchmark.py baseline run + report
4c8eb75 fix(nlq-agent): stub heavy deps in benchmark.py so it runs without real services
```

6 atomic commits. Working tree clean.
