<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Filters

## Purpose
Swashbuckle 过滤器集合，统一修正/增强 OpenAPI 输出：枚举展示、操作描述、Schema 异常修正、分组标签排序。

## Key Files
| File | Description |
|------|-------------|
| `EnumSchemaFilter.cs` | `ISchemaFilter`：重写枚举 Schema；按 `[EnumToNumber]` 或全局配置决定字符串/整数；含中文枚举名时强制使用整数；同时把 `[Description]` 与数值拼接到 `model.Description`。 |
| `ApiActionFilter.cs` | `IOperationFilter`：把 `ApiDescriptionSettingsAttribute.Description` 追加到操作描述；`[Obsolete]` 消息以 `<div>` 前置展示。 |
| `AnySchemaFilter.cs` | 修复 `object` / `dynamic` / `JObject` 等无固定 Schema 的类型在文档中报错的问题。 |
| `TagsOrderDocumentFilter.cs` | `IDocumentFilter`：按 `GroupOpenApiInfos.Order` 与正则 `Group@Order` 重排 `tags` 顺序。 |

## For AI Agents

### Working in this directory
- 过滤器是无状态的（依赖通过 `context` 与 `App.Configuration` 获取）；勿在字段中持有请求级状态。
- `EnumSchemaFilter` 通过 `App.Assemblies.Contains(type.Assembly)` 排除外部程序集枚举，新增三方枚举需配置 `ExternalAssemblies`。
- 启用控制位于 `Options.SpecificationDocumentSettingsOptions`：`EnableEnumSchemaFilter` / `EnableTagsOrderDocumentFilter`。

### Common patterns
- 通过 `value.ChangeType(enumValueType)` 处理 long/byte 等非 int 枚举底层类型。
- 文档说明用 `<br />` 拼接 HTML，依赖 SwaggerUI 渲染。

## Dependencies
### Internal
- `Poxiao.Extensions`（`ChangeType` 等）、`App.Assemblies` / `App.Configuration`。
### External
- `Swashbuckle.AspNetCore.SwaggerGen`、`Microsoft.OpenApi.Models`、`Microsoft.OpenApi.Any`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
