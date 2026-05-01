<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
SignalR Hub 注册特性目录。提供 `[MapHub]` 标记，让 `IEndpointRouteBuilderExtensions.MapHubs()` 在启动期反射扫描并自动注册项目内所有继承 `Hub` / `Hub<>` 的集线器，避免手工写 `app.MapHub<TChat>("/...")`。

## Key Files
| File | Description |
|------|-------------|
| `MapHubAttribute.cs` | 类级特性，构造参数 `pattern` 即终点路由地址；与 `MapHubs()` 配合实现声明式 Hub 注册。 |

## For AI Agents

### Working in this directory
- 新增特性时保持单文件单类型；`[SuppressSniffer]` 与 `AttributeUsage(AttributeTargets.Class)` 必须保留，框架依赖它进行类型扫描过滤。
- 切勿在此目录添加运行时逻辑——只放声明式标记。注册扫描位于同模块 `Extensions/IEndpointRouteBuilderExtensions.cs`。

### Common patterns
- `sealed` + 公共 `string Pattern { get; set; }` 构造模式，便于 `GetCustomAttribute<MapHubAttribute>(true)` 取值。

## Dependencies
### Internal
- 由 `Poxiao/InstantMessaging/Extensions` 反射消费。
- 框架根 `App.EffectiveTypes`（类型扫描入口）。

### External
- `Microsoft.AspNetCore.SignalR`（间接，在扩展中）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
