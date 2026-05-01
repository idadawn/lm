<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Filters

## Purpose
ASP.NET Core 启动过滤器，负责在管道最前端注入框架级中间件：写入 `environment` 响应头、自动收口未托管对象释放、刷新 Token 时把 401 转为 403，并按 `Order` 反向遍历调用所有 `AppStartup.Configure*`。同时支持 `App.Settings.VirtualPath` 二级虚拟目录部署。

## Key Files
| File | Description |
|------|-------------|
| `StartupFilter.cs` | `IStartupFilter` 实现：注册根服务到 `InternalApp.RootServices`，统一中间件入口、解析 `AppStartup` 的 `Configure` 方法（按反射注入参数）并按虚拟目录映射 |

## For AI Agents

### Working in this directory
- `StartupFilter` 是单例且全局唯一，新增钩子优先在此注入而不是创建新的 `IStartupFilter`。
- 需要保留 `App.DisposeUnmanagedObjects()` 调用——业务模块依赖该钩子释放每请求范围对象。
- `AppStartups.Reverse()` 顺序很关键，新增逻辑不要打乱。

### Common patterns
- 反射调用 `AppStartup` 中所有 `void` 返回、首参为 `IApplicationBuilder` 的方法，并从 `ApplicationServices` 解析其余参数——遵循 ASP.NET Core 经典 Startup 注入语义。
- WebSocket 请求绕过响应头注入逻辑（`context.IsWebSocketRequest()`）。

## Dependencies
### Internal
- `App/App.cs`、`App/Internal/InternalApp.cs`、`App/Startups/AppStartup.cs`
### External
- `Microsoft.AspNetCore.Builder`、`Microsoft.AspNetCore.Http`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
