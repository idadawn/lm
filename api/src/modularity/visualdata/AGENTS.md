<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# visualdata

## Purpose
数据大屏（BladeVisual）模块根目录：管理大屏定义、配置 JSON、分类、数据源、地图。表名以 `BLADE_VISUAL_*` 命名。前端通过 `api/blade-visual/...` 访问。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.VisualData/` | 服务实现（Screen/ScreenCategory/ScreenDataSource/ScreenMapConfig）(see `Poxiao.VisualData/AGENTS.md`) |
| `Poxiao.VisualData.Entitys/` | 实体、DTO、枚举 (see `Poxiao.VisualData.Entitys/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 此模块**没有 Interfaces 项目**；服务通过 DynamicApi 自动暴露，不向其它模块暴露强类型接口（与 message/visualdev 不同）。
- 字段命名采用 BladeX 旧约定：全大写下划线（`BACKGROUND_URL`、`CATEGORY_KEY`），租户字段为 `F_TenantId`（混合大小写，与其它模块不同）。

### Common patterns
- 实体实现 `ITenantFilter` 接口；部分实体加 `[Tenant(ClaimConst.TENANTID)]` 显式开启 SqlSugar 多租户。
- 主键由 `Yitter.IdGenerator.SnowflakeIdHelper.NextId()` 生成，类型 `string`。

## Dependencies
### Internal
- `Poxiao.Common.Core`（`ITenantFilter`、`CLEntityBase` 风格基类、`IFileManager`）
### External
- Yitter.IdGenerator、SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
