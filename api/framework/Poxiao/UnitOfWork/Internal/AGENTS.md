<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
工作单元日志辅助类。`ILogger<UnitOfWork>` 在 `UnitOfWorkAttribute` 中作为日志类目占位，让事务相关日志统一带 `System.Logging.UnitOfWork` 类别名，方便在 Serilog/Sink 中按 SourceContext 过滤事务异常。

## Key Files
| File | Description |
|------|-------------|
| `Logging.cs` | 仅声明 `internal sealed class System.Logging.UnitOfWork` 占位类；无实际成员。 |

## For AI Agents

### Working in this directory
- 不要把它当作业务类型扩展；它的唯一作用是 `ILogger<UnitOfWork>` 的泛型参数。
- 改名（含命名空间）会改变所有事务日志的 SourceContext，下游日志收集规则需同步更新。

### Common patterns
- 命名空间 `System.Logging` 是约定俗成的占位空间；保持不变。

## Dependencies
### Internal
- 由 `../FilterAttributes/UnitOfWorkAttribute` 引用。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
