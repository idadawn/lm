<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# taskschedule

## Purpose
定时任务调度模块根目录。基于 `Poxiao.Schedule`（Sundial 风格）调度框架，支持本地方法（`IJob` 注解 `[JobDetail]/[PeriodSeconds]/[Daily]`）、HTTP 接口任务、脚本任务三类执行方式，并通过 `JobDetails`/`JobTriggers` 持久化租户多实例。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.TaskScheduler/` | 服务实现（含 `TimeTaskService` API + `Listener/` 内置 Job）(see `Poxiao.TaskScheduler/AGENTS.md`) |
| `Poxiao.TaskScheduler.Entitys/` | 实体（`BASE_TIMETASK*`/`JobDetails`/`JobTriggers`）、DTO、Mapper、Model (see `Poxiao.TaskScheduler.Entitys/AGENTS.md`) |
| `Poxiao.TaskScheduler.Interfaces/` | `ITimeTaskService` 抽象 (see `Poxiao.TaskScheduler.Interfaces/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 数据接口任务会**手动**创建一条 `JobDetails`；本地任务在程序启动时**自动**注册（依赖 `[JobDetail]` 反射）。两类都依赖框架自动生成 `JobTriggers`。
- 持久化使用 `[Tenant("Poxiao-Job")]` 单独租户库存放 `JobDetails`/`JobTriggers`，不要混入业务库。

### Common patterns
- 业务任务表 `BASE_TIMETASK`（CLDEntityBase 全大写约定），日志表 `BASE_TIMETASKLOG`（OEntityBase）。

## Dependencies
### Internal
- `Poxiao.Schedule`（框架）、`Poxiao.Systems.Interfaces.System`（数据接口/脚本调用）
### External
- StyleCop.Analyzers

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
