<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Builders

## Purpose
EventBus 注册流式 API。`EventBusOptionsBuilder` 是 `services.AddEventBus(opt => ...)` 委托接收的对象，用于注册订阅者、替换发布者/存储器、登记监视器/执行器/回退策略以及调节模糊匹配、GC、UTC 时间、日志等运行参数。

## Key Files
| File | Description |
|------|-------------|
| `EventBusOptionsBuilder.cs` | 默认 `ChannelCapacity = 3000`；提供 `AddSubscriber<T>()` / `AddSubscribers(Assembly[])`、`ReplacePublisher<T>`、`ReplaceStorer(factory)`/`ReplaceStorerOrFallback(...)`（失败时回退到 `ChannelEventSourceStorer`）、`AddMonitor<T>` / `AddExecutor<T>` / `AddFallbackPolicy<T>`。`internal void Build(IServiceCollection)` 把订阅者注册为 `IEventSubscriber` 单例，按需 `Replace` 发布者/存储器。 |

## For AI Agents

### Working in this directory
- `Build()` 是 `internal`，只能由 `EventBusServiceCollectionExtensions.AddEventBus` 调用 — 不要从外部触发。
- 所有事件总线组件以**单例**注册（订阅者、发布者、监视器、执行器、回退策略），`HostedService` 启动时一次性扫描所有订阅方法并缓存为 `EventHandlerWrapper`。
- `UnobservedTaskExceptionHandler` 通过 `EventBusHostedService.UnobservedTaskException += handler` 转交，订阅时机在工厂内 `AddHostedService` 委托中。

### Common patterns
- 类型校验：`AddSubscriber(Type)` 与 `AddFallbackPolicy(Type)` 都会强制 `IsAssignableFrom` 检查并拒绝接口/抽象类。

## Dependencies
### Internal
- `IEventSubscriber`, `IEventPublisher`, `IEventSourceStorer`, `IEventHandlerMonitor`, `IEventHandlerExecutor`, `IEventFallbackPolicy`, `ChannelEventSourceStorer`.
### External
- `Microsoft.Extensions.DependencyInjection.Extensions` — `services.Replace(ServiceDescriptor)`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
