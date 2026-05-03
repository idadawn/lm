<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
事件总线订阅特性。声明 `IEventSubscriber` 实现类的处理方法该响应哪些事件 Id，并配置重试、模糊匹配、排序、回退策略等行为。

## Key Files
| File | Description |
|------|-------------|
| `EventSubscribeAttribute.cs` | `[EventSubscribe(eventId)]` — 标记订阅方法；接受 `string` 或 `Enum`（通过 `Enum.ParseToString()` 序列化为 `assembly;FullName.Name`）。属性包含 `FuzzyMatch`（正则匹配）、`GCCollect`、`NumRetries`/`RetryTimeout`/`ExceptionTypes`、`FallbackPolicy`、`Order`（数值越大越先执行）。 |

## For AI Agents

### Working in this directory
- 此特性只能贴在 `IEventSubscriber` 实现类的实例方法上，方法签名必须为 `Task XxxHandler(EventHandlerExecutingContext context)`。
- 同一方法可声明多个 `[EventSubscribe]`（`AllowMultiple = true`）以响应多个事件 Id。
- `FallbackPolicy` 指定的类型必须实现 `IEventFallbackPolicy`，且需通过 `EventBusOptionsBuilder.AddFallbackPolicy` 注册到容器。

### Common patterns
- `[SuppressSniffer]` 用于阻止依赖注入扫描器误把特性识别为可注入服务。
- 重试参数（`NumRetries`/`RetryTimeout`/`ExceptionTypes`）由 `EventBusHostedService` 通过 `Retry.InvokeAsync` 兑现。

## Dependencies
### Internal
- `Poxiao.Extensitions.EventBus.EventBusExtensitions.ParseToString` — 枚举到字符串。
- `IEventSubscriber`、`IEventFallbackPolicy`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
