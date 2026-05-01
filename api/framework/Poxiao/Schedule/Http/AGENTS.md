<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Http

## Purpose
内置 HTTP 作业实现。`HttpJob` 是一个 `IJob`，按 `JobDetail.Properties` 中存储的 `HttpJobMessage` 配置发起 HTTP 请求并把响应写回 `JobExecutingContext.Result`，常被 `ISchedulerFactory.AddHttpJob(...)` 调用。

## Key Files
| File | Description |
|------|-------------|
| `HttpJob.cs` | 注入 `IHttpClientFactory` + `IScheduleLogger`：从 `JobDetail.GetProperty<string>(nameof(HttpJob))` 反序列化 `HttpJobMessage`，发请求、按 `EnsureSuccessStatusCode` 校验，最后把 `{StatusCode, Body}` 序列化到 `context.Result`。 |
| `HttpJobMessage.cs` | 请求消息：`RequestUri`、`HttpMethod`（默认 GET）、`Body`、`ClientName`（默认 `nameof(HttpJob)`）、`EnsureSuccessStatusCode`（默认 true）。 |

## For AI Agents

### Working in this directory
- 默认请求体只支持 `application/json`；如需 form/multipart，新增字段并在 `HttpJob.ExecuteAsync` 中分支处理，不要在调用方改 `HttpJob`。
- `HttpClientFactory` 通过 `httpJobMessage.ClientName` 定位命名客户端，宿主可用 `services.AddHttpClient(nameof(HttpJob), ...)` 自定义超时/Handler。
- `User-Agent` 已硬编码为 Edge UA；如需调整，确保不要破坏被监控目标的 UA 白名单。

### Common patterns
- 通过 `services.AddHttpClient()` 与 `services.AddSchedule(b => b.AddHttpJob(msg => { msg.RequestUri = "..."; }, Triggers.PeriodMinutes(5)))` 注册。

## Dependencies
### Internal
- `../Internal/Penetrates`（序列化/反序列化）、`../Loggers/IScheduleLogger`、`../Dependencies/IJob`。
### External
- `System.Net.Http` (`IHttpClientFactory`)。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
