<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Filters

## Purpose
全局 MVC 异常过滤器。捕获请求管道中所有未处理异常，分流到统一返回结果（`UnifyContext`）、Razor Pages 错误页（`BadPageResult`）或原生 `JsonResult`，并按需写入日志、MiniProfiler。

## Key Files
| File | Description |
|------|-------------|
| `FriendlyExceptionFilter.cs` | 实现 `IAsyncExceptionFilter`。流程：①判断是否 `AppFriendlyException.ValidationException`；②非校验异常调用用户 `IGlobalExceptionHandler.OnExceptionAsync`；③跳过 WebSocket；④读取 `UnifyContext.GetExceptionMetadata`；⑤分支：Razor Pages → `BadPageResult`，WebAPI → 跳过规范化时返回 `JsonResult`，否则交给 `UnifyResult.OnException`；⑥按 `FriendlyExceptionSettings.LogError` 写错误日志；⑦`PrintToMiniProfiler` 输出堆栈位置。 |

## For AI Agents

### Working in this directory
- 通过 `AddFriendlyException(opt => opt.GlobalEnabled = true)` 由 `AddMvcFilter<FriendlyExceptionFilter>` 注册 —— 不要手动 `services.AddSingleton`。
- `context.ExceptionHandled = true` 后续过滤器不再处理 —— 校验异常分支会主动设置。
- 自定义异常**额外操作**（持久化、链路追踪）请实现 `IGlobalExceptionHandler`，不要修改本类。

### Common patterns
- WebSocket 请求 (`IsWebSocketRequest`) 与 Response 已开始 (`Response.HasStarted`) 都会跳过响应写入，避免破坏协议。
- MiniProfiler 仅在 `App.Settings.InjectMiniProfiler == true` 时启用。

## Dependencies
### Internal
- `IGlobalExceptionHandler`、`UnifyContext`、`Penetrates.IsApiController`、`DataValidationFilter`、`ValidatorContext`、`BadPageResult`、`FriendlyExceptionSettingsOptions`、`AppFriendlyException`。
### External
- `Microsoft.AspNetCore.Mvc.Filters.IAsyncExceptionFilter`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
