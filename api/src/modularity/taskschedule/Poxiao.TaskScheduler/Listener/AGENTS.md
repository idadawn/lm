<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Listener

## Purpose
内置 `IJob` 监听器集合：随程序启动由 `Poxiao.Schedule` 自动扫描 `[JobDetail]` 注册到调度器，并写入 `JobDetails`/`JobTriggers` 持久化。

## Key Files
| File | Description |
|------|-------------|
| `ScheduleJob.cs` | `job_schedule`：每日刷新日程推送；遍历全局租户缓存，触发 `IScheduleService.AddPushTaskQueue` |
| `OnlineUserJob.cs` | `job_onlineUser`：程序重启清理 Redis 中 `CACHEKEYONLINEUSER` 在线用户键 |
| `SpareTimeDemo.cs` | 框架 `SpareTime` 示例 / 兜底任务 |

## For AI Agents

### Working in this directory
- 实现 `IJob.ExecuteAsync`：必须 `using var scope = _serviceProvider.CreateScope()` 取作用域服务，避免长期持有数据库连接。
- 注解组合：`[JobDetail("唯一key", GroupName, Concurrent)]` + 频率注解（`[PeriodSeconds]` / `[Daily]` / `[Cron]`），`Concurrent = false` 表示串行执行。
- 不要在 Job 中抛异常打断调度循环，使用 `Log.Error` 上报。

### Common patterns
- 日志统一使用 `Poxiao.Logging.Log.Information("【{Now}】...")` 中文消息体。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Manager.ICacheManager`、`Poxiao.Infrastructure.Const.CommonConst`
- `Poxiao.Systems.Interfaces.System.IScheduleService`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
