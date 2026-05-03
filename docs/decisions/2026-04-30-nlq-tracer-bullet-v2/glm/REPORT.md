# GLM Worker — NLQ Tracer-Bullet v2.1 Acceptance Report

**Branch:** `omc-team/nlq-tracer/glm`
**Date:** 2026-04-30
**Plan:** `/data/project/lm/.omc/plans/nlq-tracer-bullet-v2.md`

## Completion Summary

| # | Feature | Status | Notes |
|---|---------|--------|-------|
| F1 | `_evaluate_condition` operator-class branching + `ConditionEvalError` | done | 3 classes: numeric (<=,>=,=,<,>), list (IN/NOT IN), range (BETWEEN); raises on type mismatch |
| F2 | `_diagnostic_select_for_condition` per-condition SELECT | done | Reuses 合格率 CTE, aggregates monthly rows to spec level, canonical Chinese field keys, SQL validated through `validate_sql` |
| F3 | `update_condition_step` docstring + WARN log | done | "matches by field, mutates in place, idempotent" docstring; `logger.warning` on no-match |
| F4 | `_doc_type_to_step_kind` collection_metrics → "spec" | done | Already correct in codebase: `collection_metrics` maps to `ReasoningStepKind.SPEC` (value `"spec"`) |
| F5 | `METRIC_SQL_TEMPLATES['合格率']` sample_count + DATE_FORMAT | done | Added `COUNT(*) AS sample_count`, `DATE_FORMAT(F_CREATORTIME, '%Y-%m') AS month_bucket`, both in SELECT and GROUP BY |
| F6 | E2E test: two-assertion contract | done | `validate_sql.is_valid` + `qualified_rate==75.0 AND sample_count>=100`; condition actual non-None |
| F7 | `@pytest.mark.live_llm` variant | done | `TestE2EPassRateLiveLLM` with ±2.0pp tolerance, `pytest.skip` by default |
| F8 | Unit test `_evaluate_condition` ≥6 cases | done | 20 test cases across 3 classes: numeric (9), list (6), range (5) |
| F9 | Unit test `_doc_type_to_step_kind` 6-kind lock | done | 11 test cases: 7 enum-vs-TSoT parametrized + 4 mapping tests |
| F10 | `seed_lab.sql` 12 specs × ≥100 samples | done | Stored procedure generates 12 specs with controlled qualified_pct; spec_001 has exactly 75% + 120 samples; UTC timestamps; CLDEntityBase audit cols |
| F11 | `docker-compose.test.yml` MySQL 8 :33307 | done | `nlq-mysql-test:33307`, `TZ=UTC`, healthcheck-gated, init from seed_lab.sql; `docker compose config` validates |
| F12 | Register `live_llm` marker | done | Added to `[tool.pytest.ini_options].markers` in `pyproject.toml` |

## Test Results

```
33 passed, 1 deselected (live_llm) in 0.69s
```

- `tests/unit/test_evaluate_condition.py`: 20 passed
- `tests/unit/test_doc_type_to_step_kind.py`: 11 passed
- `tests/e2e/test_e2e_pass_rate.py`: 2 passed (mocked)
- `tests/e2e/test_e2e_pass_rate.py`: 1 skipped (live_llm)

**Pre-existing issue:** `tests/test_pipeline.py::test_reject_insert` asserts `"INSERT" in error` but `validate_sql` returns `"仅允许 SELECT 查询"` first (non-SELECT check fires before forbidden-keyword check). Not caused by this PR.

## Key Decisions

1. **Diagnostic SELECT re-aggregation:** F5 adds `DATE_FORMAT` monthly grouping to the 合格率 template. The diagnostic SELECT (F2) re-aggregates monthly rows back to per-spec level via `GROUP BY F_PRODUCT_SPEC_ID`, computing `actual_合格率 = SUM(qualified_count)*100.0/NULLIF(SUM(total_count),0)` and `actual_抽样数量 = SUM(sample_count)`.

2. **Fallback path preserved:** `_backfill_conditions` tries diagnostic SELECT first; if no events generated (e.g., no 合格率 metric), falls back to direct first-row lookup.

3. **Condition step field matching:** Uses `cond.field` for `update_condition_step` matching, ensuring compatibility regardless of whether LLM produces Chinese or English field names.

## Known Limitations / TODO

- **F7 live_llm test** is a stub (`pytest.skip`); needs real LLM + docker-compose.test.yml to run.
- **Diagnostic scope** is limited to 合格率 metric; extending to other metrics (铁损均值, 叠片系数) is out-of-scope per plan.
- **seed_lab.sql** uses stored procedure + `UUID()` for F_ID; requires MySQL 8.0+.
- **F4** was already correct; no code change needed.

## Commit Hashes

```
7cafdaa feat(nlq-agent): F1-F5 stage2 backfill via diagnostic SELECT + DDL extension
f29b4f3 test(nlq-agent): F6-F9 e2e pass-rate + unit tests
ed88b6d chore(nlq-agent): F10-F12 fixtures + docker-compose + pytest markers
```

## Files Changed

```
nlq-agent/src/pipelines/stage2/data_sql_agent.py   (+156, -12)
nlq-agent/src/services/sse_emitter.py               (+18, -4)
nlq-agent/src/models/ddl.py                          (+6, -2)
nlq-agent/tests/e2e/test_e2e_pass_rate.py           (new, +230)
nlq-agent/tests/unit/test_evaluate_condition.py      (new, +115)
nlq-agent/tests/unit/test_doc_type_to_step_kind.py   (new, +113)
nlq-agent/tests/fixtures/seed_lab.sql                (new, +180)
nlq-agent/docker-compose.test.yml                    (new, +29)
nlq-agent/pyproject.toml                             (+3)
```
