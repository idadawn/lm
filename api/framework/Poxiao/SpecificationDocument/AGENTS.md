<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SpecificationDocument

## Purpose
基于 Swashbuckle/Swagger 的"规范化文档"模块，自动为 `Poxiao.DynamicApiController` 生成多分组 OpenAPI、SwaggerUI、MiniProfiler、JWT 鉴权按钮、枚举/标签/SchemaId 优化等。是后端 LIMS API（`api/src/application/Poxiao.API.Entry`）的接口门面。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Assets/` | 内嵌 `index.html` / `index-mini-profiler.html` SwaggerUI 资源 (see `Assets/AGENTS.md`) |
| `Attributes/` | 文档级特性：`SchemaIdAttribute` / `OperationIdAttribute` / `EnumToNumberAttribute` (see `Attributes/AGENTS.md`) |
| `Builders/` | 核心 `SpecificationDocumentBuilder` 静态构建器 (see `Builders/AGENTS.md`) |
| `Extensions/` | `IServiceCollection` / `IApplicationBuilder` 注册与启用拓展 (see `Extensions/AGENTS.md`) |
| `Filters/` | Swagger Schema/Operation/Document 过滤器 (see `Filters/AGENTS.md`) |
| `Internal/` | 文档内部模型：分组、登录、安全方案、OpenApi Info (see `Internal/AGENTS.md`) |
| `Options/` | `SpecificationDocumentSettingsOptions` 可配置选项 (see `Options/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 整个模块通过 `App.Settings.InjectSpecificationDocument` 总开关启停；不要硬编码绕过。
- 分组逻辑依赖 `ApiDescriptionSettings` 与 `_groupOrderRegex`，新增分组应通过特性或配置 `GroupOpenApiInfos` 而非直接改 Builder。
- 与 `Poxiao.DynamicApiController` 强耦合：控制器名/分组规则改动需同步该模块。
- 修改 SwaggerUI 资源需替换 `Assets/index*.html` 嵌入文件而非新增静态文件。

### Common patterns
- 静态 `[SuppressSniffer]` Builder + `IConfigurableOptions<>` 选项 + 服务集合扩展 `AddSpecificationDocuments` / `UseSpecificationDocuments`。
- JWT Bearer 默认安全方案在 `PostConfigure` 中注入。

## Dependencies
### Internal
- `Poxiao.DynamicApiController`、`Poxiao.ConfigurableOptions`、`Poxiao.Reflection`、`Poxiao.Extensions`、`App.Settings`。
### External
- `Swashbuckle.AspNetCore.Swagger` / `SwaggerGen` / `SwaggerUI`、`Microsoft.OpenApi.Models`、`MiniProfiler`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
