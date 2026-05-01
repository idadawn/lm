<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dependencies

## Purpose
TaskQueue 的核心契约 `ITaskQueue` 与基于 `Channel.CreateBounded` 的默认实现。是任务"入队/出队"的真正引擎。

## Key Files
| File | Description |
|------|-------------|
| `ITaskQueue.cs` | 定义 4 个入队重载（同步/异步 × 延迟/Cron）+ 1 个 `DequeueAsync(cancellationToken)`。 |
| `TaskQueue.cs` | `internal sealed partial class TaskQueue`：用 `BoundedChannelOptions { FullMode = Wait }` 创建有限容量通道；同步入队通过包装成异步委托 + `_ = ...AsTask()` 派发；Cron 入队调用 `Crontab.Parse(...).GetSleepMilliseconds(now)` 折算为毫秒延迟入队。 |

## For AI Agents

### Working in this directory
- 实现是 `internal sealed`：业务勿直接 `new`，通过 `App.GetRequiredService<ITaskQueue>` 拿到单例。
- 同步 `Enqueue` 内部仍是 fire-and-forget（`_ = ...AsTask()`），异常仅由 HostedService 捕获——不要假设入队同步抛错。
- Cron 入队是**单次延迟**实现（`GetSleepMilliseconds` 仅算下一次），需要循环触发的任务请使用 `Schedule` 模块或在 handler 内自重新入队。
- 通道为 `Bounded` + `Wait`：当容量满时生产者会阻塞，合理评估容量或捕获取消。

### Common patterns
- `Channel<Func<IServiceProvider, CancellationToken, ValueTask>>` 持有任务委托而非数据，直接消费 DI。
- `partial` 类便于按文件维度拆分（当前仅一个文件，预留扩展位）。

## Dependencies
### Internal
- `Poxiao.TimeCrontab.Crontab` / `CronStringFormat`。
### External
- `System.Threading.Channels`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
