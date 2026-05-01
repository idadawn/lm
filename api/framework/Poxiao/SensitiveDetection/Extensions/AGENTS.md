<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
脱敏词汇模块的 DI 注册入口。提供 `AddSensitiveDetection()` / `AddSensitiveDetection<TProvider>()` 的 `IMvcBuilder` 与 `IServiceCollection` 重载，用于注册 Provider 单例与 `SensitiveDetectionBinderProvider`。

## Key Files
| File | Description |
|------|-------------|
| `SensitiveDetectionServiceCollectionExtensions.cs` | 四个重载：`mvcBuilder.AddSensitiveDetection()`、`mvcBuilder.AddSensitiveDetection<TProvider>(handle)`、`services.AddSensitiveDetection()`（默认 `SensitiveDetectionProvider`）、`services.AddSensitiveDetection<TProvider>(handle)`（自定义 Provider + 后续配置回调）。 |

## For AI Agents

### Working in this directory
- Provider 注册为单例；自定义 Provider 实现请保证线程安全（默认实现内部维持词典快照）。
- `MvcOptions.ModelBinderProviders.Insert(0, new SensitiveDetectionBinderProvider())` 必须保留 `Insert(0)` —— 让脱敏绑定器优先于其他字符串绑定器，否则替换会失效。
- `handle` 委托允许调用方追加注册（如把字典持久化、缓存预热等）。

### Common patterns
- 默认 Provider 读取入口程序集目录下的 `sensitive-words.txt`；自定义 Provider 时通常会从数据库或 Redis 加载词库。

## Dependencies
### Internal
- `SensitiveDetectionProvider`（默认）、`SensitiveDetectionBinderProvider`。
### External
- `Microsoft.AspNetCore.Mvc`、`Microsoft.Extensions.DependencyInjection`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
