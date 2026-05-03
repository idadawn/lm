<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Factories

## Purpose
事件总线动态订阅工厂。允许在运行时（非启动期扫描）注册或注销事件处理程序，把订阅操作封装为特殊事件源 `EventSubscribeOperateSource` 写回存储器，由后台主机统一处理。

## Key Files
| File | Description |
|------|-------------|
| `IEventBusFactory.cs` | 公开接口：`Subscribe(eventId, handler, attribute?, methodInfo?, cancellationToken)` / `Unsubscribe(eventId, cancellationToken)`。 |
| `EventBusFactory.cs` | 默认实现 (internal)。仅依赖 `IEventSourceStorer`：把每次操作封装为 `EventSubscribeOperateSource { Operate = Append/Remove }` 调用 `WriteAsync`，由 `EventBusHostedService.ManageEventSubscribers` 在主循环中增删 `ConcurrentDictionary<EventHandlerWrapper, EventHandlerWrapper>`。 |

## For AI Agents

### Working in this directory
- 注入 `IEventBusFactory` 即可；它在 `EventBusServiceCollectionExtensions.AddInternalService` 中以单例注册。
- `Subscribe` 不会立即生效 —— 操作要进入存储器队列再被消费，因此存在毫秒级延迟。
- `attribute` 可为空（动态订阅时方法元数据缺失），此时 `EventHandlerContext.Attribute` / `HandlerMethod` 为 `null`，处理程序应做空检查。

### Common patterns
- 动态注销按 `EventId` 全量匹配并移除字典中所有同 ID 的 `EventHandlerWrapper`。

## Dependencies
### Internal
- `IEventSourceStorer`、`EventSubscribeOperateSource`、`EventSubscribeAttribute`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
