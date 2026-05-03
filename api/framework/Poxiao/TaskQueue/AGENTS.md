<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# TaskQueue

## Purpose
基于 `System.Threading.Channels` 的进程内后台任务队列。提供延迟入队、Cron 入队，并由后台 `BackgroundService` 出队执行（带重试与未察觉异常事件）。是 LIMS 后端用于异步化短任务（推送、轻量计算、I/O 触发）的首选机制；与 RabbitMQ/事件总线相比适合**单实例本地化**场景。

## Key Files
| File | Description |
|------|-------------|
| `TaskQueued.cs` | `[SuppressSniffer]` 静态门面：`Enqueue` / `EnqueueAsync(handler, delay or cron)`；从 `App.RootServices` 解析 `ITaskQueue`。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Builders/` | `TaskQueueOptionsBuilder`（`ChannelCapacity` / 异常处理委托） (see `Builders/AGENTS.md`) |
| `Dependencies/` | `ITaskQueue` 与基于 `Channel.CreateBounded` 的默认实现 (see `Dependencies/AGENTS.md`) |
| `Extensions/` | `AddTaskQueue` 服务集合扩展 (see `Extensions/AGENTS.md`) |
| `HostedServices/` | `TaskQueueHostedService` 后台主机（含 Retry + UnobservedTaskException） (see `HostedServices/AGENTS.md`) |
| `Internal/` | 日志分类标记类 (see `Internal/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 默认 `ChannelCapacity=3000`、`BoundedChannelFullMode.Wait`：高吞吐场景需评估生产者阻塞。
- 处理委托签名为 `Func<IServiceProvider, CancellationToken, ValueTask>`：必须用传入的 `serviceProvider` 创建作用域，不可捕获请求级 `IServiceProvider`。
- Cron 入队使用 `Poxiao.TimeCrontab.Crontab.Parse(...).GetSleepMilliseconds(...)` 一次性折算延迟，**不会重复触发**——周期任务请使用 `Schedule` 模块或自重新入队。
- 不适合跨进程/持久化任务，重启即失。

### Common patterns
- 接口 + 单例实现 + `BackgroundService` 消费 + 静态门面便利方法。

## Dependencies
### Internal
- `Poxiao.TimeCrontab`（Cron 解析）、`Poxiao.FriendlyException.Retry`（重试）、`App.RootServices`。
### External
- `System.Threading.Channels`、`Microsoft.Extensions.Hosting.BackgroundService`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
