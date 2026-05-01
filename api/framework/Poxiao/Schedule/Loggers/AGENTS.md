<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Loggers

## Purpose
Schedule 模块的日志门面。`IScheduleLogger` 提供 `LogTrace/Debug/Information/Warning/Error/Critical/Log` 接口；`ScheduleLogger` 默认实现把消息转发到 `ILogger<ScheduleService>`，并支持运行时关闭（`LogEnabled=false`）。

## Key Files
| File | Description |
|------|-------------|
| `IScheduleLogger.cs` | 公共日志接口，参数与 `Microsoft.Extensions.Logging` 风格一致（消息模板 + `params object[] args`）。 |
| `ScheduleLogger.cs` | 默认实现：依赖 `ILogger<ScheduleService>`，根据 `logEnabled` 选择 no-op；Error 路径单独走 `_logger.LogError(ex, ...)`。 |

## For AI Agents

### Working in this directory
- 通过 `ScheduleOptionsBuilder.LogEnabled` 控制；该值在 `AddInternalService` 中以构造参数注入。
- 不要让 `ScheduleLogger` 在禁用状态下构造消息字符串——通过 `LogEnabled` 早返回保护性能。
- 自定义实现请保持"内部消息全部走 `Log(LogLevel, ...)`"的统一入口，便于日志中转/采集。

### Common patterns
- 调度内部日志 categoryName 实际为 `System.Logging.ScheduleService`（参见 `Internal/Logging.cs` 与 `ScheduleExtensions.GetLogger`）。

## Dependencies
### External
- `Microsoft.Extensions.Logging`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
