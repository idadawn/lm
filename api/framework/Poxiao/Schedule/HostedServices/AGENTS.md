<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HostedServices

## Purpose
Schedule 模块的后台主机服务实现。`ScheduleHostedService` 继承 `BackgroundService`，是真正的"调度循环"：等待集群指示 → 预加载 → 周期性查找触发的作业 → 并行派发执行 → 写持久化 → 进入休眠。

## Key Files
| File | Description |
|------|-------------|
| `ScheduleHostedService.cs` | 唯一实现：注入 `IScheduleLogger`/`ISchedulerFactory`，可选解析 `IJobMonitor`、`IJobExecutor`、`IJobClusterServer`；实现 `StartAsync/ExecuteAsync/StopAsync/Dispose`，提供 `CheckIsBlocked` 串行控制、`UnobservedTaskException` 事件桥接。 |

## For AI Agents

### Working in this directory
- 该类由 `AddSchedule` 自动注册为 HostedService，外部代码不要再 `services.AddHostedService<ScheduleHostedService>()`。
- `BackgroundProcessing` 用 `Parallel.ForEach` + `TaskFactory.StartNew` 投递；新增逻辑必须考虑并发安全与异常隔离（捕获后通过 `UnobservedTaskException` 上报）。
- 串行作业 (`Concurrent=false`) 依赖 `JobDetail.Blocked` 标记—— 不要在外部修改该字段。
- 重试只在未配置 `IJobExecutor` 时自动启用，使用 `Retry.InvokeAsync(..., trigger.NumRetries, trigger.RetryTimeout, retryAction:...)`。

### Common patterns
- 失败处理：`trigger.IncrementErrors` → `trigger.SetStatus(...)` → `_schedulerFactory.Shorthand`，最后调用 `FallbackAsync` 与 `Monitor.OnExecutedAsync`。
- 集群协议：StartAsync→`ClusterServer.Start`，ExecuteAsync 入口先 `WaitingForAsync`，StopAsync→`Stop`，Dispose→`Crash`。

## Dependencies
### Internal
- `../Factories/ISchedulerFactory`、`../Monitors/IJobMonitor`、`../Executors/IJobExecutor`、`../Servers/IJobClusterServer`、`../Internal/Penetrates`。
### External
- `Microsoft.Extensions.Hosting.BackgroundService`、`Poxiao.FriendlyException.Retry`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
