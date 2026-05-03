<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Contexts

## Purpose
事件处理程序上下文对象。框架在调度订阅方法时构造这些上下文，向用户处理函数和监视器传递事件源、共享数据、方法元数据与执行计时。

## Key Files
| File | Description |
|------|-------------|
| `EventHandlerContext.cs` | 抽象基类。字段：`IEventSource Source`、`IDictionary<object,object> Properties`（执行前/后共享）、`MethodInfo HandlerMethod`、`EventSubscribeAttribute Attribute`。动态订阅时 `HandlerMethod` / `Attribute` 可能为 `null`。 |
| `EventHandlerExecutingContext.cs` | 执行前上下文。新增 `DateTime ExecutingTime`，由 `EventBusHostedService` 按 `UseUtcTimestamp` 标志设值。订阅方法签名固定为 `Func<EventHandlerExecutingContext, Task>`。 |
| `EventHandlerExecutedContext.cs` | 执行后上下文（被 `IEventHandlerMonitor.OnExecutedAsync` 接收）。携带 `ExecutedTime`、`Exception`（`InvalidOperationException`，包裹原始异常）。 |

## For AI Agents

### Working in this directory
- 构造函数为 `internal` —— 仅 `EventBusHostedService` 应实例化，业务代码不要 `new`。
- `Properties` 字典在同一事件的多个处理程序间共享，可用于跨处理程序传值；写入时注意线程安全（`Parallel.ForEach` + `taskFactory.StartNew`）。

### Common patterns
- 监视器钩子：`Monitor.OnExecutingAsync(executing)` → 处理 → `Monitor.OnExecutedAsync(executed)`，由 `finally` 块保证调用。

## Dependencies
### Internal
- `IEventSource`、`EventSubscribeAttribute`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
