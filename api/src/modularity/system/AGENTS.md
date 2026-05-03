<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# system

## Purpose
系统管理与权限模块（system module）—— 模块化单体后端中的核心 RBAC、组织架构、菜单/按钮权限、字典、定时任务、数据接口、消息模板等基础平台能力。被几乎所有其他业务模块（lab、workflow、ai 等）依赖。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.Systems/` | 业务实现层：Service 类（动态 API 控制器） (see `Poxiao.Systems/AGENTS.md`) |
| `Poxiao.Systems.Entitys/` | 实体、Dto、Model、Enum、Mapper 定义层 (see `Poxiao.Systems.Entitys/AGENTS.md`) |
| `Poxiao.Systems.Interfaces/` | 服务接口契约层（IXxxService） (see `Poxiao.Systems.Interfaces/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 三段式分层：实现项目（Poxiao.Systems）依赖接口项目（Poxiao.Systems.Interfaces），接口项目依赖实体项目（Poxiao.Systems.Entitys）。新增 Service 必须在三处都登记（Entity/Dto/Interface/Service）。
- 实体继承自 `CLDEntityBase`；遗留表统一带 `[SugarColumn(ColumnName = "F_XXX")]` 大写命名映射，遵守 `.cursorrules`。
- Service 通常实现 `IDynamicApiController, ITransient` 并通过 `[ApiDescriptionSettings(Tag = "...", Name = "...", Order = ...)]` 暴露为 REST API。
- 业务功能划分两大领域：`Permission`（用户/机构/角色/岗位）与 `System`（菜单/字典/数据接口/任务/日志等）。

### Common patterns
- 仓储注入：`ISqlSugarRepository<TEntity>`。
- DTO 命名：`XxxCrInput`（Create）、`XxxUpInput`（Update）、`XxxListOutput`、`XxxInfoOutput`、`XxxSelectorOutput`。
- 多租户实体加 `[Tenant(ClaimConst.TENANTID)]`。

## Dependencies
### Internal
- `api/src/modularity/common/Poxiao.Common`（Poxiao.Infrastructure 共用契约/常量）
- `api/src/infrastructure/Poxiao.Extras.CollectiveOAuth`、`Poxiao.Extras.Thirdparty`
- `api/src/modularity/engine/Poxiao.VisualDev.Engine`、`message`、`workflow`、`taskschedule`（接口）
- `api/src/modularity/app/Poxiao.Apps.Entitys`

### External
- SqlSugar、Mapster、SkiaSharp、SharpZipLib、System.Drawing.Common、Microsoft.Extensions.Caching.Memory

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
