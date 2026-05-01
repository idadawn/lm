<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
SpecificationDocument 模块对外暴露的 DI 注册与中间件启用入口。被 `Poxiao.API.Entry` 启动管线调用以接入 Swagger + MiniProfiler。

## Key Files
| File | Description |
|------|-------------|
| `SpecificationDocumentServiceCollectionExtensions.cs` | `AddSpecificationDocuments(IMvcBuilder/IServiceCollection)`：注册可配置选项、`AddEndpointsApiExplorer`、`AddSwaggerGen` 调用 `Builder.BuildGen`，并按需注册 MiniProfiler（路由 `/index-mini-profiler`，仅 Swagger 请求触发）。 |
| `SpecificationDocumentApplicationBuilderExtensions.cs` | `UseSpecificationDocuments(routePrefix?, swagger?, swaggerUI?)`：依次 `UseSwagger`/`UseSwaggerUI` 并按 `App.Settings.InjectMiniProfiler` 启用 MiniProfiler 中间件。 |

## For AI Agents

### Working in this directory
- 两个总开关均落在 `App.Settings`：`InjectSpecificationDocument` 控总开关，`InjectMiniProfiler` 控性能面板。
- 用户自定义 `Action<SwaggerGenOptions>` 会在 Builder 流程的最后执行，保留覆盖能力——不要把它前置。
- 命名空间故意放在 `Microsoft.Extensions.DependencyInjection` / `Microsoft.AspNetCore.Builder`，便于直接被宿主项目发现，不要更改。

### Common patterns
- `[SuppressSniffer]` 静态 `IServiceCollection` / `IApplicationBuilder` 拓展。

## Dependencies
### Internal
- `Poxiao.SpecificationDocument.Builders.SpecificationDocumentBuilder`、`App.Settings`。
### External
- `Swashbuckle.AspNetCore.*`、`MiniProfiler.AspNetCore.Mvc`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
