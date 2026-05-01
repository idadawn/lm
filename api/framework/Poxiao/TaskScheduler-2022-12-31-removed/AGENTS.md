<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# TaskScheduler-2022-12-31-removed

> **DEPRECATED / LEGACY**：整个目录及其全部子目录都标注为 `[Obsolete("The <TaskScheduler> will be removed on December 31, 2022. Please use the <Scheduler> instead.")]`。仓库保留仅作历史参考与编译兼容；**新代码请使用 `Poxiao.Schedule`（Scheduler）模块**。

## Purpose
旧版基于 `System.Timers.Timer` + 自实现 Cron 解析（`CronExpression`，参考 Hangfire/Cronos）的定时任务框架。通过 `[SpareTime]` 特性扫描 `ISpareTimeWorker` 实现类的方法，注册成 Interval 或 Cron 任务，由 `SpareTime` 静态调度器统一管理。

## Key Files
| File | Description |
|------|-------------|
| `SpareTime.cs` | 静态调度门面（≈900 行）：`Do` / `DoOnce` / `Start` / `Stop` / `Cancel` / `WorkerRecords` 字典缓存。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[SpareTime]` 特性 (see `Attributes/AGENTS.md`) |
| `Cron/` | 自实现 Cron 解析器与时区/日历辅助 (see `Cron/AGENTS.md`) |
| `Enums/` | 任务类型/状态/执行类型枚举 (see `Enums/AGENTS.md`) |
| `Extensions/` | `AddTaskScheduler` 反射扫描注册 (see `Extensions/AGENTS.md`) |
| `Handlers/` | IPCChannel 监听处理程序 (see `Handlers/AGENTS.md`) |
| `Internal/` | `SpareTimer` / `SpareTimerExecuter` / `WorkerRecord` (see `Internal/AGENTS.md`) |
| `Listeners/` | `ISpareTimeListener` 监听接口 (see `Listeners/AGENTS.md`) |
| `Workers/` | `ISpareTimeWorker` 标记接口 (see `Workers/AGENTS.md`) |

## For AI Agents

### Working in this directory
- **不要在此添加新功能**。Bug 修复请权衡是否直接迁移到 `Poxiao.Schedule`。
- 所有公开类型与方法均带 `[Obsolete]`；引用方将看到 CS0618 警告，这是预期。
- 目录名带日期前缀（`-2022-12-31-removed`），保留以保护已存在的引用方编译。删除该目录前需全仓搜索 `Poxiao.TaskScheduler` 确认无依赖。

### Common patterns
- `[SuppressSniffer]` + `[Obsolete]` 双标注；反射扫描 `ISpareTimeWorker` 实现以注册 `[SpareTime]` 方法。

## Dependencies
### Internal
- `Poxiao.Templates.Extensions.Render`、`Poxiao.IPCChannel`（监听管道）、`App.EffectiveTypes`。
### External
- `System.Timers.Timer`、`System.Threading.Channels`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
