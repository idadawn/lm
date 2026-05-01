<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# connectForm

## Purpose
数据连接 modal — links an authorization (column or form) to an actual DB table/field. Reused by `columnAuthorize` and `formAuthorize`.

## Key Files
| File | Description |
|------|-------------|
| `Form.vue` | Top-level modal that hosts the `TableModal` field picker via slot. |
| `TableModal.vue` | Table picker scoped to a `dbLinkId`. |
| `FieldModal.vue` | Field/column picker for a chosen table. |

## For AI Agents

### Working in this directory
- This is a shared utility — keep it parameterized by `dbLinkId`, `dataType`, `moduleId`. Do not import from `columnAuthorize` or `formAuthorize` (the dependency direction is inward only).
- Submission calls `update` from `/@/api/system/authorize`.
