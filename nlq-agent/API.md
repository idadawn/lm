# NLQ-Agent API Documentation

> Version: 0.1.0 | Port: 18100 | Protocol: FastAPI + SSE

## Service Overview

NLQ-Agent is a two-stage NL→SQL microservice for laboratory data analysis. It accepts natural language queries in Chinese, resolves business context via a knowledge graph (Stage 1), generates and executes SQL (Stage 2), and streams results as Server-Sent Events (SSE).

**Base URL:** `http://<host>:18100`

**Dependencies:**
```
┌─────────────┐     ┌──────────────────────┐     ┌───────────┐
│   Client     │────▶│   FastAPI (18100)     │────▶│   MySQL   │
│  (Frontend)  │◀────│                      │◀────│  (33307)  │
└─────────────┘     │  Stage1: SemanticKG   │     └───────────┘
                    │  Stage2: DataSQLAgent │     ┌───────────┐
                    │                      │────▶│  Qdrant    │
                    │                      │◀────│  (6333)    │
                    │                      │     └───────────┘
                    │                      │     ┌───────────┐
                    │                      │────▶│  TEI       │
                    │                      │◀────│  (8001)    │
                    └──────────────────────┘     └───────────┘
                                │                ┌───────────┐
                                └───────────────▶│  LLM API   │
                                    (SSE stream) │ (external) │
                                                 └───────────┘
```

---

## POST /api/v1/chat/stream

Core endpoint — SSE streaming NL→SQL Q&A.

### Request

```json
{
  "messages": [
    {"role": "user", "content": "50W470 牌号硅钢片的铁损合格率"}
  ],
  "session_id": "optional-session-id",
  "model_name": "optional-model-override"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `messages` | `ChatMessage[]` | Yes | Conversation history; last message is the current query |
| `session_id` | `string` | No | Session identifier for multi-turn context |
| `model_name` | `string` | No | Override the default LLM model |

`ChatMessage`:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `role` | `string` | Yes | One of: `user`, `assistant`, `system` |
| `content` | `string` | Yes | Message text |

### Response

**Content-Type:** `text/event-stream`
**Headers:** `Cache-Control: no-cache`, `Connection: keep-alive`, `X-Accel-Buffering: no`

#### SSE Event Types

Events are streamed in the following order:

```
reasoning_step (×N) → text (×N) → response_metadata (1×) → done (1×)
```

On error: `error` event replaces the normal flow at any point.

| Event | Description |
|-------|-------------|
| `reasoning_step` | Knowledge graph reasoning progress (see kinds below) |
| `text` | Streaming LLM answer chunk |
| `response_metadata` | Final payload with SQL, row count, reasoning summary |
| `done` | Stream termination signal |
| `error` | Error with code and message |

#### Reasoning Step Kinds

Each `reasoning_step` has a `kind` field:

| Kind | Description |
|------|-------------|
| `spec` | Product specification matched |
| `rule` | Judgment rule matched |
| `condition` | Filter condition extracted (e.g., `F_PERF_PS_LOSS ≤ 1.08`) |
| `grade` | Grade/quality level determined |
| `record` | Diagnostic record lookup (fallback for unmatched collections) |
| `fallback` | Fallback reasoning when no match found |

#### response_metadata Payload

```json
{
  "reasoning_steps": [
    {
      "kind": "spec|rule|condition|grade|record|fallback",
      "label": "string",
      "detail": "string|null",
      "satisfied": "bool|null",
      "field": "string|null",
      "expected": "string|null",
      "actual": "string|number|null",
      "meta": "object|null"
    }
  ],
  "sql": "SELECT ...",
  "sql_explanation": "Human-readable SQL description",
  "row_count": 0,
  "truncated": false
}
```

### Supported Query Types

#### 1. Statistical (合格率/产量/均值)

```bash
curl -N -X POST http://localhost:18100/api/v1/chat/stream \
  -H 'Content-Type: application/json' \
  -d '{"messages":[{"role":"user","content":"50W470 牌号硅钢片的铁损 P17/50 合格率"}]}'
```

#### 2. Trend (按时间维度变化)

```bash
curl -N -X POST http://localhost:18100/api/v1/chat/stream \
  -H 'Content-Type: application/json' \
  -d '{"messages":[{"role":"user","content":"50W470 近三个月铁损趋势"}]}'
```

#### 3. By Shift (按班次对比)

```bash
curl -N -X POST http://localhost:18100/api/v1/chat/stream \
  -H 'Content-Type: application/json' \
  -d '{"messages":[{"role":"user","content":"各班次 50W470 合格率对比"}]}'
```

#### 4. Root Cause (根因分析)

```bash
curl -N -X POST http://localhost:18100/api/v1/chat/stream \
  -H 'Content-Type: application/json' \
  -d '{"messages":[{"role":"user","content":"为什么 50W470 本月合格率下降了"}]}'
```

#### 5. Conceptual (概念解释)

```bash
curl -N -X POST http://localhost:18100/api/v1/chat/stream \
  -H 'Content-Type: application/json' \
  -d '{"messages":[{"role":"user","content":"铁损 P17/50 是什么意思"}]}'
