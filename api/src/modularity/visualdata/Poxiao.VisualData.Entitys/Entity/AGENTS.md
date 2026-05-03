<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Entity

## Purpose
数据大屏实体集合，全部 `BLADE_VISUAL_*` 表名，字段使用 BladeX 风格全大写下划线。

## Key Files
| File | Description |
|------|-------------|
| `VisualEntity.cs` | `BLADE_VISUAL` 大屏主表（title/backgroundUrl/category/password/createUser/createDept/...）；含 `Create/LastModify/Delete` 内置方法 |
| `VisualConfigEntity.cs` | `BLADE_VISUAL_CONFIG` 配置 JSON（visualId/detail/component） |
| `VisualDBEntity.cs` | `BLADE_VISUAL_DB` 数据源（driverClass/url/username/password） |
| `VisualMapEntity.cs` | `BLADE_VISUAL_MAP` 自定义地图 GeoJSON |
| `VisualCategoryEntity.cs` | `BLADE_VISUAL_CATEGORY` 分类（categoryKey/categoryValue） |

## For AI Agents

### Working in this directory
- 实体均**实现** `ITenantFilter`，部分加 `[Tenant(ClaimConst.TENANTID)]` 强制按租户过滤。租户字段统一 `F_TenantId`（**混合大小写**，与其它 Poxiao 模块的 `F_TENANTID` 不同）。
- 主键 `string Id`，由 `SnowflakeIdHelper.NextId()` 生成；删除标记 `IS_DELETED` (int) 软删，列表查询需 `Where(v => v.IsDeleted == 0)`。
- `Create()` 默认背景图为 `/api/file/VisusalImg/bg/bg1.png`（拼写 `Visusal`），勿改。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
