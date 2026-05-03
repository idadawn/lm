<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# labelManagement

## Purpose
KPI 标签 (metric tag) CRUD — used by indicator tagging UI to organize/filter metrics by user-defined tags.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `postMetrictag`, `postMetrictagList`, `getMetrictag`, `putMetrictag`, `deleteMetrictag`. Base path `/api/kpi/v1/metrictag`. |

## For AI Agents

### Working in this directory
- `putMetrictag` only sends `{ name, description }` — backend ignores other fields here. Keep that mapping intentional.
- Pure CRUD; pagination/list filtering is server-side via the POST list endpoint.

### Common patterns
- POST for create + list, GET for single, PUT for update, DELETE for remove.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
