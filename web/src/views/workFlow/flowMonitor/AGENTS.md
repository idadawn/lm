<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# flowMonitor

## Purpose
流程监控 — admin/superuser view of all running and completed flows with bulk delete and version-tag display.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Admin list with `flowVersion` tag, status, completion, and 删除 (bulk). |

## For AI Agents

### Working in this directory
- Admin-only page; assume permission check happens at the route level. Don't hide UI based on userStore here.
- Bulk delete must collect selected row keys via `useTable` selection — preserve the delete confirmation flow.
