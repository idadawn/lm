<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
工作单元的 DI 注册扩展。把业务侧实现的 `IUnitOfWork`（通常是 SqlSugar 的事务包装类）按 Transient 注入，使 `[UnitOfWork]` 过滤器可在请求作用域内解析到具体事务实现。

## Key Files
| File | Description |
|------|-------------|
| `UnitOfWorkServiceCollectionExtensions.cs` | 提供 `AddUnitOfWork<TUnitOfWork>` 重载（IServiceCollection / IMvcBuilder）；内部 `AddTransient<IUnitOfWork, TUnitOfWork>()`。 |

## For AI Agents

### Working in this directory
- 必须在 `Program.cs` 中显式调用一次 `services.AddUnitOfWork<XxxUnitOfWork>()`；缺失会让 `[UnitOfWork]` Action 在 `GetRequiredService<IUnitOfWork>()` 处抛 InvalidOperationException。
- Transient 生命周期是必须的——每次请求需要独立的 Tran/连接，不要改成 Scoped/Singleton。

### Common patterns
- 命名空间归在 `Microsoft.Extensions.DependencyInjection`，便于 Startup 类无需额外 using。

## Dependencies
### Internal
- `Poxiao.DatabaseAccessor.IUnitOfWork`。
### External
- `Microsoft.Extensions.DependencyInjection`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
