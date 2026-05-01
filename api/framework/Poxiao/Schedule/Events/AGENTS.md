<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Events

## Purpose
Schedule 调度器对外抛出的事件参数定义。当前仅含 `SchedulerEventArgs`，承载 `JobDetail`，用于 `SchedulerFactory.OnChanged` 与看板 SSE 通道的变更通知。

## Key Files
| File | Description |
|------|-------------|
| `SchedulerEventArgs.cs` | `EventArgs` 派生类型，构造时接收 `JobDetail`，仅暴露只读 `JobDetail` 属性。 |

## For AI Agents

### Working in this directory
- 不要在事件内部携带可变状态；订阅者（如看板的 `BlockingCollection<JobDetail>` 队列）会按引用直接转发。
- 若需引入新事件（例如 `TriggerChanged`），保持 `EventArgs` 派生 + 只读属性的简单约定，并同步更新 `ISchedulerFactory.OnChanged` 等出口签名。

### Common patterns
- 通过 `OnChanged?.Invoke(this, new(jobDetail))` 触发，被 `Dashboard/backend/ScheduleUIMiddleware` 推送为 SSE 帧。

## Dependencies
### Internal
- `../Details/JobDetail`。
### External
- BCL `System.EventArgs`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
