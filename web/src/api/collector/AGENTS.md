<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# collector

## Purpose
Data collector (数据采集器) management endpoints — wraps the standalone `/collectServer/collector/*` micro-service. Covers type dropdown, config templates, paginated list, CRUD, and start/stop controls used by the data-collection module.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `collectorDropDown`, `collectorConfigTemplate`, `collectorPage`, `collectorAdd`, `collectorUpdate`, `collectorRemove`, run-state controls, dictionary lookups. Base path constant: `const server = '/collectServer'`. |

## For AI Agents

### Working in this directory
- The base path `/collectServer` is proxied (see `vite.config.ts` proxy table) — do not bake a host into URLs here.
- Removal endpoint sends id in body rather than URL; preserve that for backend compatibility.
- Pair with `service/` (`/collectServer/service/*`) which has parallel structure for service-side endpoints.

### Common patterns
- Verb-prefixed function names (`collectorAdd`, `collectorRemove`) keep call sites self-describing.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
