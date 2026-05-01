<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
字典分类 (dictionary type) management — drawer for CRUD over the type tree shown on the dictionary page.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Drawer-hosted type list with add/edit/delete. |
| `Form.vue` | Type edit form (code, name, parent). |

## For AI Agents

### Working in this directory
- Emit `visible-change` so the parent `index.vue` can refresh its left tree after edits.
