<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
TaskQueue 模块对外暴露的 DI 注册入口。把 `ITaskQueue` 注册为单例，并将 `TaskQueueHostedService` 作为后台主机服务挂载到通用 .NET Host。

## Key Files
| File | Description |
|------|-------------|
| `TaskQueueServiceCollectionExtensions.cs` | 两个 `AddTaskQueue` 重载：一种接收 `Action<TaskQueueOptionsBuilder>`、一种接收已构建的 Builder。注册流程：`Build()` → `AddSingleton<ITaskQueue>(_ => new TaskQueue(capacity))` → `AddHostedService(...)` 并订阅 `UnobservedTaskExceptionHandler`。 |

## For AI Agents

### Working in this directory
- 命名空间为 `Microsoft.Extensions.DependencyInjection`，便于宿主直接发现，**不要修改**。
- `ITaskQueue` 通过工厂注册为单例，其 `BoundedChannel` 进程级共享。
- HostedService 通过 `ActivatorUtilities.CreateInstance` 构造以注入 `ILogger` / `IServiceProvider` / `ITaskQueue`，新增依赖直接加构造函数即可。
- 不要在此直接 `services.AddHostedService<TaskQueueHostedService>()`——会丢失 `UnobservedTaskException` 订阅。

### Common patterns
- `[SuppressSniffer]` 静态扩展类 + `private static AddInternalService` 拆分注册细节。

## Dependencies
### Internal
- `Poxiao.TaskQueue.TaskQueueOptionsBuilder` / `ITaskQueue` / `TaskQueue` / `TaskQueueHostedService`。
### External
- `Microsoft.Extensions.DependencyInjection`、`Microsoft.Extensions.Hosting`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
