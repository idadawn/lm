<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Triggers

## Purpose
触发器（Trigger）的核心定义与内置实现。`Trigger` 抽象类描述触发元数据、运行计数、重试与 Timeline；`CronTrigger` 基于 `Poxiao.TimeCrontab` 解析 cron，`PeriodTrigger` 按毫秒间隔运行；`Triggers` 静态类提供面向用户的便捷构建 API。

## Key Files
| File | Description |
|------|-------------|
| `Trigger.cs` | partial 主体，声明 TriggerId/JobId/Status/StartTime/EndTime/Last|NextRunTime/NumberOfRuns/MaxNumberOfRuns/NumberOfErrors/MaxNumberOfErrors/NumRetries/RetryTimeout/StartNow/RunOnStart/ResetOnlyOnce/Result/ElapsedTime/UpdatedTime 与运行时字段（`RuntimeTriggerType`、`RuntimeTriggerArgs`、`Timelines` 队列）。 |
| `Trigger.Methods.cs` | partial 实现：`GetNextOccurrence`、`Increment`/`IncrementErrors`、`SetStatus`、`CurrentShouldRun`/`NextShouldRun`、`CheckAndFixNextOccurrence`、`RecordTimeline`、`ConvertToJSON`/`ConvertToSQL`/`ConvertToMonitor`、`GetTimelines`。 |
| `CronTrigger.cs` | Cron 表达式触发器；构造接收 `(string schedule, object args)`，按 `args` 是 int/`CronStringFormat`/object[] 切换 `Crontab.Parse` 路径。 |
| `PeriodTrigger.cs` | 毫秒周期触发器，最小间隔 100ms，`GetNextOccurrence` 用 `startAt.AddMilliseconds`，`ToString` 以 ms/s/min/h 自动单位化。 |
| `Triggers.cs` | 静态门面：`Period/PeriodSeconds/PeriodMinutes/PeriodHours`、`Cron`、`Secondly/Minutely/Hourly/Daily/Monthly/Weekly/Yearly/Workday` 与 `*At`、`Create<TTrigger>(args)`、`From(json)`、`Clone(...)`。 |
| `TriggerTimeline.cs` | 单次执行记录（`NumberOfRuns/LastRunTime/NextRunTime/Status/Result/ElapsedTime/CreatedTime`），调度器只保留最近 10 条。 |
| `TriggerOptions.cs` | 全局触发器配置，目前承载 `ConvertToSQL` 重写委托。 |

## For AI Agents

### Working in this directory
- 自定义触发器：继承 `Trigger`，实现 `GetNextOccurrence(DateTime)`，并通过 `Triggers.Create(typeof(MyTrigger), args)` 注册；构造函数签名要与 `RuntimeTriggerArgs` 一致以便反射重建。
- 不要把构造的 Cron 字符串直接序列化到 `Args` 之外的字段——持久化层重新加载时只反序列化 `Args` JSON。
- `ResetOnlyOnce` 默认 `true`，避免最大一次性触发器在持久化重启后被跳过；移除前请评估副作用。

### Common patterns
- `Status` 流转由 `ScheduleHostedService.BackgroundProcessing` 与 `Trigger.Increment*` 共同驱动；不要外部直接置位。
- 字符串化输出 `<JobId TriggerId> <Schedule|Period> <Description摘要> <Runs>ts`，被日志直接消费。

## Dependencies
### Internal
- `../Constants/TriggerStatus`、`../Constants/PersistenceBehavior`、`../Internal/Penetrates`、`../../TimeCrontab/`。
### External
- `System.Text.Json.Serialization`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
