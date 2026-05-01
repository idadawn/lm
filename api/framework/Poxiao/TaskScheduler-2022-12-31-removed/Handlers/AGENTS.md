<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Handlers

> **DEPRECATED / LEGACY**：旧版 TaskScheduler 监听处理程序。新代码请使用 `Poxiao.Schedule` 监听器机制。

## Purpose
旧调度器的"监听器解耦"实现：通过 `Poxiao.IPCChannel` 提供的 `ChannelHandler<T>` 把内部触发事件异步推到用户实现的 `ISpareTimeListener`。

## Key Files
| File | Description |
|------|-------------|
| `SpareTimeListenerChannelHandler.cs` | `internal sealed`，覆写 `InvokeAsync(SpareTimerExecuter)`：从 DI 中取 `ISpareTimeListener`，若已注册则调用 `OnListener(executer)`。 |

## For AI Agents

### Working in this directory
- Handler 通过 `Poxiao.IPCChannel` 与定时器执行链解耦——不要把它改为同步调用。
- 实现 `ISpareTimeListener` 即可订阅生命周期事件（开始/前置/成功/失败/停止/取消，对应 `SpareTimerExecuter.Status`）。

### Common patterns
- IPC 通道桥接：发布端写入 `SpareTimerExecuter`，消费端在此回调用户监听器。

## Dependencies
### Internal
- `Poxiao.IPCChannel.ChannelHandler<>`、本目录父级 `Listeners/ISpareTimeListener`、`Internal/SpareTimerExecuter`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
