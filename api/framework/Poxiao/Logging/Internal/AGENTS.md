<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
日志模块的内部辅助类。两类职责：(1) `Penetrates` 提供共享的"标准消息模板"格式化（带 ANSI 颜色、缩进对齐、堆栈框架、TraceId、异常分隔栏），并从 `appsettings.json` 装配 File/Database `*LoggerProvider`；(2) `StringLoggingPart` 是 `Log.Xxx` 与 `"msg".LogXxx()` 共享的链式构建器。

## Key Files
| File | Description |
|------|-------------|
| `Penetrates.cs` | `internal static`：`CreateFromConfiguration(Func<string>, Action<FileLoggerOptions>)`（File/Database 两个重载，把 Settings 拷到 Options 后构造 Provider）；`OutputStandardMessage`（核心模板，把 `LogMessage` 拼成多行带颜色字符串，含异常分隔符 `++++...`）；`SetLogContext` 通过 `IExternalScopeProvider.ForEachScope` 收集 `LogContext` 并合并；`GetLogLevelString` 短名映射；`GetLogLevelConsoleColors` ANSI 转义。 |
| `StringLoggingPart.cs` | `partial sealed class`，承载链式状态：`Message`/`Level`/`Args`/`EventId`/`CategoryType`/`Exception`/`LoggerScoped`/`LogContext`；静态 `Default()`。 |
| `StringLoggingPartSetters.cs` | 链式 `SetMessage/SetLevel/SetEventId/SetCategory<T>/SetException/SetLoggerScoped/SetArgs/ScopeContext` 等流式 API。 |
| `StringLoggingPartMethods.cs` | 终结方法：`LogInformation/LogWarning/LogError/LogDebug/LogTrace/LogCritical`；以及 `GetLogger()`（解析作用域、构造 `ILogger<TCategory>`，返回 `(logger, factory, hasException)` 三元组）。 |
| `Logging.cs` | `internal sealed class StringLogging`：仅作 `Log.*` 默认日志分类名占位。 |

## For AI Agents

### Working in this directory
- `Penetrates.OutputStandardMessage` 的 6 列前缀对齐（`PadLeftAlign`，6 空格）是历史既定布局；改动会破坏现有日志查看工具。
- `OutputStandardMessage` 中 `withStackFrame` 触发 `EnhancedStackTrace.Current()`，性能开销大——只用于排错，不要在生产默认开启。
- 新增链式 setter 加在 `StringLoggingPartSetters.cs`，新增终结方法加在 `StringLoggingPartMethods.cs`，与现有 partial 切分一致。

### Common patterns
- Builder/Method/Setter 三 partial 文件分割；`Penetrates` 内私有 ANSI 颜色助手 `AppendWithColor`/`Get*EscapeCode`。

## Dependencies
### Internal
- `Logging/Implantations/*`、`Poxiao.App.GetTraceId/GetConfig`。

### External
- `Microsoft.Extensions.Logging`、`System.Diagnostics`、`System.Text.StringBuilder`、`Ben.Demystifier (EnhancedStackTrace)`（按 `withStackFrame`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
