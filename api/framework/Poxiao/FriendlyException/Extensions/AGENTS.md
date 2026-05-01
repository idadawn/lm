<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
友好异常模块的服务注册与异常对象 fluent 拓展。提供 `IMvcBuilder.AddFriendlyException` / `IServiceCollection.AddFriendlyException` 入口，及 `AppFriendlyException.StatusCode(...)` / `WithData(...)` 链式 API。

## Key Files
| File | Description |
|------|-------------|
| `FriendlyExceptionServiceCollectionExtensions.cs` | 4 个 `AddFriendlyException` 重载（带/不带 `IErrorCodeTypeProvider` 泛型；MVC vs DI）。注册 `FriendlyExceptionSettingsOptions` / `ErrorCodeMessageSettingsOptions` 配置选项；按 `FriendlyExceptionOptions.GlobalEnabled`（默认 true）通过 `services.AddMvcFilter<FriendlyExceptionFilter>()` 装配全局过滤器。 |
| `AppFriendlyExceptionExtensions.cs` | `StatusCode(this AppFriendlyException, int)` 与 `WithData(...)`，链式修改异常对象。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Options/` | `FriendlyExceptionOptions` 服务注册选项 (see `Options/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 推荐入口：`mvcBuilder.AddFriendlyException<MyErrorCodeProvider>()` —— 同时挂载错误码提供器与过滤器。
- `IErrorCodeTypeProvider` 注册为单例；提供器返回的枚举类型与代码中 `[ErrorCodeType]` 枚举求并集。
- 想关闭全局过滤器（自行接管异常）：`services.AddFriendlyException(opt => opt.GlobalEnabled = false)`。

### Common patterns
- 命名空间 `Microsoft.Extensions.DependencyInjection` —— 沿用 BCL DI 拓展惯例，在 `Program.cs` 中 `using` 一次即可。

## Dependencies
### Internal
- `FriendlyExceptionFilter`、`FriendlyExceptionSettingsOptions`、`ErrorCodeMessageSettingsOptions`、`IErrorCodeTypeProvider`、`AddMvcFilter`（`Poxiao.Filter`）、`AddConfigurableOptions`（`Poxiao.ConfigurableOptions`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
