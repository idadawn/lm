<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HostedServices

## Purpose
TaskQueue 的消费端：常驻的 .NET `BackgroundService`，循环出队并执行任务委托，带重试与未察觉异常事件。

## Key Files
| File | Description |
|------|-------------|
| `TaskQueuedHostedService.cs` | `internal sealed class TaskQueueHostedService : BackgroundService`：构造注入 `ILogger<TaskQueueService>` / `IServiceProvider` / `ITaskQueue`；`ExecuteAsync` 循环 `DequeueAsync` 并调用 `Retry.InvokeAsync(handler, retries:3, delay:1000ms)`；失败时记日志并经 `UnobservedTaskException` 事件透传 `AggregateException`。 |

## For AI Agents

### Working in this directory
- 当前实现使用 `Parallel.For(0, 1, async _ => ...)`，等价于一个并发槽位——若需提高吞吐应改并发度并评估 `IServiceProvider` 作用域。
- 重试参数硬编码（3 次/1000ms），需要可配置时通过 `TaskQueueOptionsBuilder` 暴露。
- 异常事件订阅在 `Extensions.AddTaskQueue` 中完成，**不要在此修改订阅时机**。
- 直接使用根 `_serviceProvider`：handler 内务必创建 `IServiceScope` 才能消费 Scoped 服务。

### Common patterns
- `BackgroundService` + `await DequeueAsync(stoppingToken)` 循环体；`LogCritical` 标记主机停止。

## Dependencies
### Internal
- `Poxiao.FriendlyException.Retry`、`Poxiao.TaskQueue.ITaskQueue`、`System.Logging.TaskQueueService`。
### External
- `Microsoft.Extensions.Hosting.BackgroundService`、`Microsoft.Extensions.Logging`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
