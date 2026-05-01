<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
即时通信模块的 ASP.NET Core 终点路由扩展。提供 `MapHubs()` 一次性扫描并注册所有贴有 `[MapHub("...")]` 的 SignalR Hub 类型，是 `Poxiao.InstantMessaging` 对外的唯一启动钩子。

## Key Files
| File | Description |
|------|-------------|
| `IEndpointRouteBuilderExtensions.cs` | 反射 `App.EffectiveTypes` 找到 `Hub`/`Hub<>` 子类、读取 `MapHubAttribute.Pattern`，并通过反射调用 `HubEndpointRouteBuilderExtensions.MapHub<T>` 完成注册；同时回调 Hub 类静态方法 `HttpConnectionDispatcherOptionsSettings`、`HubEndpointConventionBuilderSettings` 进行 per-Hub 配置。 |

## For AI Agents

### Working in this directory
- 命名空间故意置于 `Microsoft.AspNetCore.Builder` 以便 `app.MapHubs()` 自动出现在用户 IntelliSense 中——保持不变。
- 若新增 Hub 选项约定，请优先扩展 Hub 类内部的两个静态钩子方法签名，而非修改本扩展的反射调用面。

### Common patterns
- 静态扩展类 + `[SuppressSniffer]` 阻止框架的依赖嗅探。
- 通过 `MakeGenericMethod` 动态调用泛型 `MapHub<T>`，避免对每个 Hub 写注册代码。

## Dependencies
### Internal
- `Poxiao.InstantMessaging.MapHubAttribute`（标记目标）。
- `Poxiao.App.EffectiveTypes`、`HasImplementedRawGeneric` 反射工具。

### External
- `Microsoft.AspNetCore.SignalR`、`Microsoft.AspNetCore.Http.Connections`、`Microsoft.AspNetCore.Routing`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
