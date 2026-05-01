<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# task

## Purpose
定时任务 (scheduled task) management — CRUD for cron-style tasks plus per-task execution log viewer.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Task list with status tag and 新建/edit/log actions. |
| `Form.vue` | Task editor (cron expression, handler, params). |
| `Log.vue` | Execution log popup. |

## For AI Agents

### Working in this directory
- API: `/@/api/system/task` (`getTaskList`, `delTask`).
- `Form` and `Log` registered via `usePopup` — keep popup naming consistent (`registerForm`, `registerLog`).
