# GLM Worker Report — Qdrant 1.10+ API Upgrade

**Branch:** `omc-team/r3/glm-qdrant-1.10-upgrade`
**Base:** `3a4e602` (main)
**Status:** All 3 changes complete, 50/50 tests green.

---

## 改动 #1 — search → query_points fix

**Status:** DONE

`nlq-agent/src/services/qdrant_service.py:151` — replaced `self._client.search(query_vector=...)` with `self._client.query_points(query=...)`. Response iterates `response.points` instead of raw list.

Equivalent to the lead's fix on main, applied independently in this worktree.

## 改动 #2 — Full qdrant-client API audit

**Status:** DONE — zero additional deprecated calls found.

Scanned all `.py` files under `nlq-agent/src/` and `nlq-agent/scripts/` for `self._client.*` calls:

| Method | Location | 1.10+ Status |
|---|---|---|
| `collection_exists` | qdrant_service.py:57 | Stable |
| `create_collection` | qdrant_service.py:59 | Stable |
| `upsert` | qdrant_service.py:107 | Stable |
| ~~`search`~~ | ~~qdrant_service.py:151~~ | **Removed → fixed** |
| `get_collections` | qdrant_service.py:205 | Stable |
| `close` | qdrant_service.py:212 | Stable |

**Total: 1 deprecated call** (search), now fixed. No `search_groups`, `recommend`, or `count` usage found anywhere in the codebase. SKILL.md files reference synchronous `QdrantClient` in documentation examples only — not runtime code.

## 改动 #3 — e2e regression test

**Status:** DONE — hybrid smoke + live approach.

- **Lane 1 (always runs):** `TestQdrantMethodSmoke` uses `hasattr(AsyncQdrantClient, ...)` to assert `query_points`/`upsert`/`create_collection`/`delete_collection`/`collection_exists`/`get_collections`/`close` exist, and `search`/`search_groups`/`recommend` are removed. No Qdrant server needed. Runs on every CI.
- **Lane 2 (`@pytest.mark.live_qdrant`):** Round-trip create→upsert→`query_points`→delete with 3 mock docs. Auto-skips if Qdrant unreachable. CI skips by default.
- **Rationale for hybrid:** Pure smoke catches the most common regression (downgrade or API misuse) at zero infra cost. Live test validates full round-trip when Qdrant is available (staging/manual).

Marker `live_qdrant` registered in `pyproject.toml` alongside existing `live_llm`.

## Test Results

```
50 passed, 2 deselected (live markers) in 0.68s
```

Verified with qdrant-client **1.17.1**.

## Commits

```
2ac49e1 test(nlq-agent): qdrant-client 1.10+ API compatibility regression
63e5472 fix(nlq-agent): use query_points instead of removed AsyncQdrantClient.search
```
