# NLQ-Agent Tracer-Bullet R2 + R3 + Real E2E — Decision Record

**Date:** 2026-05-01 02:21 → 03:05 CST (~45 min worker exec, then lead-driven autonomous merge)
**Merge commits:**
- [`344e52f`](../../../) Merge r3 KIMI (chat_stream defensive)
- [`4ad0190`](../../../) Merge r3 GLM (qdrant-client 1.10+ upgrade)
- [`d35d44c`](../../../) Merge r2 KIMI (diagnostic SELECT registry + F2 short-circuit)
- [`1da0a94`](../../../) Merge r2 GLM (TREND query type)
- [`31bd401`](../../../) `fix(nlq-agent): unblock real E2E + add RUNBOOK + mock ingest helper` (lead)

**Status:** ✅ All 4 worker branches merged + lead E2E unblock + 91 pytest passed + RUNBOOK shipped.

## Background

After Round-1 (ADR `2026-04-30-nlq-tracer-bullet-v2`) shipped the first vertical slice (statistical pass-rate query), three parallel work tracks ran on 2026-05-01:

1. **R2 KIMI** — refactor `_diagnostic_select_for_condition` from hard-coded if-else to metadata-driven `DIAGNOSTIC_FIELD_REGISTRY` + add F2 short-circuit when main row already contains target field. Added 12 unit tests.
2. **R2 GLM** — add `IntentType.TREND` (new query class beyond statistical) + stage1 routing + `合格率_趋势` SQL template + summary rendering. Added 23 tests across unit and e2e.
3. **R3 KIMI + GLM + Lead** — fire-fight bugs surfaced when the lead drove the real E2E SSE chain end-to-end:
   - Bug A (qdrant-client 1.10+ removed `AsyncQdrantClient.search`) — fixed by R3 GLM with `query_points()` upgrade + e2e regression test
   - Bug B (`chat_stream` `IndexError` on SiliconFlow heartbeat chunks with `choices=[]`) — fixed by R3 KIMI with 4-layer defensive parsing + 188 lines of unit tests
   - Bug C (`QDRANT_API_KEY=""` empty string → AsyncQdrantClient enables HTTPS, SSL handshake fails against local HTTP Qdrant) — fixed by lead in main with `or None` coercion
   - Bug D (post-merge `IntentType` import lost in `ort` strategy) — fixed by lead with manual import insertion

## Process

**5-hour autonomous mandate (02:55 → 07:55):** user delegated full A-G stage execution.
- A: round-3 worker spawn + monitor
- B: lead's main E2E unblock + RUNBOOK + commit
- C: 03:23 cron verification (consolidated round-2 + round-3 + lead status)
- D: merge 4 branches sequentially with pytest gates
- E: init_semantic_layer.py real-run validation (deferred — schema gap requires additional work)
- F: dispatch round-4 worker(s) for safe undone tasks (in progress at time of writing)
- G: temp-file cleanup (deferred — `.omc/team-nlq-r{2,3}/` are git-ignored)

Worker performance:
| Worker | Round | Tests Added | Wall Time | Commits |
|---|---|---|---|---|
| r2-KIMI | 2 | 12 unit | ~13 min | 2 atomic Conventional |
| r2-GLM | 2 | 23 unit + e2e | ~17 min | 3 atomic Conventional |
| r3-KIMI | 3 | 6 defensive unit | ~6 min | 1 atomic Conventional |
| r3-GLM | 3 | e2e regression + smoke | ~6 min | 2 atomic Conventional |

Round-3 was 2-3× faster than round-2 because the bug scope was tightly constrained by lead's real-stack diagnosis.

## Real E2E proof

After all merges + 4 fixes, a real `POST /api/v1/chat/stream` produced (2026-05-01 03:04):
- 6 `reasoning_step` (spec×2, rule×1, condition×2, grade×1)
- 164 `text` chunks (LLM streaming answer)
- 1 `response_metadata` with full SQL + reasoning_steps + sql_explanation
- 1 `done`
- Total ~344 SSE lines; LLM picked `F_PRODUCT_SPEC_CODE` correctly first try

See `docs/RUNBOOK_NLQ_E2E.md` §7 for full transcript and §8 for 6 troubleshooting recipes derived from this run.

## Layout

```
docs/decisions/2026-05-01-nlq-tracer-bullet-r2-r3/
├── README.md                 ← you are here
├── AUTONOMOUS_LEDGER.md      ← lead's 5h decision log (red lines + plan + rationale)
├── r2-kimi/{prompt,REPORT}.md
├── r2-glm/{prompt,REPORT}.md
├── r3-kimi/{prompt,REPORT}.md
└── r3-glm/{prompt,REPORT}.md
```

