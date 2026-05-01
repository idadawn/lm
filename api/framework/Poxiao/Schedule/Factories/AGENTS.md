<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Factories

## Purpose
作业计划工厂 `ISchedulerFactory` / `SchedulerFactory` 的接口定义与实现，是 Schedule 模块的中枢。维护内存中的 `Scheduler` 集合、持久化消息队列、唤醒/休眠 token、HTTP/动态作业入口与 `OnChanged` 通知。

## Key Files
| File | Description |
|------|-------------|
| `ISchedulerFactory.Exports.cs` | 公共 API：`GetJobs/TryGetJob/TrySaveJob/TryAddJob*`（含 HTTP/动态/泛型/并发组合超载）、`UpdateJob`、`RemoveJob`、`StartAll/PauseAll/PersistAll/CollateAll/RunJob`。 |
| `ISchedulerFactory.Internal.cs` | 内部成员：休眠 token 管理、`Shorthand` 持久化入队、`Preload`、`OnChanged` 事件、`GetCurrentRunJobs` 计算下一批触发。 |
| `SchedulerFactory.Exports.cs` | 公共 API 的实现（构建 `Scheduler` 实例、协调 `IJobPersistence`、广播 `OnChanged`）。 |
| `SchedulerFactory.Internal.cs` | 内部实现：`ConcurrentDictionary<string, Scheduler>`、`BlockingCollection<PersistenceContext>` 队列、`ProcessQueue` 长时任务、`SleepAsync/CancelSleep`、GC 节流（`GC_COLLECT_INTERVAL_MILLISECONDS`）。 |

## For AI Agents

### Working in this directory
- 工厂被注册为单例；新增字段需考虑线程安全（用 `ConcurrentDictionary` / `BlockingCollection`）。
- `Shorthand(...)` 是唯一的持久化入口，`DynamicExecuteAsync != null` 的作业不入队（避免持久化运行时委托）。
- `Preload` 失败仍要标记 `PreloadCompleted = true` 并清空 `_schedulerBuilders`，否则会内存泄漏。
- 不要在 `Dispose` 中阻塞太久；当前限定 `_processQueueTask?.Wait(1500)` 留 1.5s 缓冲。

### Common patterns
- partial 类按 Exports/Internal 拆分，便于审阅"对外契约 vs 内部实现"。
- 唤醒机制：`_sleepCancellationTokenSource.Cancel()` + 在 token 注册 GC 回收回调。

## Dependencies
### Internal
- `../Schedulers/Scheduler`、`../Contexts/PersistenceContext`、`../Constants/ScheduleResult`、`../Persistences/IJobPersistence`、`../Loggers/IScheduleLogger`。
### External
- `Microsoft.Extensions.DependencyInjection.IServiceProvider`、`System.Collections.Concurrent`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
