<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SysLog

## Purpose
DTOs for the 系统日志 admin pages — surfaces login logs, operation logs, request logs and exception logs persisted by `Poxiao.Systems` log services. Used by the log query/list/delete endpoints.

## Key Files
| File | Description |
|------|-------------|
| `LogListQuery.cs` | Common log list filter (keyword / time range / type). |
| `LogLoginOutput.cs` | 登录日志 row — `creatorTime`、`userName`、`ipaddress`、`platForm`. |
| `LogOperationOutput.cs` | 操作日志 row — adds `moduleName`、`requestMethod`、`requestDuration`、operation `json`. |
| `LogRequestOutput.cs` | HTTP 请求日志 row. |
| `LogExceptionOutput.cs` | 异常日志 row with stack/message. |
| `LogDelInput.cs` | Bulk delete-by-time-range input. |

## For AI Agents

### Working in this directory
- Output DTOs are read-only projections from `base_sys_log_*` tables — keep field naming aligned with the Log entities under `Entity/`.
- `creatorTime` is the canonical timestamp field (matches `F_CREATORTIME` audit column on the entity side).
- Add new log categories by creating a new `Log<X>Output.cs` rather than overloading existing classes.
- `[SuppressSniffer]` on every class; camelCase props.

### Common patterns
- One DTO per log category, sharing `id` + `creatorTime` + `userName` + `ipaddress` + `platForm` baseline.
- `requestDuration` is `int` milliseconds; `json` is the raw body snapshot.

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- Pairs with the `LogLogin` / `LogOperation` / `LogException` / `LogRequest` services in the system module.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
