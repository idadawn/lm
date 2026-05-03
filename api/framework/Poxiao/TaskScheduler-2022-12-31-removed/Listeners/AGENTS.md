<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Listeners

> **DEPRECATED / LEGACY**：旧版 TaskScheduler 监听器接口。新代码请使用 `Poxiao.Schedule` 的 `IJobMonitor` 等接口。

## Purpose
旧调度器对外暴露的监听器抽象。业务实现 `ISpareTimeListener` 并以单例注册到 DI，即可在每次定时器状态变更时收到回调（开始/前置/成功/失败/停止/取消）。

## Key Files
| File | Description |
|------|-------------|
| `ISpareTimeListener.cs` | 单方法接口 `Task OnListener(SpareTimerExecuter executer)`，带 `[Obsolete]`。 |

## For AI Agents

### Working in this directory
- 监听器实现需注册为单例：触发端通过 `App.GetService<ISpareTimeListener>(App.RootServices)` 解析。
- 实现内不要再 `await` 长 IO，避免阻塞 IPC 通道处理；建议把工作再发给 `TaskQueue`。

### Common patterns
- `Status` 通过 `SpareTimerExecuter.Status` 数字区分，使用 switch 处理。

## Dependencies
### Internal
- 父级 `Internal/SpareTimerExecuter`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
