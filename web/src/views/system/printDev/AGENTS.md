<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# printDev

## Purpose
打印模板开发 — manages print templates (`.bp` import/export, copy, preview) for laboratory reports and forms.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Template list with create/import (`.bp`) actions. |
| `Form.vue` | Template designer popup. |
| `Preview.vue` | Live preview of rendered template. |
| `Log.vue` | Print log viewer. |

## For AI Agents

### Working in this directory
- Import endpoint is fixed: `/api/system/printDev/Actions/ImportData` — do not change without backend coordination.
- Preview/Form are heavy; mounted via `usePopup` so they tear down on close.

## Dependencies
### Internal
- `/@/api/system/printDev`, `/@/components/Modal`, `/@/components/Table`
