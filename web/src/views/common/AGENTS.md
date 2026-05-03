<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# common

## Purpose
Reusable runtime hosts referenced from menu items via `route.meta.relationId`. Each subdir is a generic page that pulls its real configuration from the in-app low-code platform (`onlineDev`/`systemData`) at runtime.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `dynamicDataReport/` | Iframe host for the report engine (`/preview.html?id=...&token=...`) (see `dynamicDataReport/AGENTS.md`) |
| `dynamicDictionary/` | Per-type 字典 data CRUD bound by `meta.relationId` → `typeId` (see `dynamicDictionary/AGENTS.md`) |
| `dynamicModel/` | Visual-dev model runtime — switches between Form (single-record) and List views (see `dynamicModel/AGENTS.md`) |
| `dynamicPortal/` | Visual portal runtime host |
| `externalLink/` | External link redirect / iframe page |
| `formShortLink/` | Short-link standalone form (anonymous public submissions) |

## For AI Agents

### Working in this directory
- These pages read their identity from `useRoute().meta.relationId` (a UUID). They are mounted multiple times under different menu paths — KeepAlive identity must be unique.
- Don't add domain logic here — the goal is to host any model defined via the visual dev tools. Domain logic lives under feature-specific dirs (`lab`, `system`, `kpi`, …).
- Backend equivalents are under `api/src/modularity/onlineDev` and `api/src/modularity/systemData`.

### Common patterns
- `defineOptions({ name: 'dynamicXxx' })` to keep the route name predictable.
- Async config load → branch on `webType`, `linkType`, or similar before mounting the actual sub-component (`Form.vue` / `List/index.vue`).

## Dependencies
### Internal
- `/@/api/onlineDev/{visualDev,portal}`, `/@/api/systemData/dictionary`, `/@/hooks/setting`, `/@/utils/auth`
### External
- `ant-design-vue`, `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
