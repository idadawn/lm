<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
日志模块的所有 DI/Builder/扩展方法层。把 `Console`/`File`/`Database`/`Monitor` 四种落地方式作为可选项暴露给应用启动；同时把 `string`/`ILogger`/`ILoggerFactory` 扩展成"`"msg".LogInformation()`、`logger.ScopeContext(props)`"风格 API，让业务代码不需要直接接触 `ILogger.Log<T>`。

## Key Files
| File | Description |
|------|-------------|
| `LoggingServiceCollectionExtensions.cs` | `AddConsoleFormatter`、`AddMonitorLogging`（读取 `Logging:Monitor` 节点 + 注册全局 `LoggingMonitorAttribute` MVC Filter）、`AddFileLogging`（4 重载）、`AddDatabaseLogging<TWriter>`（3 重载）。 |
| `ILoggingBuilderExtensions.cs` | 与上面对位的 `ILoggingBuilder.AddFile/AddDatabase/AddConsoleFormatter` 实现。 |
| `ILoggerFactoryExtensions.cs` / `ILoggerExtensions.cs` | `CreateLogger`、`ScopeContext` 等便利方法。 |
| `LogContextExtensions.cs` | 在 `LogContext.Properties` 上的 `Set/SetRange/Get` 链式语法糖。 |
| `LogMessageExtensions.cs` | `LogMessage` 序列化辅助（如 `Write(writerAction, indented)`），被 `LoggerFormatter` 复用。 |
| `StringLoggingExtensions.cs` | `string` 上的 6 个级别 × 多签名拓展，统一委托给 `StringLoggingPart`。 |

## For AI Agents

### Working in this directory
- `AddMonitorLogging` 内会主动设置 `IsMvcFilterRegister=false`、`FromGlobalFilter=true` 来避开过去重复注册的 bug；改动这两行会复活历史问题。
- 新增 sink 注册扩展时，分别在 `IServiceCollection`（业务接入面）+ `ILoggingBuilder`（Logging 子系统接入面）成对暴露，与现状一致。

### Common patterns
- `services.AddLogging(builder => builder.AddX(...))` 包装；`[SuppressSniffer]` 阻止注册重复扫描。

## Dependencies
### Internal
- `Implantations/*` 的 Provider/Logger 类。

### External
- `Microsoft.Extensions.Logging`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
