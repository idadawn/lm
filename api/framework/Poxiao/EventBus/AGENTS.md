<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# EventBus

## Purpose
In-process (with optional persistent / RabbitMQ-backed) event bus for the Poxiao framework. `MessageCenter` is the static publisher facade used across `api/src/modularity/**` — call `MessageCenter.PublishAsync("event-id", payload)` from anywhere and a hosted-service consumer dispatches to subscribers tagged with `[EventSubscribe]`. Supports delayed publication, monitors, retry policies and pluggable storage.

## Key Files
| File | Description |
|------|-------------|
| `MessageCenter.cs` | Static facade with `PublishAsync` / `PublishDelayAsync` overloads accepting `IEventSource`, `string` event id, or `Enum` event id; resolves an `IEventPublisher` from DI. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[EventSubscribe]` and related markers on handler methods. |
| `Builders/` | Fluent builders for handler / source registration. |
| `Constants/` | Stringly-typed constant ids and config keys. |
| `Contexts/` | `EventHandlerExecutingContext` / `EventHandlerExecutedContext` passed to handlers. |
| `Dependencies/` | Core interfaces (`IEventPublisher`, `IEventSubscriber`, `IEventHandlerMonitor`, etc.). |
| `Executors/` | Strategies for invoking handlers (sequential / parallel). |
| `Extensions/` | `services.AddEventBus(...)` registration entry points. |
| `Factories/` | `IEventHandlerExecutorFactory`, dynamic subscriber factories. |
| `HostedServices/` | Long-running consumers that pump messages from storers. |
| `Internal/` | Private helpers. |
| `Monitors/` | Hooks for tracing/logging executions. |
| `Policies/` | Retry / fallback policies. |
| `Sources/` | `IEventSource` implementations (channel-based, MQ-backed). |
| `Storers/` | Persistent buffers for delayed / durable events. |
| `Wrappers/` | Adapters wrapping handler methods into invocable units. |

## For AI Agents

### Working in this directory
- Prefer publishing through `MessageCenter` (static) for ergonomics, or inject `IEventPublisher` for testability.
- Subscribers go in feature modules (`api/src/modularity/**`) decorated with `[EventSubscribe("event-id")]` and registered via DI conventions (`IScoped` etc.).
- For durable / delayed events, use `PublishDelayAsync` and ensure a non-default `IEventSourceStorer` is registered.

### Common patterns
- All async, every public entry point returns `Task` and accepts `CancellationToken`.
- `[SuppressSniffer]` on the static facade.

## Dependencies
### Internal
- `Poxiao` core (`App.GetService`), `Poxiao.DependencyInjection`, `Poxiao.Logging`.
### External
- `System.Threading.Channels`, optionally RabbitMQ.Client (via infrastructure module).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
