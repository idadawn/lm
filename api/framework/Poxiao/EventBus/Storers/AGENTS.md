<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Storers

## Purpose
事件源存储器抽象。事件总线"消息中心"的读写端口，可替换为 Kafka、SQL Server、Redis 等外部介质，默认是内存有界 `Channel`。

## Key Files
| File | Description |
|------|-------------|
| `IEventSourceStorer.cs` | 双方法接口：`ValueTask WriteAsync(IEventSource, CancellationToken)` 与 `ValueTask<IEventSource> ReadAsync(CancellationToken)`。 |
| `ChannelEventSourceStorer.cs` | 默认实现 (internal sealed partial)。基于 `System.Threading.Channels.BoundedChannel<IEventSource>`，`FullMode = Wait` —— 队列满时**阻塞写入方**而不丢消息。容量来自 `EventBusOptionsBuilder.ChannelCapacity`（默认 3000）。 |

## For AI Agents

### Working in this directory
- 替换路径：`EventBusOptionsBuilder.ReplaceStorer(sp => new MyStorer())` 或 `ReplaceStorerOrFallback(...)`（异常时退回 `ChannelEventSourceStorer`）。
- 自定义实现必须支持 `EventSubscribeOperateSource` 类型（动态订阅消息），通常透传即可，不需要特殊路由。
- 写入失败的语义由实现自定 —— 默认实现会因 `cancellationToken` 取消而抛 `OperationCanceledException`。

### Common patterns
- 单例生命周期，`ReadAsync` 在后台主机的 `while` 循环中持续 `await`，是热路径上唯一的同步点。

## Dependencies
### Internal
- `IEventSource`。
### External
- `System.Threading.Channels` (BCL)。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
