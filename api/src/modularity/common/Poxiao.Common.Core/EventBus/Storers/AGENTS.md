<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Storers

## Purpose
`IEventSourceStorer` implementations that bridge the in-memory `Channel<IEventSource>` queue with an external broker. Currently only RabbitMQ; Redis and Kafka entries exist in `EventBusOptions.EventBusType` for future implementation.

## Key Files
| File | Description |
|------|-------------|
| `RabbitMQEventSourceStorer.cs` | Bounded `Channel.CreateBounded<IEventSource>(capacity)` with `BoundedChannelFullMode.Wait`. Declares a non-durable, non-exclusive queue, `BasicQos(prefetchCount: 1)`, manual ack. Special-cases payloads that arrive without an `EventId` as `User:Maxkey_Identity` (Maxkey SSO publishes raw JSON). |

## For AI Agents

### Working in this directory
- The storer is `IDisposable` and owns both the `IConnection` and `IModel`; do not share them across multiple storers.
- `WriteAsync(...)` is split: `ChannelEventSource` payloads are published to the queue (`BasicPublish`), other event types fall back to in-memory `Channel.WriteAsync` so dynamic subscriptions still work.
- When adding a new broker, mirror this shape: queue declaration in the constructor, message dispatch in `Received += ...`, `BasicAck` after the channel write succeeds.

### Common patterns
- Logs each received message via `Poxiao.Logging.Log.Information`.

## Dependencies
### Internal
- `Poxiao.EventBus`, `Poxiao.Infrastructure.Extension`, `Poxiao.Logging`.

### External
- RabbitMQ.Client (`ConnectionFactory`, `IConnection`, `IModel`, `EventingBasicConsumer`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
