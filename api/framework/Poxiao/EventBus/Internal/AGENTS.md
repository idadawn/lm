<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
EventBus 默认实现细节。承载内存通道发布者与日志（`partial` 类的 `LoggerMessage` 高性能日志方法）等不对外暴露的组件。

## Key Files
| File | Description |
|------|-------------|
| `ChannelEventPublisher.cs` | 默认 `IEventPublisher` 实现 (internal sealed partial)。所有 `PublishAsync` 重载都构造 `ChannelEventSource(eventId, payload, ct)` 后调用 `_eventSourceStorer.WriteAsync`；`PublishDelayAsync` 通过 `Task.Factory.StartNew + Task.Delay` 延迟后再写入（fire-and-forget）。 |
| `Logging.cs` | `partial` 类的 `LoggerMessage` 源生成日志（高性能结构化日志）。 |

## For AI Agents

### Working in this directory
- 不要直接 `new ChannelEventPublisher`；通过 DI 解析 `IEventPublisher`。它在 `EventBusServiceCollectionExtensions.AddInternalService` 中注册为单例。
- `PublishDelayAsync` 的延迟任务**未持久化** —— 进程重启会丢失。需要持久化延迟需替换 `IEventSourceStorer` 为外部存储。
- 想替换发布者实现，参见 `EventBusOptionsBuilder.ReplacePublisher<T>()`。

### Common patterns
- `partial sealed class` 配合 `Logging.cs` 是 .NET 现代日志惯用模式。

## Dependencies
### Internal
- `IEventSourceStorer`、`ChannelEventSource`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
