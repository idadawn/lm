<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# InstantMessaging

## Purpose
SignalR 即时通讯模块的轻量封装。围绕 ASP.NET Core SignalR 提供静态访问入口（`IM.GetHub`）、约定式 Hub 路由特性（`[MapHub]`），以及 `IEndpointRouteBuilder` 的批量映射拓展，把 Hub 类的注册声明化。

## Key Files
| File | Description |
|------|-------------|
| `IM.cs` | 静态入口。`IM.GetHub<THub>()` 通过 `App.GetService<IHubContext<THub>>` 解析；`IM.GetHub<THub, TStronglyTyped>()` 解析强类型客户端 `IHubContext<THub, TStronglyTyped>` —— 跨服务向客户端推送消息的标准方式。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[MapHub("/route")]` Hub 路由声明特性 (see `Attributes/AGENTS.md`) |
| `Extensions/` | `IEndpointRouteBuilder` 自动映射拓展 (see `Extensions/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 业务代码推送实时消息：`var hub = IM.GetHub<NotifyHub>(); await hub.Clients.Group("group").SendAsync("Notice", payload);`。
- 新建 Hub 时贴 `[MapHub("/hubs/notify")]`，并在 `Configure` 中调用框架 endpoint 拓展即可自动 `endpoints.MapHub<T>("/hubs/notify")`。
- 强类型客户端方法定义放在独立接口（如 `INotifyClient`），并让 Hub 继承 `Hub<INotifyClient>`，对应 `IM.GetHub<NotifyHub, INotifyClient>()`。

### Common patterns
- 命名空间 `Poxiao.InstantMessaging`；与 `Poxiao.infrastructure/WebSockets`（裸 WebSocket 路径）有所区分 —— 前者基于 SignalR，后者更底层。

## Dependencies
### Internal
- `App.GetService<T>`（Furion 依赖容器）。
### External
- `Microsoft.AspNetCore.SignalR`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
