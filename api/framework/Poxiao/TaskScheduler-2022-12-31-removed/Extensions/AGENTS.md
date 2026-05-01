<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

> **DEPRECATED / LEGACY**：旧版 TaskScheduler 注册扩展。新代码请使用 `Poxiao.Schedule.AddSchedule`。

## Purpose
`AddTaskScheduler` 服务集合扩展：通过反射扫描所有 `ISpareTimeWorker` 实现类、收集贴有 `[SpareTime]` 的 `(SpareTimer, long)` 签名方法，按 Interval/Cron + DoOnce/异步同步分支调用 `SpareTime.Do` / `DoOnce` 完成注册。

## Key Files
| File | Description |
|------|-------------|
| `TaskSchedulerServiceCollectionExtensions.cs` | 一次性扫描 `App.EffectiveTypes`，每个 Worker 类用 `Activator.CreateInstance` 实例化（**注意：非 DI 实例**），用 `Delegate.CreateDelegate` 把方法绑定为 `Action`/`Func<,,Task>` 委托后注册到 `SpareTime`。 |

## For AI Agents

### Working in this directory
- Worker 实例由 `Activator.CreateInstance` 构建——**无法注入依赖**。这是迁移到 `Poxiao.Schedule` 的关键动机之一。
- 方法签名固定为 `(SpareTimer timer, long count)`；不同签名会被忽略且不报错。
- 不要在此目录扩充新逻辑，迁移路径见 `Poxiao.Schedule`。

### Common patterns
- 反射 + 委托缓存的批量注册，逻辑写死 4×2 分支（Interval/Cron × Sync/Async × DoOnce）。

## Dependencies
### Internal
- `App.EffectiveTypes`、`Poxiao.Extensions.IsAsync()`、`SpareTime` 静态调度器。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
