<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Policies

## Purpose
事件重试失败回退策略。当 `[EventSubscribe(NumRetries = N)]` 全部重试用尽并最终抛出异常时，框架回调 `IEventFallbackPolicy.CallbackAsync` 实现死信投递、补偿任务、告警等收尾逻辑。

## Key Files
| File | Description |
|------|-------------|
| `IEventFallbackPolicy.cs` | 单方法接口：`Task CallbackAsync(EventHandlerExecutingContext context, Exception ex)`。框架要求**单例**生命周期；通过 `EventSubscribeAttribute.FallbackPolicy = typeof(MyPolicy)` 与具体订阅方法绑定。 |

## For AI Agents

### Working in this directory
- 必须先调 `EventBusOptionsBuilder.AddFallbackPolicy<T>()` 注册到容器，再在 `[EventSubscribe]` 中 `FallbackPolicy = typeof(T)`，否则 `_serviceProvider.GetService` 返回 null，回退不生效。
- 仅在 `IEventHandlerExecutor` **未注册**时生效（默认路径）；自定义执行器需在内部自行调度策略。
- 回调中再抛异常会被 `EventBusHostedService` 顶层 catch 兜住，但不再触发二次回退。

### Common patterns
- 与 `EventSubscribeAttribute.ExceptionTypes` 协作：仅当异常类型匹配时才会触发重试，最终失败再走 `FallbackPolicy`。

## Dependencies
### Internal
- `EventHandlerExecutingContext`、`Retry.InvokeAsync`（`Poxiao.FriendlyException`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
