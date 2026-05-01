<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
常用字段 (frequently-used fields) management drawer used by the data-model form to pick reusable field definitions across models.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Drawer hosting the 常用字段 list with add/edit/delete actions. |
| `Form.vue` | Edit form for a single 常用字段 entry. |

## For AI Agents

### Working in this directory
- Drawer width and titles follow the `full-drawer` class used elsewhere in `systemData/`.
- Communicate selection back via `emit('select', record)`; do not mutate parent state directly.
