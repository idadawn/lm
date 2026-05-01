<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Startups

## Purpose
模块化启动体系：`AppStartup` 抽象类被各业务模块继承（如 `Lab.LabAppStartup`），按 `[AppStartup(order)]` 排序自动加载；`HostingStartup` 通过 `[assembly: HostingStartup]` 在 ASP.NET Core 启动早期触发框架注入；`FakeStartup` 解决 .NET 6+ 泛型主机不再要求 `Startup.cs` 后的兼容问题；`GenericHostLifetimeEventsHostedService` 提供应用启动/停止事件。

## Key Files
| File | Description |
|------|-------------|
| `AppStartup.cs` | 抽象基类，业务模块继承后实现 `ConfigureServices(IServiceCollection)` / `Configure(IApplicationBuilder, ...)` 等方法 |
| `HostingStartup.cs` | `IHostingStartup` 实现，`[assembly: HostingStartup(typeof(Poxiao.HostingStartup))]` 让框架在外部应用集成时自动接入 |
| `FakeStartup.cs` | 占位 Startup，配合 `IWebHostBuilder.UseStartup<FakeStartup>()` 满足泛型主机要求 |
| `GenericHostLifetimeEventsHostedService.cs` | 监听 `IHostApplicationLifetime` 的启动/停止事件，输出框架日志 |

## For AI Agents

### Working in this directory
- 业务模块新增启动逻辑：在自身工程定义 `class XxxAppStartup : AppStartup`，加 `[AppStartup(N)]`；**不要直接修改本目录**。
- `HostingStartup` 在程序集级别注册，禁止移除 `[assembly: HostingStartup(...)]`。
- `FakeStartup` 仅作框架兼容用途，不能放业务依赖。

### Common patterns
- 反射 + 约定：`StartupFilter` 找名字以 `Configure` 开头、首参 `IApplicationBuilder` 的方法批量调用，参数从 DI 解析。

## Dependencies
### Internal
- 被 `App/Filters/StartupFilter.cs` 与 `App/Internal/InternalApp.cs` 协同消费。
### External
- `Microsoft.AspNetCore.Hosting`、`Microsoft.Extensions.Hosting`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
