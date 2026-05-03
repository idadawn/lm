<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Executors

## Purpose
事件处理程序执行器抽象。允许用自定义执行器替换默认的 `Retry.InvokeAsync` 调用链路，实现超时控制、限流、自定义重试策略等横切逻辑。

## Key Files
| File | Description |
|------|-------------|
| `IEventHandlerExecutor.cs` | 单方法接口：`Task ExecuteAsync(EventHandlerExecutingContext context, Func<EventHandlerExecutingContext, Task> handler)`。当注册了执行器时，`EventBusHostedService` 跳过内置 `Retry.InvokeAsync`，直接调用此方法 — 重试与回退由实现自行负责。 |

## For AI Agents

### Working in this directory
- 注册路径：`EventBusOptionsBuilder.AddExecutor<TExecutor>()` —— 注册为单例 `IEventHandlerExecutor`。
- **注意**：注册执行器后 `EventSubscribeAttribute` 上的 `NumRetries` / `RetryTimeout` / `FallbackPolicy` 不会自动生效，需要在执行器内自行读取 `context.Attribute` 实现等价行为。
- 必须捕获并向上抛出异常，由宿主服务（`EventBusHostedService.BackgroundProcessing` 的 catch 段）统一记录日志、触发 `UnobservedTaskException`。

### Common patterns
- 适合包装 Polly、CancellationTokenSource 超时等策略；执行器是单例，不要持有请求作用域状态。

## Dependencies
### Internal
- `EventHandlerExecutingContext`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
