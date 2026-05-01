<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Sources

## Purpose
事件源（事件承载对象）抽象与默认实现。事件源是流经存储器、被订阅者消费的最小消息单元，封装事件 Id、负载、创建时间、取消令牌。

## Key Files
| File | Description |
|------|-------------|
| `IEventSource.cs` | 公开接口：`EventId` / `Payload` / `CreatedTime` / `CancellationToken`。 |
| `ChannelEventSource.cs` | 默认实现。多重构造：`string` / `Enum`（自动 `ParseToString`）+ payload + `CancellationToken`；`CancellationToken` 同时被 Newtonsoft 与 `System.Text.Json` 标注 `JsonIgnore`，序列化安全。`CreatedTime` 默认 `DateTime.UtcNow`。 |
| `EventSubscribeOperateSource.cs` | 框架内部专用事件源：携带 `EventSubscribeOperates` 操作（Append/Remove）、handler 委托、特性。被 `EventBusFactory` 写入存储器，`EventBusHostedService` 识别后走动态订阅分支而非派发。 |

## For AI Agents

### Working in this directory
- 业务一般不需要直接 `new ChannelEventSource` —— 用 `IEventPublisher.PublishAsync(eventId, payload)` 重载即可。
- 如要替换为持久化存储（Kafka/RabbitMQ/SQL），可自定义 `IEventSource` + `IEventSourceStorer`，注意保留 `EventSubscribeOperateSource` 类型识别能力。
- `Payload` 是 `object` —— 序列化策略由具体存储器决定。

### Common patterns
- `EventSubscribeOperateSource` 没有 `EventId` 用作分发，而是被 `is` 类型检查在主循环中"短路"。

## Dependencies
### Internal
- `Poxiao.Extensitions.EventBus.EventBusExtensitions.ParseToString`、`EventSubscribeAttribute`、`EventSubscribeOperates`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