Round-2 prior plan reference: [`docs/decisions/2026-04-30-nlq-tracer-bullet-v2/PLAN.md`](../2026-04-30-nlq-tracer-bullet-v2/PLAN.md).

## What landed in main (this round)

**Round-3 fixes (4 worker commits + 1 lead commit):**
- `nlq-agent/src/services/llm_client.py` — chat_stream defensive (R3 KIMI)
- `nlq-agent/src/services/qdrant_service.py` — query_points + `api_key or None` (R3 GLM + lead)
- `nlq-agent/tests/unit/test_chat_stream_defensive.py` — 6 cases (R3 KIMI)
- `nlq-agent/tests/e2e/test_qdrant_api_compat.py` — regression (R3 GLM)
- `nlq-agent/pyproject.toml` — `live_qdrant` marker (R3 GLM)

**Round-2 features (5 worker commits):**
- `nlq-agent/src/pipelines/stage2/data_sql_agent.py` — registry + short-circuit + TREND branch (R2 KIMI + GLM)
- `nlq-agent/src/pipelines/stage1/semantic_kg_agent.py` — TREND intent routing (R2 GLM)
- `nlq-agent/src/models/{ddl,schemas}.py` — TREND template + IntentType.TREND (R2 GLM)
- `nlq-agent/src/utils/prompts.py` — TREND prompt few-shot (R2 GLM)
- `nlq-agent/tests/unit/test_{diagnostic_registry,backfill_short_circuit}.py` — registry + short-circuit (R2 KIMI)
- `nlq-agent/tests/unit/test_trend_{intent,sql_template}.py` + `tests/e2e/test_e2e_trend_query.py` — trend coverage (R2 GLM)

**Lead's deliverables:**
- `docs/RUNBOOK_NLQ_E2E.md` — 195-line ops manual with 8 troubleshooting recipes + real transcript
- `nlq-agent/scripts/mock_ingest_demo.py` — Qdrant ingest validation that bypasses MySQL (uses real TEI embeddings)
- IntentType import fix + qdrant_api_key `or None` coercion (Bug D + Bug C)

## Verification

```bash
$ cd /data/project/lm/nlq-agent && uv run pytest tests/ -m "not live_llm and not live_qdrant"
91 passed, 2 deselected in 0.83s
```

Real SSE end-to-end:
```bash
$ curl -N -X POST http://127.0.0.1:18100/api/v1/chat/stream \
    -d '{"messages":[{"role":"user","content":"50W470 牌号硅钢片样品的铁损 P17/50 合格率"}]}'
# → 344 lines, 6 reasoning_step + 164 text + 1 response_metadata + 1 done
```

## Key decisions / consequences

1. **Merge order: r3 → r2** — round-3 fixes the foundation, round-2 builds on top. Reverse order would have surfaced Bug A and Bug B during r2 testing and confused the diagnosis.
2. **Lead patched main directly during diagnosis, then reverted before worker merge** — avoided double-fix; r3 GLM's worktree did the canonical fix and we reset main to take theirs cleanly. The same approach worked for r3 KIMI's chat_stream fix.
3. **`ort` merge strategy auto-resolved stage2 conflict** between r2-KIMI (refactor) and r2-GLM (add TREND branch) — but **silently dropped `IntentType` from the import block**. Manual fix needed. This is a known weakness of structural-text auto-merge.
4. **Lead committed `RUNBOOK + mock_ingest_demo + qdrant_api_key fix + IntentType import fix` in one commit** — these belong together as "the real-E2E unblock package"; splitting would lose narrative.

## Out of scope (deferred for future iterations)

- E1+E2: `init_schema.sql` for fresh MySQL test container + real `init_semantic_layer.py` run (gen schema mismatch — needs production schema dump or hand-crafted DDL for ≥3 tables).
- F1: Frontend `<KgReasoningChain>` real wiring — `web/src/api/nlqAgent.ts` exists but no view consumes it. Cross-stack risk.
- F3: .NET event-bus → `/api/v1/sync/rules` callback for live Qdrant sync. Cross-stack.
- F4: SKILL.md route doc fix (`/api/v1/query` → `/api/v1/chat/stream`) — trivial doc-only PR.
- F6: 3 more query types (root-cause, by-shift, conceptual) — could go in next round.

## Tooling stack used (this round)

- `/oh-my-claudecode:team` (round-2 + round-3 dispatch)
- `claude-kimi` / `claude-glm` (CLI workers, same as round-1)
- `CronCreate` (1h verification fire — manually replaced with autonomous lead-driven merge)
- Lead's manual real-stack diagnosis (TEI / Qdrant / MySQL / SiliconFlow LLM endpoint validation)
