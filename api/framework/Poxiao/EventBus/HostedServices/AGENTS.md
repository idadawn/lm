<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HostedServices

## Purpose
事件总线后台主机服务 —— 整个 EventBus 的执行心脏。从 `IEventSourceStorer` 持续读取事件源，匹配订阅，按 `Order` 排序后并行调度处理程序。

## Key Files
| File | Description |
|------|-------------|
| `EventBusHostedService.cs` | 继承 `BackgroundService`。启动时反射扫描所有 `IEventSubscriber` 实例，把贴 `[EventSubscribe]` 的方法用 `MethodInfo.CreateDelegate` 转为 `Func<EventHandlerExecutingContext, Task>` 并装入 `EventHandlerWrapper`。`ExecuteAsync` 循环读取事件，分流：`EventSubscribeOperateSource` 走 `ManageEventSubscribers`（动态增删）；普通事件用 `Parallel.ForEach + taskFactory.StartNew` 多线程派发。每个处理程序独立计时、调用 `IEventHandlerMonitor` 钩子、走 `Retry.InvokeAsync`（无自定义执行器时）或 `IEventHandlerExecutor`，可选触发 `GC.Collect`（默认 3 秒间隔）。捕获到的异常被 `UnobservedTaskException` 事件转发。 |

## For AI Agents

### Working in this directory
- 类标记 `internal sealed` —— 不可继承，业务通过 `EventBusOptionsBuilder` 提供的扩展点（Monitor / Executor / FallbackPolicy）注入行为。
- `Parallel.ForEach` 中 `taskFactory.StartNew` 返回的 `Task` 未被 await —— 处理程序异常通过 `_logger.LogError` + `UnobservedTaskException` 暴露，不会中断循环。
- `_eventHandlers` 是 `ConcurrentDictionary`，键值同对象 `EventHandlerWrapper`，所以**同一方法多次注册不会去重**（按引用相等）。

### Common patterns
- 模糊匹配：处理程序级 `FuzzyMatch` 覆盖全局；启用时把 `EventId` 编译为 `Regex(RegexOptions.Singleline)`。
- GC 节流：`LastGCCollectTime` 限制最快 3 秒一次（`GC_COLLECT_INTERVAL_SECONDS`）。

## Dependencies
### Internal
- `IEventSourceStorer`、`IEventSubscriber`、`IEventHandlerMonitor`、`IEventHandlerExecutor`、`IEventFallbackPolicy`、`EventHandlerWrapper`、`Retry`（`Poxiao.FriendlyException`）。
### External
- `Microsoft.Extensions.Hosting.BackgroundService`、`System.Threading.Channels`、`System.Text.RegularExpressions`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
