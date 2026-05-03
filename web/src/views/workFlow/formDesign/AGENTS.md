<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# formDesign

## Purpose
工作流表单设计 — form-template management for flow forms. Supports drag-drop designer, preview (PC), import (`.fff`), copy, export and release.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Form-template list with 已发布/未发布 tag and CRUD/import actions. |
| `Form.vue` | The form-designer popup. |
| `FieldForm.vue` | Field-level config form. |
| `AddModal.vue` | Picker shown when creating a new form template. |
| `Preview.vue` | Form preview popup. |

## For AI Agents

### Working in this directory
- Two preview registers exist: `registerPreview` (modal-style) and `registerPreviewPopup` (full popup) — keep both; they handle different preview entry points.
- `release` flips `enabledMark` server-side; refresh the table after success.

## Dependencies
### Internal
- `/@/api/workFlow/formDesign`, `/@/components/Popup`, `/@/components/Modal`
