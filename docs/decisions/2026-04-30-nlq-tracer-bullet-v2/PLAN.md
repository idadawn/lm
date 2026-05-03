# Plan v2.1 — NLQ-Agent Stage1+Stage2 Tracer-Bullet (Consensus APPROVED)

> APPROVED via `/oh-my-claudecode:ralplan` consensus (2 iterations, Planner→Architect→Critic).
> See conversation log for full Architect/Critic verdicts.

## Target query (single E2E vertical slice)
"合格率 ≥ 75% 且 抽样数量 ≥ 100 的产品规格"
Exercises multi-condition back-fill with two distinct operators on two distinct fields.

## Principles (5)
1. Spec tightness over architectural reach.
2. One canonical reasoning list, mutated in place.
3. Mock LLM/Qdrant/Embedding, real MySQL.
4. Mitigations are commitments, not notes.
5. Reproducible verification.

## Decision drivers (lane-bound)
- **D1** every `ReasoningStep.kind=="condition"` has populated `actual` for the canonical query.
- **D2** mocked-LLM orchestration correctness AND nightly `live_llm` lane (`qualified_rate ± 2.0pp`). **`live_llm` lane is non-merge-gating; advisory only.**
- **D3** `validate_sql(sql).is_valid is True` AND executing returns `qualified_rate == 0.75` exact. **D3 binds to default `-m "not live_llm"` lane and IS the merge gate.**

## Option chosen — B
Mock LLM/Qdrant/Embedding + real MySQL via docker. (A real-LLM-in-CI rejected on cost/non-determinism; C pure-unit rejected because it cannot satisfy D3.)

## File changes (12)

| # | File | Change |
|---|---|---|
| F1 | `nlq-agent/src/pipelines/stage2/data_sql_agent.py:266` `_evaluate_condition` | operator-class branching (numeric / list IN-NOT IN / range BETWEEN); raise `ConditionEvalError` on type mismatch; **no silent `float(<list>)` coercion** |
| F2 | `nlq-agent/src/pipelines/stage2/data_sql_agent.py` (new `_diagnostic_select_for_condition`) | replaces `_BACKFILL_ALIASES`; per-condition `SELECT` reuses metric CTE; canonical Chinese field keys; batched per spec via `IN (:ids)`; SQL re-validated through `validate_sql` |
| F3 | `nlq-agent/src/services/sse_emitter.py:147` `update_condition_step` | docstring "matches by field, mutates in place, idempotent" + WARN log on no-match |
| F4 | `nlq-agent/src/pipelines/stage1/semantic_kg_agent.py:240-248` `_doc_type_to_step_kind` | `collection_metrics → "spec"` (lowercase) |
| F5 | `nlq-agent/src/models/ddl.py:174-196` `METRIC_SQL_TEMPLATES['合格率']` | project `sample_count`; group with `DATE_FORMAT(F_CREATORTIME,'%Y-%m')` for tz-stable buckets |
| F6 | `nlq-agent/tests/e2e/test_e2e_pass_rate.py` (new) | two-assertion contract: `validate_sql.is_valid` AND `qualified_rate==0.75 AND sample_count>=100`; per-step `actual is not None` |
| F7 | same file `@pytest.mark.live_llm` variant | real LLM, `qualified_rate ± 2.0pp` |
| F8 | `nlq-agent/tests/unit/test_evaluate_condition.py` (new) | ≥6 cases covering all operator classes |
| F9 | `nlq-agent/tests/unit/test_doc_type_to_step_kind.py` (new) | locks 6-kind enum equality with **`nlq-agent/packages/shared-types/src/reasoning-protocol.ts`** (SoT is `.ts`, NOT generated `.d.ts`) |
| F10 | `nlq-agent/tests/fixtures/seed_lab.sql` (new) | **12 product specs × ≥100 LAB_INTERMEDIATE_DATA samples per spec, ≥1 spec qualifying**; UTC timestamps; CLDEntityBase audit cols (`F_CREATORTIME/F_CREATORUSERID/F_ENABLEDMARK/F_TenantId`) populated per `.cursorrules` |
| F11 | `nlq-agent/docker-compose.test.yml` (new) | MySQL 8 `nlq-mysql-test:33307`, `TZ=UTC`, init from seed_lab.sql, healthcheck-gated |
| F12 | `nlq-agent/pyproject.toml` / `pytest.ini` | register `live_llm` marker |

