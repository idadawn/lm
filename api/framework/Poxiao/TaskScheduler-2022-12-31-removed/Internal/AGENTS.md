<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

> **DEPRECATED / LEGACY**：旧版 TaskScheduler 内部类型。新代码请使用 `Poxiao.Schedule`。

## Purpose
旧调度器内部使用的定时器封装、执行状态对象与并发记录。

## Key Files
| File | Description |
|------|-------------|
| `SpareTimer.cs` | 继承 `System.Timers.Timer`，附加 `WorkerName`、`Type`、`Description`、`Status`、`ExecuteType`、`Exception` 字典与 `Tally`；构造时把自身写入 `SpareTime.WorkerRecords`，重名抛 `InvalidOperationException`。 |
| `SpareTimerExecuter.cs` | 包装 `(SpareTimer Timer, int Status)`，状态码：`0=开始 / 1=前置 / 2=成功 / 3=失败 / -1=停止 / -2=取消`。 |
| `WorkerRecord.cs` | 并发记录结构：`Interlocked` / `Tally` / `Timer`，配合 `SpareTime.WorkerRecords` 字典实现并发去重。 |

## For AI Agents

### Working in this directory
- 类型为 `internal sealed`/`public sealed`：业务侧仅消费 `SpareTimerExecuter`（监听器参数），其余内部勿反射访问。
- `Tally` / `Interlocked` 配合 `Interlocked.Exchange` 实现串行/并行执行控制，重构请保留原子语义。

### Common patterns
- `Status` 用魔法数字而非枚举（API 已稳定，迁移时再改）。

## Dependencies
### External
- `System.Timers.Timer`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
