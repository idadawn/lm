<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Console

## Purpose
拓展 ASP.NET Core 默认控制台日志格式化器。覆盖原版 `ConsoleFormatter` 的输出布局（颜色、TraceId、堆栈帧、自定义 `MessageFormat`），开发期 `dotnet watch run` 输出靠它实现彩色对齐和异常分隔栏。

## Key Files
| File | Description |
|------|-------------|
| `ConsoleFormatterExtend.cs` | `ConsoleFormatter`（命名 `console-format`），构造注入 `IOptionsMonitor<ConsoleFormatterExtendOptions>` 支持热重载；`Write<TState>` 内构造 `LogMessage`，按选项决定走 `Penetrates.OutputStandardMessage` 还是用户自定义 `MessageFormat`，再交给可选 `WriteHandler` 或直接 `textWriter.WriteLine`。 |
| `ConsoleFormatterExtendOptions.cs` | 选项 POCO：`UseUtcTimestamp`、`DateFormat`、`ColorBehavior`、`IncludeScopes`、`MessageFormat`、`WriteHandler`、`WithTraceId`、`WithStackFrame`。 |
| `ConsoleColors.cs` | `internal readonly struct ConsoleColors`，承载前/背景色，被 `Penetrates` 颜色装饰使用。 |

## For AI Agents

### Working in this directory
- 颜色禁用规则：`ColorBehavior == Disabled` 或 `Default && Console.IsOutputRedirected`——保持此判断，否则在 docker/journald 输出会留下 ANSI 乱码。
- `MessageFormat` 优先级高于内置标准模板；外部自定义时务必返回非 null，否则 `textWriter.WriteLine` 会写空行。

### Common patterns
- 选项热重载：`OnChange(ReloadFormatterOptions)` + 私有可变字段 `_formatterOptions`。

## Dependencies
### Internal
- `Logging/Internal/Penetrates`、`LogMessage`。

### External
- `Microsoft.Extensions.Logging.Console`、`Microsoft.Extensions.Logging.Abstractions`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
