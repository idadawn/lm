<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Executors

## Purpose
作业执行装饰点接口 `IJobExecutor`。允许宿主在不替换调度核心的前提下，自定义重试、超时、限流或链路追踪等"执行包装"逻辑；若未注册，默认走 `Retry.InvokeAsync`。

## Key Files
| File | Description |
|------|-------------|
| `IJobExecutor.cs` | 单方法接口 `ExecuteAsync(JobExecutingContext, IJob, CancellationToken)`，由 `ScheduleHostedService.BackgroundProcessing` 调用。 |

## For AI Agents

### Working in this directory
- 自定义实现需要保持与 `Retry.InvokeAsync` 等价的"异常向上抛出"语义——异常不抛出会让 `trigger.IncrementErrors` 永远不触发。
- 通过 `services.AddSingleton<IJobExecutor, MyExecutor>()` 注册即可生效，调度器通过 `IServiceProvider.GetService` 解析。
- 注意：覆盖 `IJobExecutor` 后，框架不再自动应用 `trigger.NumRetries` / `RetryTimeout`，需在自定义执行器中显式调用 `Retry.InvokeAsync` 或自行重试。

### Common patterns
- 实现为单例服务，作业执行时由 `ScheduleHostedService` 直接持有引用。

## Dependencies
### Internal
- `../Contexts/JobExecutingContext`、`../Dependencies/IJob`。
### External
- BCL `System.Threading`、`System.Threading.Tasks`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
