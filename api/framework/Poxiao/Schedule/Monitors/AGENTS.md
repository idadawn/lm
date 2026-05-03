<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Monitors

## Purpose
作业执行监视器接口。允许宿主在每次作业触发的"执行前"/"执行后"插入横切逻辑，例如埋点、Prometheus 指标、租户审计、链路追踪等，由 `ScheduleHostedService` 在调用 `IJob.ExecuteAsync` 前后驱动。

## Key Files
| File | Description |
|------|-------------|
| `IJobMonitor.cs` | `OnExecutingAsync(JobExecutingContext, CancellationToken)` 与 `OnExecutedAsync(JobExecutedContext, CancellationToken)` 双钩子。 |

## For AI Agents

### Working in this directory
- 注册：`services.AddSingleton<IJobMonitor, MyMonitor>()`。`Monitor` 解析失败时 `OnExecutedAsync` 仍会在异常路径下被代码绕过，但 `OnExecutingAsync` 不被调用。
- `OnExecutedAsync` 调用被 `try/catch` 吞掉异常 —— 不要把关键失败逻辑放这里，应当返回 `Task.CompletedTask` 或自行记录。
- `JobExecutedContext.Exception` 为 `InvalidOperationException`（含 inner），监视器需要内部解包再判断真实异常类型。

### Common patterns
- 使用监视器搭配 `IJobExecutor` 可分离"执行包装"与"观测"职责。

## Dependencies
### Internal
- `../Contexts/JobExecutingContext`、`../Contexts/JobExecutedContext`。
### External
- BCL `System.Threading`、`System.Threading.Tasks`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
