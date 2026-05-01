<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Logging

## Purpose
检测室系统统一日志门面。基于 `Microsoft.Extensions.Logging.ILogger`，扩展出 4 个落地通道（`Console`/`File`/`Database`/`Empty`）和 1 个 MVC 切面（`LoggingMonitor`）；同时提供 `Log.Information/Warning/Error/Critical/Debug/Trace`、`"msg".LogXxx()` 字符串扩展、`LogContext` 作用域上下文。系统所有审计/操作日志（含 `[OperateLog]` 切面）都收敛到此处。

## Key Files
| File | Description |
|------|-------------|
| `Log.cs` | 静态门面，约 600 行，覆盖 6 个级别 × 4 个签名重载（msg+args / +EventId / +Exception / +EventId+Exception）× 是否带 `<TClass>` 分类。 |
| `LoggerFormatter.cs` | 提供 `Func<LogMessage,string> Json/JsonIndented` 内置格式化器，按 `Utf8JsonWriter` 输出结构化字段（logLevel、logDateTime、traceId 等）。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[OperateLog]` 操作日志、`[IgnoreLog]` 忽略标记 (see `Attributes/AGENTS.md`) |
| `Extensions/` | DI 注册（`AddFileLogging`/`AddDatabaseLogging`/`AddMonitorLogging`）+ Logger/字符串扩展 (see `Extensions/AGENTS.md`) |
| `Implantations/` | 4 套 ILogger 实现 + LoggingMonitor 切面 (see `Implantations/AGENTS.md`) |
| `Internal/` | `Penetrates` 标准格式化、`StringLoggingPart` 链式构建器 (see `Internal/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 业务代码统一用 `Log.Xxx` 或 `"msg".LogXxx()`；不要直接 new `LoggerFactory`——只有 `Log.CreateLoggerFactory` 才会装配 `ConsoleFormatterExtend`。
- 新增 sink 时在 `Implantations/` 建子目录，对应 `Extensions/LoggingServiceCollectionExtensions` 暴露 `AddXxxLogging`。
- `LoggingMonitor` 默认全局生效——若不希望某个 Action 被监控，加 `[SuppressMonitor]` 而不是改全局配置。

### Common patterns
- `StringLoggingPart` 链式 builder + 静态门面双入口；结构化日志通过 `LogContext.Properties` 传递。

## Dependencies
### Internal
- `Poxiao.App.GetTraceId/GetThreadId`、`Poxiao.UnifyResult.UnifyContext`、`Poxiao.FriendlyException`。

### External
- `Microsoft.Extensions.Logging`、`Microsoft.Extensions.Logging.Console`、`Newtonsoft.Json`、`System.Text.Json`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
