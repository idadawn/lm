<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# log

## Purpose
系统日志 — tabbed viewer for 登录/请求/操作/异常 logs. Each tab is a `BasicTable` over a separate API endpoint with shared search filters and bulk-delete.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tabs (key 1=登录, 5=请求, 3=操作, 4=异常) sharing `getSearchInfo` and per-tab columns. |
| `Form.vue` | Detail viewer for a single log entry (e.g. operation JSON payload). |

## For AI Agents

### Working in this directory
- Each tab uses its own `useTable` registration — do not collapse them; columns differ per log type.
- 一键清空 / 删除 are destructive; preserve confirmation flow already present in handlers.
- Shared search bar emits to all tab tables via `getSearchInfo` computed — keep that wiring intact.

## Dependencies
### Internal
- `/@/api/system/log/*`, `/@/components/Table`, `/@/components/Form`
