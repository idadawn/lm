<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Http

## Purpose
Built-in `IJob` that fires HTTP requests on a schedule — used when a user creates a "HTTP" type task in the front-end task scheduler. Generates a fresh JWT for the configured user/tenant before each call so the target endpoint sees an authenticated request.

## Key Files
| File | Description |
|------|-------------|
| `PoxiaoHttpJob.cs` | `IJob` implementation. Reads serialised `PoxiaoHttpJobMessage` from `JobDetail.GetProperty<string>(nameof(PoxiaoHttpJob))`, switches `_sqlSugarClient` to the tenant DB, fetches the user via `UserEntity` query, mints a token via `NetHelper.GetToken(...)`, sends an `HttpRequestMessage` (Authorization + UA), and stores `{ StatusCode, Body }` JSON into `context.Result`. |
| `PoxiaoHttpJobMessage.cs` | Plain DTO: `RequestUri`, `HttpMethod` (default GET), `Body`, `TaskId`, `UserId`, `TenantId`. |

## For AI Agents

### Working in this directory
- Default UA string is hard-coded to mimic Edge 104. If you need a configurable UA, route through `MessageOptions` rather than editing this constant.
- The job only writes `application/json` bodies; for other content types, extend the `if (httpJobMessage.HttpMethod != Get/Head ...)` branch.
- Result format is a stable `{ StatusCode, Body }` JSON — `OnTriggerChanged` stores `context.Trigger.Result.ToJsonString()` into `TimeTaskLogEntity.Description`. Don't change the shape without updating the log viewer.

### Common patterns
- Tenant routing reuses the same shape as `LogEventSubscriber` (see `../EventBus/AGENTS.md`).
- `Penetrates.Serialize` / `Deserialize` (uses `DateTimeJsonConverter`) is mandatory for round-trip safety with `JobDetail` properties.

## Dependencies
### Internal
- `Poxiao.Schedule.IJob`, `Poxiao.Infrastructure.Configuration.KeyVariable`, `Poxiao.Infrastructure.Manager.ICacheManager`, `Poxiao.Infrastructure.Net.NetHelper`, `Poxiao.Systems.Entitys.Permission.UserEntity`.

### External
- `System.Net.Http`, `Microsoft.Extensions.Logging`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