## Diagnostic SELECT shape (F2)
```sql
WITH base AS ( <existing 合格率 CTE, F5-extended> )
SELECT product_spec_id,
       qualified_rate AS actual_合格率,
       sample_count   AS actual_抽样数量
FROM base
WHERE product_spec_id IN (:spec_ids)
```
One execution per `(spec_ids, condition_field)` group; results fan out into `update_condition_step(field=<canonical Chinese>, actual=<value>)`. No alias map; no-match logs WARN; e2e asserts no WARN.

## Verification block (copy-paste)
```bash
cd /data/project/lm/nlq-agent

docker compose -f docker-compose.test.yml up -d --wait
uv sync --frozen --extra test || pip install -e ".[test]"

ruff check src tests
mypy src

# Default (merge-gating) lane
pytest -m "not live_llm" -x --tb=short
pytest tests/unit/test_evaluate_condition.py -v
pytest tests/unit/test_doc_type_to_step_kind.py -v
pytest tests/e2e/test_e2e_pass_rate.py -v

# SoT sync check (if exists)
[ -f scripts/check-reasoning-protocol-sync.ps1 ] && pwsh scripts/check-reasoning-protocol-sync.ps1 || true

# Smoke
uvicorn src.api.main:app --port 8088 &
sleep 2
curl -N -X POST http://127.0.0.1:8088/api/v1/chat/stream \
  -H "Content-Type: application/json" \
  -d '{"query":"合格率不低于75%且抽样数量不少于100的产品规格"}'

# Nightly only (advisory, NOT merge-gating)
LIVE_LLM=1 pytest -m live_llm -v
```

## Risks & committed mitigations

| Risk | Mitigation |
|---|---|
| TZ drift dev↔CI | `DATE_FORMAT(F_CREATORTIME,'%Y-%m')` + UTC seed + container `TZ=UTC` |
| Audit cols missing | F10 seeds per `.cursorrules` |
| Wrong `kind` for product_spec | F4 commits `"spec"`; F9 locks against `.ts` SoT |
| `validate_sql` false-negative on diagnostic | F2 routes diagnostic SQL through same validator |
| Frontend breakage | no new event types; SSE shape verified against `nlqAgent.ts` + `reasoning-protocol.ts` |
| Lane drift / merge-gate ambiguity | D3 = default lane (gate); D2 = `live_llm` (advisory) |

## Out of scope
SKILL.md route fix (doc-only PR), generalising back-fill to non-`合格率` metrics, frontend formatting of `actual`, F2 short-circuit synthesis (Architect optional, deferred).

## Verified facts (from Architect's read of the codebase)
- Route is `POST /api/v1/chat/stream` at `nlq-agent/src/api/routes.py:31`. The SKILL.md mention of `/api/v1/query` is **stale**.
- `validate_sql` exists at `nlq-agent/src/services/database.py:58-81` with regex blocking INSERT/UPDATE/DELETE/DROP/CREATE/ALTER/TRUNCATE/GRANT/EXEC/CALL/SET/LOAD/INTO OUTFILE.
- `update_condition_step` matches by `field` (`nlq-agent/src/services/sse_emitter.py:147`) and mutates `_reasoning_steps` in place.
- `_evaluate_condition` crashes on text via `float(actual)` at `nlq-agent/src/pipelines/stage2/data_sql_agent.py:266`.
- `METRIC_SQL_TEMPLATES['合格率']` exists at `nlq-agent/src/models/ddl.py:174-196` and currently only emits `qualified_rate`.
- `_doc_type_to_step_kind` at `nlq-agent/src/pipelines/stage1/semantic_kg_agent.py:240-248` maps `collection_metrics → SPEC` (the only available kinds in protocol are 6 in `web/src/types/reasoning-protocol.d.ts`).
- Frontend dispatcher: `web/src/api/nlqAgent.ts:44, 80-92` for events `text`/`reasoning_step`/`response_metadata`/`error`/`done`.

## DoD
- All 12 file edits applied per spec.
- pytest `-m "not live_llm"` green.
- Verification block runs cleanly.
- 1+ atomic commits per logical unit (F1-F5 backend; F6-F9 tests; F10-F12 infra), Conventional Commits format.
- mypy + ruff clean.
- No frontend file modified.
- No new SSE event types.
