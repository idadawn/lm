<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Entity

## Purpose
低代码模块实体集合：功能模板、发布版本、模型数据、外链、门户与门户数据。

## Key Files
| File | Description |
|------|-------------|
| `VisualDevEntity.cs` | `BASE_VISUALDEV` 功能模板（type/webType/formData/columnData/flowTemplateJson/接口元信息）|
| `VisualDevReleaseEntity.cs` | `BASE_VISUALDEV_RELEASE` 线上发布版本（字段几乎对齐 VisualDevEntity） |
| `VisualDevModelDataEntity.cs` | `BASE_VISUALDEV_MODELDATA` 无表模式下的通用数据载体（VisualDevId + ParentId + Data JSON） |
| `VisualDevShortLinkEntity.cs` | `BASE_VISUALDEV_SHORT_LINK` 外链填单/查询（FormUse/FormLink/FormPassword/ColumnUse/ColumnLink/ColumnPassword/RealPcLink/RealAppLink） |
| `PortalEntity.cs` | `BASE_PORTAL` 门户主表（Type/CustomUrl/LinkType/EnabledLock） |
| `PortalDataEntity.cs` | `BASE_PORTAL_DATA` 门户表单 JSON 子表（PortalId + Platform[web/app] + Type[model/release]） |

## For AI Agents

### Working in this directory
- `VisualDevEntity` ↔ `VisualDevReleaseEntity` 双表设计；编辑发布动作需同步两边字段，新增字段也要同步。
- `VisualDevModelDataEntity` 仅在「无表模式」中使用；有表模式直接对用户表 CRUD（由 `RunService` 切库）。
- `PortalEntity.AppCustomUrl` 字段当前注释保留，删除注释前先确认前端不再依赖。
- `PortalDataEntity` 沿用 `CLEntityBase`（注意是 `CLEntityBase`，非 `CLDEntityBase`），且自定义了 `F_DeleteMark/F_DeleteTime/F_DeleteUserId`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
