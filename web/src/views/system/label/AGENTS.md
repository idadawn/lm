<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# label

## Purpose
指标标签 (metric tag) management — name/description CRUD with department-scoped form and member assignment. Used to label metrics in lab analysis flows.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tag list page (`postMetrictagList`/`deleteMetrictag`). |
| `Form.vue` | Tag basics form. |
| `DepForm.vue` | Department-scoped tag form (active modal in `index.vue`). |
| `Member.vue` | Member assignment for a tag. |

## For AI Agents

### Working in this directory
- API base is `/@/api/labelManagement` — note this is **not** under `/@/api/system/`.
- Component is registered as `defineOptions({ name: 'label' })`; do not rename without updating routes.

## Dependencies
### Internal
- `/@/api/labelManagement`, `/@/components/Table`, `/@/components/Modal`
