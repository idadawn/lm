<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Empty

## Purpose
"什么都不做"的 ILogger 占位实现。用于单元测试、CLI 工具、`App.RootServices` 未就绪场景下避免 `NullReferenceException`，是 `Log.CreateLoggerFactory` 与各种降级路径的保底落点。

## Key Files
| File | Description |
|------|-------------|
| `EmptyLogger.cs` | `IsEnabled` 永远返回 `false`；`Log<TState>` 在级别有效检查后立即返回；`BeginScope` 返回 `default`（null `IDisposable`）。 |
| `EmptyLoggerProvider.cs` | `ILoggerProvider`，`CreateLogger` 直接 new `EmptyLogger()`；`Dispose` 空实现。 |

## For AI Agents

### Working in this directory
- 不要在此目录加任何状态——这是真正的 no-op，性能敏感。
- 若需要"开关式"日志，使用 `MinimumLevel = LogLevel.None` 而不是改 EmptyLogger。

### Common patterns
- 极简 sealed 实现，仅覆盖必需接口方法。

## Dependencies
### External
- `Microsoft.Extensions.Logging`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
