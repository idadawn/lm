<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# service

## Purpose
Data-collection service (服务) management endpoints — sibling to `collector/`. Wraps `/collectServer/service/*` for CRUD on the server-side data services that consume collected data.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `serviceDropDown`, `serviceConfigTemplate`, `servicePage`, `serviceAdd`, `serviceUpdate`, `serviceRemove`, status/start/stop helpers. Base path `/collectServer/service`. |

## For AI Agents

### Working in this directory
- Mirrors the structure of `collector/index.ts` — keep the parallel API shape so the two pages can reuse a generic table component.
- `/collectServer` is the proxy alias for the dedicated micro-service; do not hard-code the upstream host.

### Common patterns
- Function-prefix `service*` mirrors collector's `collector*` (e.g. `serviceAdd` ↔ `collectorAdd`).

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
