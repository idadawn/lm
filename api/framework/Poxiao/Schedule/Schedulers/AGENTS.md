<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Schedulers

## Purpose
"作业计划"的接口与实现。`Scheduler` 把单个 `JobDetail` 与其多触发器封装成一个可被工厂调用的运行单元，提供新增/更新/删除触发器、Pause/Start/Run/Persist/Collate/Reload、模型与构建器互转等操作。

## Key Files
| File | Description |
|------|-------------|
| `IScheduler.cs` | 公开接口：`GetModel/GetBuilder/GetJobBuilder/GetTriggerBuilder(s)`、`TryGet/TryAdd/TryUpdate/TryRemoveTrigger`、`TryUpdateDetail`、`Start/Pause/Run/Persist/Collate/Reload`、`ConvertToJSON`、`GetEnumerable`。 |
| `Scheduler.cs` | 字段定义（JobId/GroupName/JobDetail/Triggers/Factory/Logger/UseUtcTimestamp/JobLogger）；`internal sealed partial`，由工厂创建。 |
| `Scheduler.Methods.cs` | partial 方法：实现 `IScheduler` 全部成员，与 `SchedulerFactory.Shorthand` 协作触发持久化。 |
| `SchedulerModel.cs` | 对外可序列化模型（`JobDetail` + `Trigger[]`），用于看板/接口返回。 |

## For AI Agents

### Working in this directory
- `Scheduler` 是 `internal sealed`；外部代码只持有 `IScheduler`。新增公共能力时同时扩展接口与 `SchedulerModel`（如需 JSON 输出）。
- `Triggers` 是 `Dictionary<string, Trigger>`，多线程环境下读写需通过 `SchedulerFactory` 的工厂方法间接进行。
- `Factory/Logger/JobLogger` 为内部依赖项，由 `SchedulerFactory` 创建实例后赋值；构造时不要要求它们非空。

### Common patterns
- "Try*" 方法返回 `ScheduleResult` 而不是抛异常，外层据此区分 NotFound / NotIdentify / Exists / Succeed / Failed。
- `Reload()` 会调用工厂端 `CancelSleep`，让后台主机服务立即重新计算。

## Dependencies
### Internal
- `../Details/JobDetail`、`../Triggers/Trigger`、`../Factories/ISchedulerFactory`、`../Loggers/IScheduleLogger`。
### External
- `Microsoft.Extensions.Logging`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
