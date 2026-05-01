<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# IPCChannel

## Purpose
进程内通信（Producer/Consumer）通道封装，基于 `System.Threading.Channels`。提供有界/无界 Channel 的懒加载单例、长驻读取线程，以及 `AsyncLocal<T>` 风格的 `CallContext`，供检测室系统在请求处理之外进行异步消息扇出（如日志写入、事件分发等）。

## Key Files
| File | Description |
|------|-------------|
| `ChannelContext.cs` | 泛型 `ChannelContext<TMessage,THandler>`，懒加载 `UnBoundedChannel`（无限）和 `BoundedChannel`（默认容量 1000，FullMode=Wait），启动 LongRunning 读取任务，对每条消息用 `Retry.InvokeAsync` 重试 3 次（每次 1s）调用 `THandler.InvokeAsync`。 |
| `CallContext.cs` | 基于 `ConcurrentDictionary<string, AsyncLocal<T>>` 的线程异步流共享上下文（泛型 + 非泛型两份）；注释明确警告内存只增不减、推荐直接用 `AsyncLocal`。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Handlers/` | 消息处理器抽象基类 (see `Handlers/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新增 Channel 类型请保持懒加载 + `StartReader` 模式；不要直接暴露可变 `Channel<T>` 字段。
- `CallContext` 仅作兼容用途；新代码应直接使用 `AsyncLocal<T>`，避免无释放的字典内存增长。
- `THandler` 通过 `Activator.CreateInstance<THandler>()` 创建，故必须保证无参公共构造函数。

### Common patterns
- `Lazy<Channel<T>>` + 私有构造 + 静态属性暴露。
- 消息消费链：`reader.WaitToReadAsync` → `TryRead` → `Task.Start`（并行非等待） → `Retry.InvokeAsync(handler)`。

## Dependencies
### Internal
- `Poxiao.FriendlyException.Retry`（重试工具）。

### External
- `System.Threading.Channels`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
