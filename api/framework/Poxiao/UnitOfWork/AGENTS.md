<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# UnitOfWork

## Purpose
工作单元（事务）封装模块。通过 `[UnitOfWork]` 特性把 Controller Action / Razor Page Handler 的请求生命周期包成一次事务：Action 抛异常自动回滚、正常返回自动提交，并可启用分布式 `TransactionScope` 跨数据源事务。是检测室系统中所有写库 Action 的标准事务边界。

## Key Files
| File | Description |
|------|-------------|
| `IUnitOfWork.cs` | 抽象事务回调接口：`BeginTransaction` / `CommitTransaction` / `RollbackTransaction` / `OnCompleted`，由数据库适配层（如 SqlSugar 集成）实现。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | `AddUnitOfWork<TUnitOfWork>` DI 注册扩展 (see `Extensions/AGENTS.md`) |
| `FilterAttributes/` | `[UnitOfWork]` Action 过滤器特性 (see `FilterAttributes/AGENTS.md`) |
| `Internal/` | 日志分类占位类 (see `Internal/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 命名空间使用 `Poxiao.DatabaseAccessor`（沿用 Furion 历史命名），不要改回 `Poxiao.UnitOfWork`，否则破坏现有 Module 的 using。
- 业务模块（lab、system、workflow 等）的 Service 不要直接 `BeginTran`，应在 Controller Action 上挂 `[UnitOfWork]`。
- 需要分布式事务时 `[UnitOfWork(UseAmbientTransaction = true)]`，并校准 `TransactionTimeout` 与 SqlSugar 连接字符串中的 `Enlist`。

### Common patterns
- `IUnitOfWork` 注册为 `Transient`（每次请求新建），由 `UnitOfWorkAttribute` 通过 `RequestServices.GetRequiredService<IUnitOfWork>()` 解析。
- 该模块仅定义骨架，具体 SqlSugar 事务实现一般落在 `api/src/modularity/` 数据库基类中。

## Dependencies
### Internal
- 由 Controller / Service 通过 `[UnitOfWork]` 使用。
### External
- `Microsoft.AspNetCore.Mvc.Filters`、`System.Transactions`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
