<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
`JsonSerialization` 的 DI 接线层。把 `IJsonSerializerProvider` 实现注入容器，并提供针对 `JsonOptions`、`MvcNewtonsoftJsonOptions` 的快捷配置入口，被框架核心 `Builder/Startup` 调用以装配默认 Converter。

## Key Files
| File | Description |
|------|-------------|
| `JsonSerializationServiceCollectionExtensions.cs` | `AddJsonSerialization<TProvider>()`（注册单例 Provider）、`AddJsonOptions(Action<JsonOptions>)`（非 Web 场景手动配置 STJ 选项）。 |
| `SystemTextJsonExtensions.cs` | 针对 `JsonSerializerOptions` / `JsonOptions` 的链式配置帮助：批量加 long/DateTime/DateOnly/TimeOnly/Clay 等默认 Converter。 |
| `NewtonsoftJsonExtensions.cs` | 针对 `JsonSerializerSettings`/`MvcNewtonsoftJsonOptions` 的同义配置帮助；`AddLongTypeConverters` 等方法被 `LoggingMonitor` 直接复用。 |

## For AI Agents

### Working in this directory
- 命名空间为 `Microsoft.Extensions.DependencyInjection` / `Microsoft.AspNetCore.Mvc`，使扩展方法对调用方"零 using"——保持不变。
- 新增 Provider 走 `AddJsonSerialization<T>()`；不要重复 `services.AddSingleton<IJsonSerializerProvider, ...>`。

### Common patterns
- 链式 `IServiceCollection`/`IMvcBuilder` 返回；`[SuppressSniffer]` 阻止被注册扫描器重复处理。

## Dependencies
### Internal
- `IJsonSerializerProvider`、各自定义 Converter。

### External
- `Microsoft.Extensions.DependencyInjection`、`Microsoft.AspNetCore.Mvc.JsonOptions`、`Newtonsoft.Json`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
