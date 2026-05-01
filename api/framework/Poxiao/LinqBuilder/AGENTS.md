<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# LinqBuilder

## Purpose
表达式（`Expression<Func<T,bool>>`）动态拼接工具集，给检测室系统的多条件查询场景（高级筛选、动态报表）提供 `And`/`Or`/`AndIf`/`OrIf` 组合能力。核心思路是用 `ExpressionVisitor` 替换参数符号，使两个 lambda 的参数命名空间统一后再 `AndAlso`/`OrElse`。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Builders/` | 表达式工厂（And/Or/Create 起点） (see `Builders/AGENTS.md`) |
| `Extensions/` | `Expression<>` 与 `IQueryable<>`/`IEnumerable<>` 的链式拓展 (see `Extensions/AGENTS.md`) |
| `Visitors/` | 内部 `ExpressionVisitor` 实现 (see `Visitors/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新增组合操作时不要直接拼 `Expression.AndAlso`，必须先经 `ParameterReplaceExpressionVisitor` 替换参数；否则 SqlSugar/EF 翻译会因参数对象不一致而失败。
- 同时维护"无索引"和"带索引 (`int i`)"两个签名，模式见现有 `And<T>`/`IndexAnd<T>`。

### Common patterns
- 静态类 + `[SuppressSniffer]`；条件版 `*If` 直接复用基础组合方法。

## Dependencies
### External
- `System.Linq.Expressions`、`System.Linq.Queryable`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
