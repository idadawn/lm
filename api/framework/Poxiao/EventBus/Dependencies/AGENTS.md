<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dependencies

## Purpose
事件总线对外的两个根接口。`IEventPublisher` 由业务代码注入用于发布事件；`IEventSubscriber` 是空标记接口，订阅类只需实现并贴 `[EventSubscribe]`。

## Key Files
| File | Description |
|------|-------------|
| `IEventPublisher.cs` | 6 个 `Task PublishAsync/PublishDelayAsync` 重载。支持 `IEventSource`、`string` eventId、`Enum` eventId（自动 `ParseToString`）；延迟版本接受 `long delay`（毫秒）。 |
| `IEventSubscriber.cs` | 标记接口，无成员；XMLDoc 内注释展示标准订阅方法骨架（`[EventSubscribe(eventId)] Task Handler(EventHandlerExecutingContext)`）。 |

## For AI Agents

### Working in this directory
- 业务调用一律走 `IEventPublisher`；不要直接写 `IEventSourceStorer`，那是内部存储抽象。
- 自定义订阅类必须 **可被构造为单例**（`EventBusOptionsBuilder.Build` 注册为 `Singleton(typeof(IEventSubscriber), ...)`）—— 谨慎使用作用域服务依赖，建议在处理方法内通过 `App.GetService<T>()` 解析。
- 默认 `IEventPublisher` 实现为 `ChannelEventPublisher`（`../Internal/`），通过 `EventBusOptionsBuilder.ReplacePublisher<T>()` 替换为 RabbitMQ/Kafka 等外部实现。

### Common patterns
- `Enum` 形式事件 Id 通过 `Poxiao.Extensitions.EventBus.EventBusExtensitions.ParseToString` 序列化为 `Assembly;Namespace.Type.Member`，确保跨模块唯一。

## Dependencies
### Internal
- `IEventSource`（事件载体）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
