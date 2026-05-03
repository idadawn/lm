<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Implantations

## Purpose
日志的"实现"层（`ILogger`/`ILoggerProvider`/`ConsoleFormatter` 实体）。包含框架内置的 4 套 sink 与 MVC 监视器切面，以及共享的 `LogMessage` 结构和 `LogContext` 作用域容器。

## Key Files
| File | Description |
|------|-------------|
| `LogMessage.cs` | 结构体，承载 LogName/Level/EventId/Message/Exception/State/Time/ThreadId/UseUtcTimestamp/TraceId/Context；所有 sink 共用此结构。 |
| `LogContext.cs` | 作用域上下文，仅含 `IDictionary<object,object> Properties`；通过 `logger.ScopeContext(ctx)` 注入。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Console/` | `ConsoleFormatterExtend` 拓展控制台格式化器 (see `Console/AGENTS.md`) |
| `Database/` | `DatabaseLogger` + `IDatabaseLoggingWriter` 数据库 sink (see `Database/AGENTS.md`) |
| `Empty/` | `EmptyLogger`/`EmptyLoggerProvider` 空实现 (see `Empty/AGENTS.md`) |
| `File/` | `FileLogger` + 滚动/容量/错误回退选项 (see `File/AGENTS.md`) |
| `Monitors/` | `LoggingMonitor` MVC Filter 切面 (see `Monitors/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `LogMessage` 是 `struct`，所有跨 sink 传递都是值拷贝；保持只读字段（除 `Message`/`Context` 内部可写）以避免线程问题。
- 新增 sink 时复制 `File/` 或 `Database/` 的"Logger + Provider + Options + Settings + Writer"五件套结构，调用方一致性来自此约定。

### Common patterns
- 每个 sink 遵循：`*Logger : ILogger` + `*LoggerProvider : ILoggerProvider, ISupportExternalScope` + `*LoggerOptions`（业务可写） + `*LoggerSettings`（配置文件 POCO） + 可选 `I*Writer`（落地实现注入点）。

## Dependencies
### External
- `Microsoft.Extensions.Logging`、`Microsoft.Extensions.Logging.Console`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
