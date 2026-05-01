<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Filter

## Purpose
Global MVC filters that publish events to the EventBus for every request and exception, so the log/audit trail is captured uniformly without per-controller code. Both honour the `[IgnoreLog]` attribute and stop after publishing — actual persistence happens in `LogEventSubscriber`.

## Key Files
| File | Description |
|------|-------------|
| `RequestActionFilter.cs` | `IAsyncActionFilter`. Times each request with `Stopwatch`, then publishes `LogEventSource("Log:CreateReLog", …)` (Category=5). If the action also has `[OperateLog]`, additionally publishes `Log:CreateOpLog` (Category=3) carrying serialised args + result + module name. |
| `LogExceptionHandler.cs` | `IGlobalExceptionHandler, ISingleton`. On exception, publishes `LogEventSource("Log:CreateExLog", …)` (Category=4) with `Message + StackTrace + TargetSite.Parameters`. |

## For AI Agents

### Working in this directory
- Both filters resolve `tenantId`/`userId`/`userName` via `App.User?.FindFirstValue(ClaimConst.CLAINMUSERID/REALNAME/TENANTID)`. Any new filter that needs the user context should follow the same pattern (do NOT inject `IUserManager` here — it will fail outside an authenticated request).
- Skip logic uses `context.ActionDescriptor.EndpointMetadata.Any(m => m.GetType() == typeof(IgnoreLogAttribute))`. Honour the same attribute when extending.
- IP comes from `Poxiao.Infrastructure.Net.NetHelper.Ip` (cached static accessor) — keep using it instead of parsing `httpContext.Connection.RemoteIpAddress`.

### Common patterns
- All persistence is fire-and-forget via `IEventPublisher.PublishAsync`. Don't `await` SqlSugar calls inside these filters.

## Dependencies
### Internal
- `Poxiao.EventBus.IEventPublisher`, `Poxiao.Logging.Attributes` (`IgnoreLogAttribute`, `OperateLogAttribute`), `Poxiao.Infrastructure.Net.UserAgent` + `NetHelper`, `Poxiao.Systems.Entitys.System.SysLogEntity`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
