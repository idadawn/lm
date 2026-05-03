<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# backend

## Purpose
Schedule 看板的服务端实现。`UseScheduleUI` 中间件从程序集嵌入资源吐出前端 SPA，并在 `/<RequestPath>/api/*` 之下提供作业/触发器的查询、控制与 SSE 推送。

## Key Files
| File | Description |
|------|-------------|
| `ScheduleUIExtensions.cs` | `IApplicationBuilder.UseScheduleUI(...)`：注册中间件 + `EmbeddedFileProvider`，识别 `DisableOnProduction`，校验 RequestPath 形式。 |
| `ScheduleUIMiddleware.cs` | 处理 `/get-jobs`、`/operate-job`（start/pause/remove/run）、`/operate-trigger`（含 timelines）、`/check-change` 长连接 SSE，注入 `Access-Control-Allow-*` 跨域头。 |
| `ScheduleUIOptions.cs` | 看板配置：`RequestPath`（默认 `/schedule`）、`EnableDirectoryBrowsing`、`DisableOnProduction`、`VirtualPath`。 |

## For AI Agents

### Working in this directory
- 仅注入 `ISchedulerFactory`；如果未注册 Schedule 服务，`UseScheduleUI` 会静默跳过，不要在中间件中再做 DI 检查。
- `RequestPath` 必须以 `/` 开头且不以 `/` 结尾，否则中间件不会启用——保持该校验。
- 新增 API action 时使用小写匹配（`action?.ToLower()`），与现有 switch 风格一致。

### Common patterns
- JSON 输出走 `Penetrates.GetDefaultJsonSerializerOptions()` 并强制 `CamelCase`，与前端契合。
- SSE 通道使用 `BlockingCollection<JobDetail>`，订阅 `_schedulerFactory.OnChanged`，通过 `data: ...\n\n` 推送。

## Dependencies
### Internal
- `../../Internal/Penetrates`、`../../Factories/ISchedulerFactory`、`../../Triggers/TriggerTimeline`。
### External
- `Microsoft.AspNetCore.Http`、`Microsoft.Extensions.FileProviders`、`System.Text.Json`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