```

### Error Codes

| HTTP Status | Code | When |
|-------------|------|------|
| 413 | `PAYLOAD_TOO_LARGE` | Body > 100 KB |
| 422 | `QUERY_TOO_LONG` | Last message > 2000 chars |
| 429 | `RATE_LIMITED` | > 30 req/min/IP |
| 500 | (SSE error event) | Internal pipeline failure |

All errors use the JSON shape: `{"error": {"code": "<code>", "message": "<msg>"}}`

---

## GET /health

Health check — verifies all dependency services.

### Response

```json
{
  "status": "ok",
  "version": "0.1.0",
  "qdrant_connected": true,
  "mysql_connected": true,
  "llm_available": true
}
```

**Note:** `/health` is exempt from rate limiting and query length guards.

```bash
curl http://localhost:18100/health
```

---

## GET /metrics

Prometheus scrape endpoint — exposes all application metrics in the standard exposition format.

### Response

**Content-Type:** `text/plain; version=0.0.4; charset=utf-8`

Returns [Prometheus exposition format](https://prometheus.io/docs/instrumenting/exposition_formats/) text.

### Exposed Metrics

| Metric | Type | Labels | Description |
|--------|------|--------|-------------|
| `chat_stream_total` | Counter | `intent_type`, `status` | Total chat stream requests |
| `chat_stream_errors_total` | Counter | `error_code` | Total errors by error code |
| `stage1_duration_seconds` | Histogram | `intent_type` | Stage 1 (semantic KG) latency |
| `stage2_sql_duration_seconds` | Histogram | — | Stage 2 (SQL gen + exec) latency |
| `chat_stream_duration_seconds` | Histogram | — | End-to-end chat stream latency |
| `active_chat_streams` | Gauge | — | Currently active concurrent streams |

**Note:** `/metrics` is exempt from rate limiting and query length guards.

```bash
curl http://localhost:18100/metrics
```

---

## POST /api/v1/sync/rules

Incremental sync of judgment rules from .NET backend to Qdrant.

### Request

```json
{
  "action": "upsert",
  "data": [
    {
      "id": "rule-001",
      "product_spec_code": "50W470",
      "name": "合格",
      "quality_status": "qualified",
      "condition": "F_PERF_PS_LOSS <= 1.08",
      "description": "..."
    }
  ]
}
```

| Field | Type | Description |
|-------|------|-------------|
| `action` | `string` | `"upsert"` or `"delete"` |
| `data` | `object[]` | Rule records to sync |

### Response

```json
{"status": "ok", "upserted": 5}
```

---

## POST /api/v1/sync/specs

Incremental sync of product specifications from .NET backend to Qdrant.

### Request

```json
{
  "action": "upsert",
  "data": [
    {
      "id": "spec-001",
      "code": "50W470",
      "name": "50W470 硅钢片",
      "detection_columns": "8",
      "description": "...",
      "attributes": [
        {"name": "铁损", "value": "1.08", "unit": "W/kg"}
      ]
    }
  ]
}
```

### Response

```json
{"status": "ok", "upserted": 3}
```

---

## Key Concepts

### IntentType

The system classifies queries into one of these intents:
- `statistical` — Aggregate metrics (pass rate, yield, averages)
- `trend` — Time-series analysis of metrics
- `by_shift` — Shift-based (早班/中班/晚班) comparisons
- `root_cause` — Diagnostic analysis for anomalies
- `conceptual` — Knowledge/explanation queries
- `out_of_scope` — Queries outside laboratory domain

### Reasoning Steps

Stage 1 produces a canonical `reasoning_steps` list that is mutated in place:
1. KG retrieval populates `spec`, `rule`, and `metric` steps
2. Condition back-fill enriches `condition` steps with `actual` values from diagnostic SQL
3. Grade determination adds `grade` steps
4. Frontend renders these as a reasoning chain visualization

### SSE Event Ordering

Events follow a strict sequence:
1. `reasoning_step` events (0-N) — Stage 1 KG retrieval + condition back-fill
2. `text` events (0-N) — Stage 2 streaming LLM answer
3. `response_metadata` (0-1) — Final SQL + metadata
4. `done` (1) — Stream end
5. `error` (0-1) — Replaces normal flow on failure

No new event types will be added without frontend coordination.

### SQL Safety

All generated SQL is validated through `validate_sql()` which blocks: `INSERT`, `UPDATE`, `DELETE`, `DROP`, `CREATE`, `ALTER`, `TRUNCATE`, `GRANT`, `EXEC`, `CALL`, `SET`, `LOAD`, `INTO OUTFILE`. Results are capped at 500 rows (`sql_max_rows`).

### Request Guards

Three middleware layers protect the service (outermost first):
1. **RateLimitInMem** — 30 req/min/IP (token bucket, in-memory)
2. **QueryLengthGuard** — last message ≤ 2000 chars
3. **RequestSizeLimit** — body ≤ 100 KB

`/health`, `/docs`, `/openapi.json` are exempt from guards 1 and 2.
