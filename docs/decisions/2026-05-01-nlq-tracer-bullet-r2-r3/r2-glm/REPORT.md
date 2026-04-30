# GLM Worker — NLQ Round-2: Trend Query Type

**Branch:** `omc-team/r2/glm-trend-query`
**Date:** 2026-05-01
**Base:** `3a4e602` (main)

## Completion Summary

| # | Feature | Status | Notes |
|---|---------|--------|-------|
| T1 | `IntentType.TREND` enum value | done | Separate from STATISTICAL; distinct SQL routing |
| T2 | Intent classification prompt (5 types) + semantic extraction | done | trend keywords, time_window extraction hints |
| T3 | Keyword routing in `_classify_intent` + time_window regex | done | Pre-check bypasses LLM for trend queries |
| T4 | `METRIC_SQL_TEMPLATES['合格率_趋势']` | done | DATE_FORMAT monthly bucketing, ORDER BY month_bucket ASC |
| T5 | Template-based SQL gen + trend summary in Stage 2 | done | `_generate_trend_sql` skips LLM; summary shows rate change per spec |
| T6 | Unit test: trend intent classification | done | 11 cases — keyword routing, time_window, non-trend exclusion |
| T7 | Unit test: 合格率_趋势 SQL template validation | done | 9 cases — validate_sql, structure, parameter substitution |
| T8 | E2E test: trend query SSE pipeline | done | 3 cases — event sequence, SQL structure, grade summary |

## Test Results

```
71 passed, 1 deselected (live_llm) in 1.17s
```

- `tests/unit/test_trend_intent.py`: 11 passed
- `tests/unit/test_trend_sql_template.py`: 9 passed
- `tests/e2e/test_e2e_trend_query.py`: 3 passed
- `tests/e2e/test_e2e_pass_rate.py`: 2 passed (statistical e2e — compatibility verified)
- `tests/unit/test_evaluate_condition.py`: 20 passed
- `tests/unit/test_doc_type_to_step_kind.py`: 11 passed
- `tests/test_pipeline.py`: 15 passed

## Key Decisions

1. **TREND is a separate IntentType (not a STATISTICAL subclass).** Reason: trend queries have fundamentally different SQL patterns (time-series GROUP BY month, ORDER BY time, time-window WHERE) and different summary rendering (rate change over time vs point-in-time value). Stage 2 routing needs a clear signal to select the time-series template. A subclass approach would require additional branching logic inside STATISTICAL handling, increasing coupling.

2. **Template-based SQL generation for TREND (no LLM call).** The trend SQL structure is deterministic once `time_window_months` is known — no need for LLM variability. This avoids hallucination risk and makes the pipeline fully testable without a live LLM.

3. **Keyword pre-check in `_classify_intent` bypasses LLM.** Trend keywords (趋势/走势/环比/同比) are unambiguous in this domain. The regex-based pre-check returns immediately with IntentType.TREND, avoiding an unnecessary LLM round-trip. Falls through to LLM classification for all other intents.

## Known Limitations

- **Trend is limited to 合格率 metric.** Other trend metrics (铁损走势, 产量变化) would need additional templates — out of scope for this tracer bullet.
- **No frontend rendering yet.** Frontend currently renders trend results as a table; a line chart visualization is needed for proper trend display.
- **`extra_where` is always empty.** Non-time-dimension filters (e.g., "合格率趋势中只看A类产品") are not yet supported in the trend template.
- **`_retrieve_knowledge` treats TREND same as STATISTICAL.** May want dedicated Qdrant collection routing for trend queries in the future.

## Commit Hashes

```
5dfd77b feat(nlq-agent): T1-T3 stage1 trend intent classification
0b40f82 feat(nlq-agent): T4-T5 stage2 合格率_趋势 SQL template + summary
e40aa52 test(nlq-agent): T6-T8 trend query unit + e2e tests
```

## Files Changed

```
nlq-agent/src/models/schemas.py                        (+2, -1)
nlq-agent/src/utils/prompts.py                         (+26, -7)
nlq-agent/src/pipelines/stage1/semantic_kg_agent.py    (+22, -2)
nlq-agent/src/models/ddl.py                            (+25)
nlq-agent/src/pipelines/stage2/data_sql_agent.py       (+47, -1)
nlq-agent/tests/unit/test_trend_intent.py              (new, +72)
nlq-agent/tests/unit/test_trend_sql_template.py        (new, +77)
nlq-agent/tests/e2e/test_e2e_trend_query.py            (new, +276)
```
