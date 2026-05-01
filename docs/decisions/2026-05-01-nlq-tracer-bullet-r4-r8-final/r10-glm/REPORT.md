# R10 GLM Worker Report — nlq-agent bulk-resync endpoint

## bulk_resync_all function

**File:** `nlq-agent/src/services/resync_service.py`
**Signature:** `async def bulk_resync_all() -> dict[str, int]`
**Return schema:**
```json
{"rules": 12, "specs": 5, "duration_ms": 432}
```
- `rules`: count of judgment rule documents upserted
- `specs`: count of product spec documents upserted
- `duration_ms`: wall-clock time in milliseconds

The function manages its own service lifecycle (EmbeddingClient, QdrantService, DatabaseService) with proper cleanup in `finally`.

## Endpoint

**Path:** `POST /api/v1/sync/resync-now`
**Status:** `202 Accepted`
**Auth:** `Authorization: Bearer <SYNC_ADMIN_TOKEN>`

### 401 conditions
1. Authorization header missing
2. Token doesn't match `settings.sync_admin_token`
3. `settings.sync_admin_token` is empty string (rejects even if request sends a token)

### Success response
```json
{"status": "ok", "rules": 12, "specs": 5, "duration_ms": 432}
```

## Files changed

| File | Action | LOC |
|------|--------|-----|
| `nlq-agent/src/services/resync_service.py` | NEW | ~230 |
| `nlq-agent/scripts/init_semantic_layer.py` | REWRITTEN | ~30 (was ~335) |
| `nlq-agent/src/api/routes.py` | MODIFIED | +20 |
| `nlq-agent/src/core/settings.py` | MODIFIED | +3 |
| `nlq-agent/.env.example` | MODIFIED | +3 |
| `nlq-agent/.env.production.example` | MODIFIED | +4 |
| `nlq-agent/tests/unit/test_resync_endpoint.py` | NEW | ~75 |
| `nlq-agent/tests/unit/test_bulk_resync.py` | NEW | ~100 |
| `nlq-agent/API.md` | MODIFIED | +30 |
| `nlq-agent/PRODUCTION_CHECKLIST.md` | MODIFIED | +8 |

**Total LOC changed:** ~510

## Tests

- `test_resync_endpoint.py`: 4 cases (no token → 401, wrong token → 401, correct token → 202, empty config token → 401)
- `test_bulk_resync.py`: 2 cases (normal data → correct schema, empty tables → zero counts)

**Suite results:**
- Default tests: **231 passed** (225 existing + 6 new)
- Load tests: **4 passed**

## Commits

```
f5b99a6 feat(nlq-agent): POST /api/v1/sync/resync-now admin endpoint with bearer token
1c54b5c refactor(nlq-agent): extract bulk_resync_all from init_semantic_layer.py
```

## CLI behavior preserved

`python -m scripts.init_semantic_layer` still works identically — imports `bulk_resync_all` from the new service module and runs it with logging output.
