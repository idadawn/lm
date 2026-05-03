<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# flowEngine

## Purpose
流程引擎 — flow-template management. Create/edit/release/stop flow templates, import (`.ffe`), copy/export, and manage versions.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Template list with 启用/禁用 tag, 发布/停止/导入 actions. |
| `Form.vue` | Flow designer popup (canvas + node config). |
| `AddModal.vue` | Picker for choosing a starting template/category. |
| `AssistModal.vue` | 协助 / collaborator config modal. |
| `VersionManage.vue` | Version history viewer. |

## For AI Agents

### Working in this directory
- `release`/`stop` are critical state transitions — confirm via `useMessage` modal before calling.
- Imports must be `.ffe` files; the API path `/api/workflow/Engine/flowTemplate/Actions/ImportData` is fixed.

## Dependencies
### Internal
- `/@/api/workFlow/flowEngine`
