<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# File

## Purpose
将日志异步写入磁盘文件的 sink。支持滚动归档（`FileSizeLimitBytes`+`MaxRollingFiles`）、自定义文件名规则（含时间戳）、写入失败回退到备用文件名（`HandleWriteError`+`UseRollbackFileName`）。检测室系统在 `Linux/Windows` 部署时，应用日志默认走该 sink。

## Key Files
| File | Description |
|------|-------------|
| `FileLogger.cs` | `ILogger`，按 `MinimumLevel` 过滤后构造 `LogMessage`、调用 `Penetrates.SetLogContext`+`OutputStandardMessage`，最终 `provider.WriteToQueue(logMsg)`。 |
| `FileLoggerProvider.cs` | `ILoggerProvider, ISupportExternalScope`，持有写入队列、后台 `Task`，处理滚动与异常回退。 |
| `FileLoggingWriter.cs` | 实际文件 IO（`FileStream` 追加/截断、滚动检查），从队列拉数据。 |
| `FileLoggerOptions.cs` | 选项：`Append`、`FileSizeLimitBytes`、`MaxRollingFiles`、`MinimumLevel`、`UseUtcTimestamp`、`MessageFormat`、`WriteFilter`、`FileNameRule`、`HandleWriteError`、`DateFormat`、`IncludeScopes`、`WithTraceId`、`WithStackFrame`。 |
| `FileLoggerSettings.cs` | 配置文件 POCO，对应 `appsettings.json` 中的 `Logging:File` 节。 |
| `FileWriteError.cs` | 写入失败上下文：`CurrentFileName` 等，提供 `UseRollbackFileName`。 |

## For AI Agents

### Working in this directory
- `FileSizeLimitBytes` 单位是字节（注释明确 `1024=1KB`）；`MaxRollingFiles` 触发后会**从最初日志文件覆盖**——不要假设无限累积。
- `FileNameRule` 接收原始 `fileName` 返回新名（含路径）；最常见实现是按日期生成 `app_2026-04-30.log`。
- 写入异常处理推荐 `err.UseRollbackFileName(... + "_alt" + ext)`，避免被同一占用文件反复阻塞。

### Common patterns
- Logger + Provider + Options + Settings + Writer + Error 六件套；与 Database sink 形成对位。

## Dependencies
### Internal
- `Logging/Internal/Penetrates`、`LogMessage`。

### External
- `Microsoft.Extensions.Logging`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
