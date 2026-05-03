<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Monitors

## Purpose
事件处理程序监视器钩子。提供执行前/后切面，常用于审计、指标埋点、链路追踪、统一日志等横切关注点。

## Key Files
| File | Description |
|------|-------------|
| `IEventHandlerMonitor.cs` | 双方法接口：`OnExecutingAsync(EventHandlerExecutingContext)` 与 `OnExecutedAsync(EventHandlerExecutedContext)`。框架在每个处理程序调用前后**串行**触发，`OnExecutedAsync` 在 `finally` 块内总会被调用，可读取 `Exception` 字段感知失败。 |

## For AI Agents

### Working in this directory
- 注册：`EventBusOptionsBuilder.AddMonitor<TMonitor>()` —— 单例。整个进程仅一个监视器实例（`GetService<IEventHandlerMonitor>()`，后注册的覆盖前者）。
- 监视器抛异常**不会**被宿主吞掉 —— 会冒泡到 `Parallel.ForEach` 的 `StartNew` 任务，最终触发 `UnobservedTaskException`。请在监视器内自行 try/catch。
- 不要在监视器内做长耗时 IO；它在事件处理热路径上。

### Common patterns
- 与执行器（`IEventHandlerExecutor`）同时存在时，监视器仍被宿主直接调用，独立于执行器。

## Dependencies
### Internal
- `EventHandlerExecutingContext`、`EventHandlerExecutedContext`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
