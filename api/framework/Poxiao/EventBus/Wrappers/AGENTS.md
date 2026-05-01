<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Wrappers

## Purpose
事件处理程序运行时包装类。在 `EventBusHostedService` 启动期把每条 `[EventSubscribe]` 元数据 + 委托 + 排序 + 模糊匹配状态打包成可调度单元，存入 `ConcurrentDictionary` 字典。

## Key Files
| File | Description |
|------|-------------|
| `EventHandlerWrapper.cs` | `internal sealed`。字段：`EventId`、`Handler`（`Func<EventHandlerExecutingContext, Task>`）、`HandlerMethod`、`Attribute`、`Pattern`（启用模糊匹配时编译的 `Regex`）、`GCCollect`、`Order`。`ShouldRun(eventId)` 先判等再尝试正则匹配。 |

## For AI Agents

### Working in this directory
- 类型为 `internal` —— 仅 `EventBusHostedService` 与 `EventBusFactory`（动态订阅时）创建。
- 字典以 `EventHandlerWrapper` 同时作为 key 和 value（按引用相等）—— 同一方法注册两次会产生两个不同包装，意味着会被**触发两次**；动态订阅尤其要注意去重。
- `Order` 数值越大越先执行（`OrderByDescending(u => u.Value.Order)`）。

### Common patterns
- 模糊匹配开销：每次事件触发都要遍历所有 wrapper 调 `ShouldRun` —— 大量订阅 + 模糊匹配场景需关注 CPU。

## Dependencies
### Internal
- `EventSubscribeAttribute`、`EventHandlerExecutingContext`。
### External
- `System.Text.RegularExpressions`、`System.Reflection`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
