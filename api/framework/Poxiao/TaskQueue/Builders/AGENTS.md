<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Builders

## Purpose
TaskQueue 的配置构建器，承载用户在 `AddTaskQueue(opts => ...)` 时设置的容量与异常处理策略。

## Key Files
| File | Description |
|------|-------------|
| `TaskQueueOptionsBuilder.cs` | `ChannelCapacity`（默认 3000；超出后按 `Wait` 模式阻塞生产者）、`UnobservedTaskExceptionHandler`（订阅后台主机的未察觉异常事件）。`Build()` 当前为占位，预留后续校验。 |

## For AI Agents

### Working in this directory
- 仅承载配置数据，**禁止**在此触发 DI 注册——注册逻辑集中在 `Extensions/TaskQueueServiceCollectionExtensions.cs`。
- 增加新选项时同时更新 `Internal/AddInternalService` 中传给 `TaskQueue` 构造函数的参数与 `HostedService` 的订阅。
- 异常处理委托返回 `EventHandler<UnobservedTaskExceptionEventArgs>`，与 .NET TaskScheduler 的事件签名保持一致以便复用日志/告警代码。

### Common patterns
- `sealed [SuppressSniffer]` POCO Builder，方法链式由调用方使用。

## Dependencies
### External
- `System.Threading.Tasks`（`UnobservedTaskExceptionEventArgs`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
