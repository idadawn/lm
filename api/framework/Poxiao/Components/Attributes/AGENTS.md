<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
组件依赖声明特性。`[DependsOn]` 标注在组件类上，描述该组件加载前需要先就绪的其它组件（`DependComponents`）以及与之“并行链接”的组件（`Links`）。框架据此生成 DAG 并拓扑排序加载顺序。

## Key Files
| File | Description |
|------|-------------|
| `DependsOnAttribute.cs` | `sealed` 特性：`params object[] dependComponents` 同时支持 `Type` 与 `"Namespace.Type, Assembly"` 字符串；setter 内校验目标必须实现 `IComponent`，否则 `InvalidOperationException` |

## For AI Agents

### Working in this directory
- 业务组件用法：`[DependsOn(typeof(RabbitMQComponent), "Poxiao.WebSockets.WebSocketComponent, Poxiao.WebSockets")]`。
- 不要循环依赖——拓扑排序检测到环会启动期失败。
- `Links` 与 `DependComponents` 语义不同：`Links` 是“一起组合可用”，不强制顺序；`DependComponents` 必须先于自身加载。

### Common patterns
- 反射友好：字符串解析走 `Poxiao.Reflection.Reflect.GetStringType`，方便跨程序集解耦声明。

## Dependencies
### Internal
- `Poxiao.Components`、`Poxiao.Reflection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
