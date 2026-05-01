<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Builders

## Purpose
表达式构建的入口工厂，给上层 `And`/`Or` 链提供"恒真/恒假"的初始 lambda，并暴露 `Create<T>(Expression)` 直通方法用于在调用点直接得到强类型 `Expression<Func<T,bool>>`。

## Key Files
| File | Description |
|------|-------------|
| `LinqExpression.cs` | 静态类。`Create` 直通；`And<T>() => u => true`（与 `&&` 链的单位元）；`Or<T>() => u => false`（与 `\|\|` 链的单位元）；额外 `IndexAnd`/`IndexOr` 支持 `(u, i) => ...` 带索引签名。 |

## For AI Agents

### Working in this directory
- "And 链以 true 起头、Or 链以 false 起头"是与 `ExpressionExtensions.And/Or` 的契约；不要互换。
- 索引版与非索引版必须成对增减，避免 `IEnumerableExtensions.Where` 多签名对接断裂。

### Common patterns
- 极短静态方法 + 显式类型推导，靠类型参数承载语义。

## Dependencies
### External
- `System.Linq.Expressions`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
