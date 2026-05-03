<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# App

## Purpose
框架的应用启动核心：提供全局 `App` 静态门面（`App.Settings`、`App.Configuration`、`App.HttpContext` 等）、`Serve`/`Native` 主机启动入口、`AppStartup` 抽象与 `IStartupFilter`，统一负责程序集扫描、配置装载、根服务存储与中间件管道的自动接入。所有业务模块通过 `App.*` 访问框架运行时上下文。

## Key Files
| File | Description |
|------|-------------|
| `App.cs` | 全局应用静态类：暴露 `Settings`/`Configuration`/`HttpContext`/`Assemblies`/`EffectiveTypes`，并集成 MiniProfiler |
| `Serve.cs` | `Serve.Run`、`RunNative`、`IdleHost` 等主机启动辅助 API（用于控制台/WinForm/WPF 宿主） |
| `ServeComponent.cs` | 组件化主机启动入口（结合 `Components/`） |
| `Native.cs` | 原生互操作辅助（如 `GetIdlePort`） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[AppStartup(order)]` 等启动标记 (see `Attributes/AGENTS.md`) |
| `Extensions/` | `IServiceCollection`/`IHostBuilder` 注入扩展 (see `Extensions/AGENTS.md`) |
| `Filters/` | `IStartupFilter` 实现，自动接入中间件 (see `Filters/AGENTS.md`) |
| `Internal/` | `InternalApp` 内部状态（不对外） (see `Internal/AGENTS.md`) |
| `Options/` | `AppSettingsOptions`、`RunOptions` 等启动选项 (see `Options/AGENTS.md`) |
| `SingleFile/` | 单文件发布支持接口 (see `SingleFile/AGENTS.md`) |
| `Startups/` | `AppStartup` 抽象 + `HostingStartup`/`FakeStartup` (see `Startups/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 切勿在业务代码中直接 `new` 出 `AppStartup` 子类——框架自动通过程序集扫描发现并按 `Order` 排序加载。
- `App.Configuration` 需考虑 Worker/CLI 场景：当 `InternalApp.Configuration` 为空时返回空配置而非抛 NRE，扩展时遵循该原则。
- `App.HttpContext` 在非请求线程为 null，使用前判空。

### Common patterns
- 静态门面 + 内部 `InternalApp` 双层结构：对外只暴露只读 `App.*`，运行时状态写入由内部静态完成。
- 入口都标记 `[SuppressSniffer]`，避免被反射 DI 误扫描。

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions`、`Poxiao.Reflection`、`Poxiao.Templates`、`Poxiao.UnifyResult`
### External
- `Microsoft.Extensions.Hosting`、`Microsoft.AspNetCore.Hosting`、`StackExchange.Profiling`、`Microsoft.CodeAnalysis.CSharp`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
