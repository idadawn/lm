<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dependencies

## Purpose
Schedule 模块面向使用者的"作业处理程序"抽象。定义业务侧需要实现的 `IJob` 与可选的 `IJobFactory`，是宿主代码注入到调度引擎的最小契约。

## Key Files
| File | Description |
|------|-------------|
| `IJob.cs` | 作业处理程序接口：`ExecuteAsync(JobExecutingContext, CancellationToken)` 必实现，`FallbackAsync` 默认空实现，发生异常或自定义监视器时被调用。 |
| `IJobFactory.cs` | 作业实例创建工厂：`CreateJob(IServiceProvider, JobFactoryContext)`，用于覆盖默认的 DI 实例化策略（如多租户、key-scope 创建）。 |

## For AI Agents

### Working in this directory
- `IJob` 实现类需搭配 `[JobDetail]` 与一个或多个 `[Trigger]`（或运行时 `Triggers.Cron(...)`）。
- 不要在 `ExecuteAsync` 中阻塞线程；调度引擎用 `TaskFactory.StartNew` 派发，长任务请配合 `stoppingToken`。
- 自定义 `IJobFactory` 返回 null 时，`SchedulerFactory.CreateJob` 会回退到 `ActivatorUtilities.CreateInstance` —— 保留这个语义以兼容默认行为。

### Common patterns
- 业务代码通常通过 `services.AddSchedule(b => b.AddJob<MyJob>(Triggers.Cron(...)))` 注册作业。

## Dependencies
### Internal
- 引用 `../Contexts/JobExecutingContext`、`../Contexts/JobExecutedContext`、`../Contexts/JobFactoryContext`。
### External
- BCL `System.Threading`、`System.Threading.Tasks`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
