<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Handlers

## Purpose
`IPCChannel` 通道消息处理器的抽象基类目录。`ChannelContext<TMessage,THandler>` 在读取到消息后，用 `Activator.CreateInstance<THandler>()` 实例化此处的 `ChannelHandler<TMessage>` 并调用其 `InvokeAsync`。

## Key Files
| File | Description |
|------|-------------|
| `ChannelHandler.cs` | 抽象基类 `ChannelHandler<TMessage>`，仅声明 `Task InvokeAsync(TMessage message)`；业务模块继承此类编写具体的消费逻辑（如写入数据库日志、发送 IM 通知等）。 |

## For AI Agents

### Working in this directory
- 派生类必须无参公共构造（被 `Activator.CreateInstance` 调用）。
- 处理器内不要抛非业务异常——上层 `Retry.InvokeAsync` 配置 `finalThrow:false`，3 次失败后会被吞掉。如需可观测性，请在 handler 内显式 `Log.Error`。
- 处理器是**每条消息一次性实例**，不要在派生类放可变成员状态。

### Common patterns
- 单方法抽象类（template method）。

## Dependencies
### Internal
- 由 `Poxiao.IPCChannel.ChannelContext<,>` 反射创建并消费。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
