<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Visitors

## Purpose
`ExpressionVisitor` 实现目录。`Compose` 合并两条 lambda 时，两边 `ParameterExpression` 实例不同（即使名字一样），直接 `AndAlso` 会产生"参数未绑定"异常；本目录的访问者负责把第二条 lambda 的参数替换为第一条的参数实例。

## Key Files
| File | Description |
|------|-------------|
| `ParameterReplaceExpressionVisitor.cs` | `internal sealed`，构造接收 `Dictionary<ParameterExpression,ParameterExpression>` 映射；重写 `VisitParameter` 按映射替换；静态 `ReplaceParameters(map, expr)` 是外部唯一入口。 |

## For AI Agents

### Working in this directory
- 类被 `ExpressionExtensions.Compose` 直接 new + 调用，请保持 `internal` 可见性。
- 不要把映射改成线程静态/单例——每次组合都是一次性短生命周期对象。
- 任何新增 Visitor（例如成员重写、常量提升）也放本目录，命名规则 `*ExpressionVisitor`。

### Common patterns
- `static ReplaceParameters(...)` 外观 + 仅重写需要的访问点。

## Dependencies
### External
- `System.Linq.Expressions`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
