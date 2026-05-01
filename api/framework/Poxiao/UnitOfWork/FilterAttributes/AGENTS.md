<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FilterAttributes

## Purpose
`[UnitOfWork]` 特性实现 —— 同时是 Action Filter (`IAsyncActionFilter`) 与 Page Handler Filter (`IAsyncPageFilter`)。它把请求体 `next()` 包在 try/catch 中，调用 `IUnitOfWork.BeginTransaction → CommitTransaction/RollbackTransaction → OnCompleted`，可选叠加 `TransactionScope` 分布式事务。

## Key Files
| File | Description |
|------|-------------|
| `UnitOfWorkAttribute.cs` | `Order=9999`（晚于绝大多数 Filter）；`EnsureTransaction`、`UseAmbientTransaction`、`TransactionScope`/`IsolationLevel`/`Timeout`/`AsyncFlow` 多选项；MiniProfiler 输出 unitOfWork 类别消息。 |

## For AI Agents

### Working in this directory
- 默认隔离级别 `ReadCommitted`，若业务需要 Serializable 必须显式设置且评估死锁影响。
- `OnPageHandlerSelectionAsync` 返回 CompletedTask 留空——别在此处实现绑定逻辑，会破坏 Razor Pages 流程。
- 若 Blazor Server (`HandlerMethod == null`)，特性会直接 next 不开事务，写库逻辑须自行控制事务。
- 不要把该特性挂在 Controller 类上做粗放事务包装；除非全部 Action 均写库且耗时短，否则会拖长事务持有时间。

### Common patterns
- 异常路径下回滚 + `logger.LogError(ex, "Transaction Failed.")` + Rollback (Ambient)。
- `dynamic resultContext` 用于同时兼容 `ActionExecutedContext` 与 `PageHandlerExecutedContext` 两种 next 返回。

## Dependencies
### Internal
- `Poxiao.DatabaseAccessor.IUnitOfWork`、`Poxiao.UnitOfWork.Internal.UnitOfWork`（日志分类）。
### External
- `Microsoft.AspNetCore.Mvc.Filters`、`System.Transactions`、`Microsoft.Extensions.Logging`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
