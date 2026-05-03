<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.TaskScheduler

## Purpose
定时任务实现工程。承载 `TimeTaskService` 动态 API（`api/scheduletask`）以及 `Listener/` 下的内置 `IJob` 实现。通过 `ISchedulerFactory` 与 `Polly` 重试组合提供任务执行能力。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.TaskScheduler.csproj` | 工程文件，引用 Common.Core / Systems.Interfaces / 自身 Interfaces |
| `TimeTaskService.cs` | 主服务：CRUD + 启停 + 立即执行；调度 `IDataInterfaceService` / 本地方法（反射） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Listener/` | 内置 `IJob` 实现（程序启动时自动注册）(see `Listener/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新增本地任务：在 `Listener/` 添加实现 `IJob` 的类，标注 `[JobDetail("job_xxx", ...)]` + `[PeriodSeconds(...)]` 或 `[Daily]`，避免直接 `new SpareTime`。
- HTTP/脚本任务由 `TimeTaskService.PerformJob` 分派；执行类型字段 `F_EXECUTETYPE` = "1" 数据接口、"3" 本地方法。
- `ICacheManager` 用作分布式锁键缓存（`CommonConst.CACHEKEY*`），不要写本地静态字典。

### Common patterns
- `[ApiDescriptionSettings(Tag = "TaskScheduler", Name = "scheduletask", Order = 220)]`、`[Route("api/[controller]")]`。

## Dependencies
### Internal
- `Poxiao.Schedule`（`ISchedulerFactory`/`IJob`/`JobExecutingContext`）
- `Poxiao.Systems.Interfaces.System.IDataInterfaceService` / `IDataBaseManager`
### External
- Polly、Aop.Api（支付宝 SDK 引用，历史遗留）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
