<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
模块内部数据结构：扩展过的 OpenAPI Info、登录信息、安全方案与分组附加信息。这些类型由 `SpecificationDocumentSettingsOptions` 持有并被 Builder 消费，不暴露给业务模块直接使用。

## Key Files
| File | Description |
|------|-------------|
| `SpecificationOpenApiInfo.cs` | 继承 `OpenApiInfo`，附加 `Group` / `Order` / `Visible` / `RouteTemplate`，`Title` 缺省回退为 `Group`。 |
| `SpecificationOpenApiSecurityScheme.cs` | 扩展 Swagger 安全方案，附带 `Id` 与 `Requirement`。 |
| `SpecificationOpenApiSecurityRequirementItem.cs` | 表达 `OpenApiSecurityScheme` + `Accesses` 二元组，便于 JSON 配置直接绑定。 |
| `SpecificationLoginInfo.cs` | SwaggerUI 登录窗体配置（用户名/密码字段、提交地址等）。 |
| `SpecificationAuth.cs` | 登录提交时携带的鉴权数据 DTO。 |
| `GroupExtraInfo.cs` | 分组的额外元数据（来源类型/方法），用于 Builder 的 `DocumentGroupExtras` 缓存。 |

## For AI Agents

### Working in this directory
- 这些类型构成 `appsettings.json -> SpecificationDocumentSettings` 的强类型契约；变更字段必须同步 JSON 配置示例与 `Options.PostConfigure` 默认值。
- 标记 `internal` 设置器（如 `RouteTemplate`）用于框架内部赋值，业务勿反射覆盖。

### Common patterns
- `sealed` POCO，多数实现 `[SuppressSniffer]`；属性使用可空类型携带缺省语义。

## Dependencies
### External
- `Microsoft.OpenApi.Models`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
