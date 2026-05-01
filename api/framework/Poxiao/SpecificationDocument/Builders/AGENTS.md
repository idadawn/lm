<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Builders

## Purpose
规范化文档的核心装配中心。`SpecificationDocumentBuilder` 把 `SpecificationDocumentSettingsOptions`、`AppSettings`、动态 API 控制器分组、XML 注释、安全定义、过滤器等组合成 SwaggerGen / Swagger / SwaggerUI 三套配置。

## Key Files
| File | Description |
|------|-------------|
| `SpecificationDocumentBuilder.cs` | 静态 `[SuppressSniffer]` 类，预计算 `DocumentGroups` / `DocumentGroupExtras` / `_groupOrderRegex`；提供 `BuildGen` / `Build` / `BuildUI` 三个入口供 `Extensions` 调用；包含 SchemaId 解析、XML 注释加载、`AllGroupsKey="All Groups"` 聚合等逻辑。 |

## For AI Agents

### Working in this directory
- 单文件含约 800+ 行配置逻辑，**只读字段使用 `static readonly` 缓存**，新增配置应在静态构造函数中初始化，避免每次请求重算。
- 与 `Poxiao.DynamicApiController` 紧耦合：通过反射枚举 `App.Assemblies` 收集控制器与分组；新增程序集需被 `App.ExternalAssemblies` 包含。
- 不要直接 new SwaggerGenOptions——Builder 通过 Action 委托配置以保持 Swashbuckle 原生扩展性。

### Common patterns
- 静态构造函数预热 + `[SuppressSniffer]` 抑制框架反射扫描。
- 通过 `_groupOrderRegex` 解析 `Group@Order` 形式的分组排序语法。

## Dependencies
### Internal
- `Poxiao.DynamicApiController`、`Poxiao.Extensions`、`Poxiao.Reflection`、`AppSettingsOptions`。
### External
- `Swashbuckle.AspNetCore.SwaggerGen` / `Swagger` / `SwaggerUI`、`Microsoft.OpenApi.Models`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
