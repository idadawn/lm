# GLM Load Test Report — Round 8

## Commits

```
054538d feat(nlq-agent): benchmark.py script + load marker registration
5ebbb07 test(nlq-agent): concurrent stream load tests
```

## New Files

| File | Purpose |
|------|---------|
| `tests/load/__init__.py` | Package marker |
| `tests/load/conftest.py` | `mock_full_stack` fixture (httpx.AsyncClient + ASGITransport, MockOrchestrator) |
| `tests/load/test_concurrent_streams.py` | 4 load tests (`@pytest.mark.load`) |
| `scripts/benchmark.py` | CLI benchmark script (~150 LOC) |

## Load Test Cases

| Test | What it verifies |
|------|-----------------|
| `test_10_concurrent_streams` | 10 parallel POST /api/v1/chat/stream — all 200 + `done` event received |
| `test_response_metadata_order_under_load` | 5 concurrent — `response_metadata` before `done`; `done` is always last |
| `test_no_event_loss` | Single stream with 100 `reasoning_step` events — all 100 delivered |
| `test_rate_limit_under_burst` | 50 concurrent same-IP — RateLimitInMem rejects >30/min (429) |

## benchmark.py Interface

```
python -m scripts.benchmark --concurrency 20 --requests 200 --output benchmark.md
```

Measures per-request latency (p50/p95/p99), throughput (req/s), error rate.
Uses 5 rotating query templates. Mocks LLM/Qdrant/DB via BenchmarkOrchestrator.
Outputs a markdown table report with timestamp.

## Test Results

### Existing tests (excluding load + live markers)

```
198 passed, 9 failed (pre-existing cwd issue), 1 skipped, 6 deselected
```

The 9 pre-existing failures are in `test_docker_compose_config.py` — tests use
`open("docker-compose.production.yml")` (relative path) and fail when pytest cwd
is not the nlq-agent directory. Not related to this change.

The 6 `test_chat_stream_defensive` tests also had a pre-existing issue (SOCKS proxy
env var in the test environment requiring `socksio`); they pass with proxy vars unset.

### Load tests

```
tests/load/test_concurrent_streams.py::test_10_concurrent_streams PASSED      [ 25%]
tests/load/test_concurrent_streams.py::test_response_metadata_order_under_load PASSED [ 50%]
tests/load/test_concurrent_streams.py::test_no_event_loss PASSED               [ 75%]
tests/load/test_concurrent_streams.py::test_rate_limit_under_burst PASSED       [100%]

4 passed in 0.09s
```

## pyproject.toml Changes

- Added `load` marker: `"load: 并发负载测试，可单独跑 pytest -m load"`
- Added dev deps: `pydantic-settings>=2.0`, `openai>=1.40`

## CONTRIBUTING.md Updates

- Documented `load` marker and how to run/skip load tests
- Added benchmark script usage section
