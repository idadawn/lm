<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# maintenance

## Purpose
预警规则维护 — list and edit warning rules. Reuses the shared form from `views/extend/tableDemo/commonForm` rather than owning its own.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Rule list with create/edit (`projectPhase` shown as 是/否 tags). |

## For AI Agents

### Working in this directory
- The form is imported from `/@/views/extend/tableDemo/commonForm/index.vue` — do not duplicate it locally.
- API: `getTableList`/`delTable` from `/@/api/extend/table` — same backend as `extend/tableDemo`.
- `defineOptions({ name: 'extend-tableDemo-commonTable' })` reflects the shared origin; do not rename without updating cached route keys.
