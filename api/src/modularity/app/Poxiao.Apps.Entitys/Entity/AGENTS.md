<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Entity

## Purpose
App 模块的 SqlSugar 实体定义目录。

## Key Files
| File | Description |
|------|-------------|
| `AppDataEntity.cs` | `[SugarTable("BASE_APPDATA")]`，继承 `OCDEntityBase`；字段 `F_OBJECTTYPE` / `F_OBJECTID` / `F_OBJECTDATA` / `F_DESCRIPTION` |

## For AI Agents

### Working in this directory
- `AppDataEntity` 是历史遗留表（使用旧的 `OCDEntityBase` 基类与全大写字段），新增字段时务必继续显式 `[SugarColumn(ColumnName = "F_XXX")]`，与既有命名风格保持一致。
- 新建表时优先使用 `CLDEntityBase` + 标准字段（`F_Id` / `F_TenantId` / `F_CREATORTIME` 等），参考 `/data/project/lm/.cursorrules` 检查清单。
- 不要在实体内写业务方法，逻辑放到 `Poxiao.Apps` 服务中。

### Common patterns
- 头部包含版本/版权/日期注释；属性使用中文 XML 注释。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Contracts`（基类）

### External
- `SqlSugar`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
