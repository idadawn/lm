<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FriendlyException

## Purpose
Poxiao 友好异常模块（基于 Furion `Oops` 模型）。统一业务异常抛出 (`Oops.Bah` / `Oops.Oh`)、错误码字典管理（枚举元数据 + 配置文件）、全局 MVC 异常过滤、错误码本地化、可重试调用 (`Retry`) 与自定义错误页输出。

## Key Files
| File | Description |
|------|-------------|
| `Oops.cs` | 抛异常入口（静态）。`Bah` = 业务校验异常 (`StatusCode 400` + `ValidationException = true`)；`Oh` = 普通友好异常。组合错误码字典、堆栈中 `[IfException]` 特性、配置 `FriendlyExceptionSettings.ThrowBah/HideErrorCode`、`L.Text` 本地化与 `string.Format`。 |
| `Retry.cs` | 通用重试工具。`Invoke`/`InvokeAsync` 支持 `numRetries`、`retryTimeout`、`finalThrow`、`exceptionTypes` 过滤、`fallbackPolicy`、`retryAction`。EventBus 与全局异常处理共享。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Assets/` | 嵌入式 HTML 错误页模板 (see `Assets/AGENTS.md`) |
| `Attributes/` | 错误码与重写异常特性 (see `Attributes/AGENTS.md`) |
| `Exceptions/` | `AppFriendlyException` 类型 (see `Exceptions/AGENTS.md`) |
| `Extensions/` | DI 注册与 fluent 拓展 (see `Extensions/AGENTS.md`) |
| `Filters/` | MVC 全局异常过滤器 (see `Filters/AGENTS.md`) |
| `Handlers/` | 用户自定义异常处理钩子 (see `Handlers/AGENTS.md`) |
| `Internal/` | 内部数据结构 (see `Internal/AGENTS.md`) |
| `Options/` | `appsettings.json` 配置选项 (see `Options/AGENTS.md`) |
| `Providers/` | 错误码类型提供器 (see `Providers/AGENTS.md`) |
| `Results/` | `BadPageResult` 错误页 (see `Results/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 抛业务校验错（默认 400）：`throw Oops.Bah("消息或错误码")`；抛系统/业务异常：`throw Oops.Oh(errorCode, args)`。
- 错误码源有两路：枚举上贴 `[ErrorCodeType]` + 字段 `[ErrorCodeItemMetadata("消息", args)]`，或 `appsettings.json` 中 `ErrorCodeMessageSettings.Definitions`；后者覆盖前者。
- 出错方法堆栈定位优先匹配 `ControllerBase` / `IDynamicApiController`，否则取首个非 `Poxiao.FriendlyException` 命名空间帧 —— 保持栈帧可见、勿过度内联。

### Common patterns
- `Oops` 通过 `App.GetConfig<FriendlyExceptionSettingsOptions>` 拉取启动配置（隐藏错误码、默认消息、ThrowBah、LogError）。
- `[IfException]` 可在方法/类上覆盖错误码消息，并支持仅按异常类型挂载。

## Dependencies
### Internal
- `Poxiao.Localization.L`、`Poxiao.Templates.Render`、`Poxiao.DynamicApiController`、`Poxiao.UnifyResult`、`Poxiao.DataValidation`。
### External
- `Microsoft.AspNetCore.Mvc.Filters` / `EnhancedStackTrace`（`Ben.Demystifier`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
