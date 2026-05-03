<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Servers

## Purpose
作业集群协调接口 `IJobClusterServer`。Schedule 不内置选主，集群仅负责"故障转移"：每个节点把自身状态广播出去，被动节点 `WaitingForAsync` 阻塞直至主节点宕机，然后接管调度。

## Key Files
| File | Description |
|------|-------------|
| `IJobClusterServer.cs` | 四个生命周期钩子：`Start(JobClusterContext)`、`WaitingForAsync(JobClusterContext)`、`Stop(JobClusterContext)`、`Crash(JobClusterContext)`。 |

## For AI Agents

### Working in this directory
- 注册为单例 (`services.AddSingleton<IJobClusterServer, MyClusterServer>()`)，未注册时 Schedule 退化为单实例运行 —— 这是默认。
- `WaitingForAsync` 阻塞会阻断 `ScheduleHostedService.ExecuteAsync` 的预加载，实现时需要支持 `CancellationToken` 取消（通过 hosted service 的 `stoppingToken`，目前接口未传入，请通过外部信号控制）。
- 实现示例：基于 Redis 锁/PubSub、ZooKeeper、数据库心跳等。`ClusterId` 来自 `ScheduleOptionsBuilder.ClusterId`。

### Common patterns
- `Crash(...)` 在 `Dispose` 路径触发；不要在该方法中执行长 I/O，避免阻塞应用关闭。

## Dependencies
### Internal
- `../Contexts/JobClusterContext`、`../Constants/ClusterStatus`。
### External
- BCL。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
