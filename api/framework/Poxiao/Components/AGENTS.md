<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Components

## Purpose
框架的“组件化注入”体系：以 `IServiceComponent` / `IApplicationComponent` / `IWebComponent` 三类组件接口为核心，配合 `[DependsOn]` 声明依赖关系，启动时按拓扑排序自动加载——把分散的 `services.AddXxx()` / `app.UseXxx()` 调用收敛为可声明、可复用的组件。LIMS 内部用此机制装配 RabbitMQ、WebSocket、OAuth 等基础设施模块。

## Key Files
| File | Description |
|------|-------------|
| `IApplicationComponent.cs` | 中间件组件：`Load(IApplicationBuilder, IWebHostEnvironment, ComponentContext)` |
| `IServiceComponent.cs` | DI 组件：`Load(IServiceCollection, ComponentContext)` |
| `IWebComponent.cs` | Web 主机组件：负责 `IWebHostBuilder` 阶段配置 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[DependsOn(...)]` 依赖声明特性 (see `Attributes/AGENTS.md`) |
| `Contexts/` | `ComponentContext` 上下文（参数、链接组件） |
| `Dependencies/` | 组件依赖图分析与拓扑排序辅助 |
| `Extensions/` | `services.AddComponent<T>()`/`app.UseComponent<T>()` 注册扩展 |
| `Internal/` | 内部辅助类型 |

## For AI Agents

### Working in this directory
- 业务模块定义组件时实现对应接口（DI 用 `IServiceComponent`、中间件用 `IApplicationComponent`），用 `[DependsOn(typeof(OtherComponent))]` 声明加载顺序。
- 不要在 `IServiceComponent.Load` 里调用 `IServiceProvider`——服务尚未构建。
- 组件类型必须实现 `IComponent` 接口（`DependsOnAttribute` 在 setter 内强校验），否则启动期 `InvalidOperationException`。

### Common patterns
- “组件 = 一段 `services.AddXxx`/`app.UseXxx`”；`ComponentContext` 在拓扑排序中传递参数。
- 既支持 `Type` 也支持字符串 `"Namespace.Type, Assembly"` 描述依赖（`Reflect.GetStringType`）。

## Dependencies
### Internal
- `Poxiao.Reflection`
### External
- `Microsoft.AspNetCore.Builder`、`Microsoft.AspNetCore.Hosting`、`Microsoft.Extensions.DependencyInjection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
