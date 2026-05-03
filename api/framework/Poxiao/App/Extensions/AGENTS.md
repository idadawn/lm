<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
框架对 `IServiceCollection`、`IHostBuilder`、`IConfiguration` 等 ASP.NET Core 抽象的注入扩展，是业务侧 `services.AddInject()` / `hostBuilder.Inject()` 的实现入口。负责把规范化文档、动态 API、数据校验、友好异常等核心能力一次性串联进 DI 容器。

## Key Files
| File | Description |
|------|-------------|
| `AppServiceCollectionExtensions.cs` | `AddInject` / `AddInjectMini` / `AddInjectBase`，整合 SpecificationDocument、DynamicApiControllers、DataValidation、FriendlyException |
| `AppApplicationBuilderExtensions.cs` | `IApplicationBuilder.UseInject()` 等中间件管道封装 |
| `AppWebApplicationBuilderExtensions.cs` | 针对 .NET 6+ `WebApplication` 的注入扩展 |
| `HostBuilderExtensions.cs` | `IWebHostBuilder.Inject` / `IHostBuilder.Inject`，处理 `HostingStartup` 装配 |
| `IConfigurationExtenstions.cs` | 配置读取扩展（注意原始拼写 `Extenstions`，不要重命名） |
| `IServiceScopeExtensions.cs` | `IServiceScope` 便捷 API |
| `ObjectExtensions.cs` | 框架内对象操作辅助 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Options/` | `InjectOptions`/`AddInjectOptions`/`UseInjectOptions` 配置载体 (see `Options/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 扩展方法的命名空间故意放到 `Microsoft.Extensions.DependencyInjection` / `Microsoft.Extensions.Hosting`，让业务代码直接 `using` 这些 BCL 命名空间即可触达，**不要改命名空间**。
- 所有扩展类都标 `[SuppressSniffer]`，避免被 DI 扫描误注册。
- 注意 `IConfigurationExtenstions.cs` 文件名拼写错误是历史遗留，调用方依赖此名，不要更正。

### Common patterns
- `services.AddXxx().AddYyy()` 链式构造；选项通过 `Action<TOptions>` 委托注入。
- Web/泛型主机分离：`IWebHostBuilder.Inject` 走 `FakeStartup`，`IHostBuilder.Inject` 直接装配 `InternalApp.ConfigureApplication`。

## Dependencies
### Internal
- `Poxiao.SpecificationDocument`、`Poxiao.DynamicApiController`、`Poxiao.DataValidation`、`Poxiao.FriendlyException`、`Poxiao.UnifyResult`、`Poxiao.Reflection`、`Poxiao.Components`
### External
- `Microsoft.AspNetCore.Hosting`、`Microsoft.Extensions.Hosting`、`Microsoft.Extensions.Configuration`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
