<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
规范化文档的强类型配置入口，对应 `appsettings.json` 中的 `SpecificationDocumentSettings` 节点。框架启动时由 `IConfigurableOptions` 自动绑定并 PostConfigure。

## Key Files
| File | Description |
|------|-------------|
| `SpecificationDocumentSettingsOptions.cs` | 文档标题、默认分组、JWT/Bearer 安全定义、XML 注释列表、`GroupOpenApiInfos`、`Servers`、过滤器开关（`EnableEnumSchemaFilter`/`EnableTagsOrderDocumentFilter`）、`EnumToNumber` 等约 18 个可配置项；`PostConfigure` 注入默认 Bearer JWT 安全方案与项目+外部 XML 注释。 |

## For AI Agents

### Working in this directory
- 实现 `IConfigurableOptions<>`，绑定路径由约定决定（类名去掉 `Options` 后缀），不要手改绑定路径。
- 修改默认值需同时改 `PostConfigure` 与项目 `Configurations/AppSetting.json` 中的示例。
- `XmlComments` 自动通过 `App.Assemblies` + `App.ExternalAssemblies` 计算文件名（去除 `.dll`），新增模块若需在文档中显示 `<summary>`，需在 csproj 启用 `GenerateDocumentationFile`。

### Common patterns
- 全部用可空类型 + `??=` 兜底；`PostConfigure(options, configuration)` 二参数签名。

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions.IConfigurableOptions<>`、`Poxiao.Reflection`、`App.Assemblies`、`SpecificationOpenApiInfo` / `SpecificationOpenApiSecurityScheme`。
### External
- `Microsoft.Extensions.Configuration`、`Microsoft.OpenApi.Models`、`Swashbuckle.AspNetCore.SwaggerUI.DocExpansion`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
