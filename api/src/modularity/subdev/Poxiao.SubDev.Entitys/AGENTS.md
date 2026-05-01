<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.SubDev.Entitys

## Purpose
Entity + DTO project for the 二次开发 module. The data-shape layer that customer-specific SubDev features will populate — currently empty (only `.csproj`). Sits at the bottom of the SubDev dependency chain, depending only on the shared `Poxiao.Infrastructure` foundation.

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.SubDev.Entitys.csproj` | Empty project; references `Poxiao.Infrastructure` (`..\..\common\Poxiao.Common\Poxiao.Infrastructure.csproj`); pulls StyleCop analyzers. |

## For AI Agents

### Working in this directory
- New SubDev tables go here. Inherit `CLDEntityBase` (from `Poxiao.Infrastructure`). Per `.cursorrules`:
  - For new tables, use the standard inherited field names; no overrides needed.
  - For legacy or external tables, use `[SugarColumn(ColumnName = "F_LegacyName")]` to remap, or `[SugarColumn(IsIgnore = true)]` to drop fields the table lacks.
  - Mind the inconsistent base names: `F_Id`, `F_TenantId` (parent), uppercase `F_CREATORTIME` / `F_CREATORUSERID` / `F_ENABLEDMARK`, mixed-case `F_LastModifyTime` / `F_DeleteMark` etc.
- Place DTOs under a `Dto/` subfolder once added (matches sibling modules' layout).
- `[SugarTable("xxx")]` table name should follow the project's `tb_subdev_*` convention once features land.
- Don't reference any service / interface project from here — keep this layer dependency-free downward.

## Dependencies
### Internal
- `Poxiao.Infrastructure` — entity base classes, security attributes, common DI helpers.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
