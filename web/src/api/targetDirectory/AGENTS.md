<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# targetDirectory

## Purpose
指标分类 / 目录 (metric category) CRUD — drives the left tree on the indicator management page. "Category == directory" semantically.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `postMetriccategory`, `getMetriccategoryList`, `getMetriccategory`, `putMetriccategory`, `deleteMetriccategory` plus tree/move helpers. Base `/api/kpi/v1/metriccategory`. |

## For AI Agents

### Working in this directory
- `putMetriccategory` deliberately picks `{ fullName, description, ownId, parentId }` — backend ignores other fields. Preserve that filtering.
- Pair with `targetDefinition/` for the tree-of-metrics page.
- Move/reorder uses `parentId` change rather than dedicated endpoint.

### Common patterns
- POST for create + list, GET for single, PUT for update, DELETE for remove.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
