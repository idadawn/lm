<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
EventBus 模块的注册与工具拓展。包含 `IServiceCollection.AddEventBus` 入口，以及枚举⇄字符串事件 Id 的双向转换工具。

## Key Files
| File | Description |
|------|-------------|
| `EventBusServiceCollectionExtensions.cs` | `AddEventBus(Action<EventBusOptionsBuilder>)` / `AddEventBus(EventBusOptionsBuilder)`。内部默认注册 `ChannelEventSourceStorer`、`ChannelEventPublisher`、`EventBusFactory`，再通过 `AddHostedService` 工厂方法构造 `EventBusHostedService` 并挂载 `UnobservedTaskExceptionHandler`。 |
| `EventBusExtensitions.cs` | `Enum.ParseToString()` 输出 `Assembly;Namespace.Type.Member`；`string.ParseToEnum()` 反序列化（`Assembly.Load` + `Enum.Parse`）。`namespace Poxiao.Extensitions.EventBus`（注意框架沿用 "Extensitions" 拼写）。 |

## For AI Agents

### Working in this directory
- 注册时机：在 `Startup.ConfigureServices` 或 `Program.cs` 中调用 `services.AddEventBus(builder => builder.AddSubscribers(typeof(Program).Assembly))`。
- 默认存储器是有限容量 `Channel`，超出 `ChannelCapacity` 的写入会**等待**（`BoundedChannelFullMode.Wait`），不会丢消息但可能阻塞发布方。
- 替换 `IEventSourceStorer` 时优先使用 `ReplaceStorerOrFallback` —— 初始化失败可平滑回退至内存通道。

### Common patterns
- 命名空间分流：DI 拓展放在 `Microsoft.Extensions.DependencyInjection`；纯工具拓展放在 `Poxiao.Extensitions.EventBus`。

## Dependencies
### Internal
- `EventBusOptionsBuilder`、`ChannelEventPublisher`、`ChannelEventSourceStorer`、`EventBusFactory`、`EventBusHostedService`。
### External
- `Microsoft.Extensions.DependencyInjection`、`Microsoft.Extensions.Hosting`、`System.Reflection`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
