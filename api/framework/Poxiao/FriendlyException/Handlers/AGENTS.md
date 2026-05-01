<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Handlers

## Purpose
用户自定义全局异常处理钩子。允许业务在 `FriendlyExceptionFilter` 拦截异常时挂入额外动作（持久化错误日志、发送告警、上报链路系统等），而不修改框架默认响应行为。

## Key Files
| File | Description |
|------|-------------|
| `IGlobalExceptionHandler.cs` | 单方法：`Task OnExceptionAsync(ExceptionContext context)`。在过滤器内通过 `RequestServices.GetService<IGlobalExceptionHandler>()` 解析，**仅当 `AppFriendlyException.ValidationException == false`** 时才调用（业务校验错不会走此处）。 |

## For AI Agents

### Working in this directory
- 业务实现：`services.AddSingleton<IGlobalExceptionHandler, MyHandler>()`（或 Scoped；解析方使用 `RequestServices`）。
- 钩子在响应写入**之前**调用，可读取 `context.HttpContext` 与 `context.Exception`，但请勿设置 `context.Result` —— 那是过滤器后续的职责。
- 如果在钩子内抛异常，会冒泡到 ASP.NET Core 默认管道并替换原异常 —— 务必 try/catch 包住。

### Common patterns
- 与 Serilog/ELK/Sentry 等集成的常见接入点。

## Dependencies
### Internal
- 被 `FriendlyExceptionFilter` 调用。
### External
- `Microsoft.AspNetCore.Mvc.Filters.ExceptionContext`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
