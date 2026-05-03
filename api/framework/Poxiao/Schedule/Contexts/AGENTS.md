<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Contexts

## Purpose
作业调度运行时上下文对象集合。承载执行前/执行后、持久化、工厂创建、集群心跳等阶段的数据载荷，在 `ScheduleHostedService`、`SchedulerFactory` 与外部 `IJobMonitor`/`IJobPersistence`/`IJobClusterServer` 之间传递作业元数据与运行结果。

## Key Files
| File | Description |
|------|-------------|
| `JobExecutionContext.cs` | 抽象基类，封装 JobId/TriggerId/JobDetail/Trigger/OccurrenceTime/RunId/ServiceProvider/Result，提供 `ConvertToJSON`。 |
| `JobExecutingContext.cs` | 执行前上下文（含 `ExecutingTime`），传给 `IJob.ExecuteAsync`。 |
| `JobExecutedContext.cs` | 执行后上下文（含 `ExecutedTime`、`Exception`、`Result`），用于 `FallbackAsync` 与 `IJobMonitor.OnExecutedAsync`。 |
| `JobFactoryContext.cs` | 作业实例创建上下文（JobId + JobType），由 `IJobFactory.CreateJob` 接收。 |
| `PersistenceContext.cs` | 作业信息持久化通知（JobDetail + `PersistenceBehavior`），提供 `ConvertToSQL` / `ConvertToJSON` / `ConvertToMonitor`。 |
| `PersistenceTriggerContext.cs` | 触发器持久化通知，扩展 `PersistenceContext` 携带 Trigger 数据。 |
| `JobClusterContext.cs` | 集群消息上下文（ClusterId），用于 `IJobClusterServer` 的 Start/Stop/Crash/WaitingFor。 |

## For AI Agents

### Working in this directory
- 上下文构造函数为 `internal`；外部代码不应自行 `new`，由 `SchedulerFactory` 与 `ScheduleHostedService` 创建。
- 上下文类型保留 `[SuppressSniffer]`，避免被 Furion/Poxiao 类型扫描污染。
- 修改属性时同步检查 `Penetrates.GetNaming` 路径——`ConvertToJSON`/`ConvertToSQL` 依赖 `NamingConventions` 转换。

### Common patterns
- 上下文持有 `JobDetail` + `Trigger` 引用而非拷贝；监视器/回退中读到的状态为最新值。
- `ToString()` 输出 `JobDetail Trigger OccurrenceTime [Status / NextRunTime]`，被日志直接消费。

## Dependencies
### Internal
- 依赖 `../Constants/`（`PersistenceBehavior`、`NamingConventions`）、`../Details/JobDetail`、`../Triggers/Trigger`、`../Internal/Penetrates`。
### External
- `Microsoft.Extensions.DependencyInjection.IServiceProvider`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
