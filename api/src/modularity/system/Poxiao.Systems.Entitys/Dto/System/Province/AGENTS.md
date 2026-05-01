<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Province

## Purpose
DTOs for "行政区划" (Chinese administrative regions) tree maintenance — province / city / county hierarchy. Exposes typical CRUD inputs plus a tree-shaped list output and a flat selector output for cascader components.

## Key Files
| File | Description |
|------|-------------|
| `ProvinceCrInput.cs` | Create — `description`, `enCode` (区域编号), `enabledMark`, `fullName`, `parentId` (上级 ID), `sortCode`. |
| `ProvinceUpInput.cs` | Update payload (adds `id`). |
| `ProvinceInfoOutput.cs` | Detail / edit-form output. |
| `ProvinceListOutput.cs` | Grid/tree projection — extends `Poxiao.Infrastructure.Security.TreeModel` to expose `children` / `id` for the tree component. |
| `ProvinceSelectorOutput.cs` | Lightweight selector projection for the cascader dropdown. |
| `ProvinceGetDataInput.cs` | Filter input for the tree data fetch. |

## For AI Agents

### Working in this directory
- `ProvinceListOutput` inherits `TreeModel`; do not add a separate `children` property — `TreeModel` already provides it.
- `enCode` follows the standard administrative-division code convention (国标 GB/T 2260) — preserve case/format when displaying.
- Namespace `Poxiao.Systems.Entitys.Dto.Province` (no `.System.` segment).
- `[SuppressSniffer]` applied.
- The tree is potentially deep (province→city→county→...); paging applies to root-level only.

### Common patterns
- Cr/Up/Info/List/Selector/Output suffix.
- Tree outputs inherit `TreeModel` rather than declaring `children` ad-hoc.

## Dependencies
### Internal
- `Poxiao.Infrastructure.Security.TreeModel` (parent class for tree projections).

### External
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
