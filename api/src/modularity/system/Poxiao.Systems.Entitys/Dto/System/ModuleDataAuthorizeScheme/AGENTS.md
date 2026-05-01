<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ModuleDataAuthorizeScheme

## Purpose
DTOs for "功能权限数据方案" — a named, reusable authorization scheme that bundles a set of conditions (`conditionJson`) for a function module. Schemes can then be assigned to roles instead of duplicating raw conditions.

## Key Files
| File | Description |
|------|-------------|
| `ModuleDataAuthorizeSchemeCrInput.cs` | Create — `moduleId`, `fullName`, `enCode`, `conditionJson` (serialised condition tree), `conditionText` (display string). Defines nested `ConditionJsonItem { logic, groups }` and `GroupsItem { id, field, type, op, value }` for the tree shape. |
| `ModuleDataAuthorizeSchemeUpInput.cs` | Update payload (adds `id`). |
| `ModuleDataAuthorizeSchemeOutput.cs` | Detail output. |
| `ModuleDataAuthorizeSchemeInfoOutput.cs` | Info projection used by the edit form. |
| `ModuleDataAuthorizeSchemeListOutput.cs` | Grid projection. |

## For AI Agents

### Working in this directory
- `conditionJson` is a JSON string (not a typed object) so the same payload can round-trip from the frontend rule builder unchanged. Only deserialise to `ConditionJsonItem[]` when actually evaluating rules.
- The `ConditionJsonItem` / `GroupsItem` shapes mirror the frontend's super-query rule builder (see `web/src/components/Generator/ColumnDesign/superQuery`). Property names (`logic`, `groups`, `field`, `op`, `value`) must stay lowerCamel.
- Namespace is `Poxiao.Systems.Entitys.Dto.ModuleDataAuthorizeScheme` (no `.System.` segment) — copy-paste from a sibling carefully.
- `[SuppressSniffer]` keeps these classes off the dynamic API surface.

### Common patterns
- Cr/Up/Info/List/Output suffix.
- Helper sub-classes (`ConditionJsonItem`, `GroupsItem`) declared in the same file as the Cr input.

## Dependencies
### Internal
- Consumed by `modularity/system/Poxiao.Systems` services / controllers; rules feed into SqlSugar `Where` builders at query time.

### External
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
