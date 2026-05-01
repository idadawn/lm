<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Database

## Purpose
将日志写入数据库的 sink。框架只提供基础设施（队列、级别过滤、上下文绑定、防递归），具体写入由业务模块实现 `IDatabaseLoggingWriter` 接口注入；检测室系统的操作日志、异常日志表都基于此扩展。

## Key Files
| File | Description |
|------|-------------|
| `DatabaseLogger.cs` | `ILogger` 实现：按 `MinimumLevel` 过滤；构造 `LogMessage`（含 TraceId）；调用 `Penetrates.SetLogContext`/`OutputStandardMessage`；通过 `IgnoreReferenceLoop`+`StackTrace` 检测避免 Writer 自身写日志导致递归；最终 `provider.WriteToQueue(logMsg)`。 |
| `DatabaseLoggerProvider.cs` | `ILoggerProvider, ISupportExternalScope`，持有阻塞队列、后台线程、`IDatabaseLoggingWriter` 工厂。 |
| `DatabaseLoggerOptions.cs` | 业务可写选项：`MinimumLevel`/`UseUtcTimestamp`/`DateFormat`/`IncludeScopes`/`IgnoreReferenceLoop`/`MessageFormat`/`WriteFilter`/`WithTraceId`/`WithStackFrame`/`WriteError`。 |
| `DatabaseLoggerSettings.cs` | 配置文件 POCO，对应 `appsettings.json` 中的 `Logging:Database` 节点。 |
| `DatabaseWriteError.cs` | 写入失败上下文，供 `HandleWriteError` 委托捕获处理。 |
| `IDatabaseLoggingWriter.cs` | 业务侧落库实现接口；签名 `void Write(LogMessage logMsg, bool flush)`。 |

## For AI Agents

### Working in this directory
- 业务实现 `IDatabaseLoggingWriter` 时**不要在内部再调用 `Log.*`**——会被 `IgnoreReferenceLoop` 检测拦截但仍是反模式。
- 新增字段在 `DatabaseLoggerOptions` 后，同步加入 `DatabaseLoggerSettings` 与 `Internal/Penetrates.CreateFromConfiguration` 的拷贝逻辑，否则 appsettings 配置不会生效。

### Common patterns
- Logger（写入入口） + Provider（生命周期/队列） + Options（运行时配置） + Settings（配置文件） + Writer（落地接口）五件套。

## Dependencies
### Internal
- `Logging/Internal/Penetrates`、`LogMessage`。

### External
- `Microsoft.Extensions.Logging`、`System.Diagnostics.StackTrace`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
