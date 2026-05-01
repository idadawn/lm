<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
`Expression<>` 组合与 `IQueryable<>`/`IEnumerable<>` 条件查询的拓展面。检测室系统的"高级查询""根据筛选条件动态加 Where"全部依赖此处。

## Key Files
| File | Description |
|------|-------------|
| `ExpressionExtensions.cs` | 核心 `Compose`（参数替换后合并）以及 `And`/`Or`/`AndIf`/`OrIf`（含索引签名各 4 个），加上 `GetExpressionPropertyName`（从 `u => u.Prop` 取属性名）和 `IsNullOrEmpty<T>(IEnumerable<T>)` 工具。 |
| `IEnumerableExtensions.cs` | `IQueryable<T>.Where(bool, Expression)`、`Where(params Expression[])`（多个表达式 Or 合并）、`Where(params (bool,Expression)[])`（条件式 Or 合并），全部带索引版本；末尾两个签名也覆盖 `IEnumerable<T>` 的 `Func` 版。 |

## For AI Agents

### Working in this directory
- `Compose` 是组合的唯一入口——新增组合方式（如 XOR）应复用它而不是手写 `ExpressionVisitor`。
- 多表达式 `Where` **使用 Or 合并**（见 `LinqExpression.Or<T>()` 起点），不要默认改成 And——会破坏现有调用方语义。
- 命名空间放置策略：`IEnumerableExtensions` 故意置于 `System.Linq` 以便用户无需 `using Poxiao.LinqBuilder` 即可使用。

### Common patterns
- 重载矩阵：每个方法都有 `(condition,)`、`(params)`、`(params (bool,))` 三种入口 × 索引/非索引两套签名。

## Dependencies
### Internal
- `Poxiao.LinqBuilder.LinqExpression`、`ParameterReplaceExpressionVisitor`。

### External
- `System.Linq.Expressions`、`System.Linq.Queryable`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
