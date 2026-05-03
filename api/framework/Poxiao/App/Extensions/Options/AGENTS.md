<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
`AddInject` / `Inject` / `UseInject` 系列扩展使用的选项载体。承载 Swagger/数据验证/友好异常等组件的统一配置入口，以及 `HostingStartup` 装配所需的程序集名称、配置扫描目录等元数据。

## Key Files
| File | Description |
|------|-------------|
| `InjectOptions.cs` | `IWebHostBuilder.Inject` / `IHostBuilder.Inject` 的配置类：`AssemblyName`、`AutoRegisterBackgroundService`、`ConfigurationScanDirectories`、`IgnoreConfigurationFiles`，以及 `ConfigureAppConfiguration`/`ConfigureServices` 委托 |
| `AddInjectOptions.cs` | `services.AddInject()` 选项，集中维护 `SwaggerGenConfigure`、`DataValidationConfigure`、`FriendlyExceptionConfigure` 等子组件回调 |
| `UseInjectOptions.cs` | `IApplicationBuilder.UseInject()` 选项，控制中间件启用顺序与开关 |

## For AI Agents

### Working in this directory
- 选项类内部公开委托使用 `internal static`，由 `App/Extensions/HostBuilderExtensions.cs` 等读取——保留 `internal` 可见性。
- 添加新选项时同时更新 `AppServiceCollectionExtensions.AddInject` 的串联代码，保持单点入口语义。

### Common patterns
- 委托式配置：每个组件提供静态 `Action<TSubOption>` 字段，业务侧通过链式 `configure` 注入。
- `params string[]` 数组通过空安全默认值（`?? Array.Empty<string>()`）保护。

## Dependencies
### Internal
- 被 `App/Extensions/AppServiceCollectionExtensions.cs`、`HostBuilderExtensions.cs`、`AppApplicationBuilderExtensions.cs` 消费。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
