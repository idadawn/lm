<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Persistences

## Purpose
作业调度持久化插件接口。允许把内存中的作业/触发器状态写入数据库或外部存储；未注册时调度器纯内存运行，注册后由 `SchedulerFactory` 在后台线程消费 `BlockingCollection<PersistenceContext>` 调用本接口。

## Key Files
| File | Description |
|------|-------------|
| `IJobPersistence.cs` | 四个钩子：`Preload()` 启动时返回 `SchedulerBuilder` 集合、`OnLoading(builder)` 装载前改写、`OnChanged(PersistenceContext)` 作业变更、`OnTriggerChanged(PersistenceTriggerContext)` 触发器变更。 |

## For AI Agents

### Working in this directory
- 注册：`services.AddSingleton<IJobPersistence, MyPersistence>()`；同时通过 `JobDetailOptions.ConvertToSQL` / `TriggerOptions.ConvertToSQL` 自定义 SQL 生成（默认实现按 `NamingConventions` 输出）。
- `Preload` 在 `SchedulerFactory.Preload` 内单次调用，异常不会终止启动 —— 但 `PreloadCompleted` 仍会标记，注意外部需要捕获并补偿。
- 持久化方法不会被 `ScheduleHostedService` 主线程直接调用，运行在工厂内部 `_processQueueTask` 长时任务上；实现需保证幂等与可中断。

### Common patterns
- `OnLoading` 常用于把数据库的 JobDetail 反序列化为 `SchedulerBuilder`；返回 null 时使用初始 builder。
- 写入失败请抛异常，`SchedulerFactory.ProcessQueue` 会按消息维度记录 ERROR 日志而不停止队列。

## Dependencies
### Internal
- `../Contexts/PersistenceContext`、`../Contexts/PersistenceTriggerContext`、`../Triggers/`、`../Details/JobDetail`。
### External
- BCL；具体存储引擎由实现类决定（项目内常见使用 SqlSugar）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
