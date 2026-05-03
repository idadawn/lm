<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ModuleForm

## Purpose
DTOs for "功能列表配置" — the per-module column / form-field mapping that drives generic list pages. Defines which columns of a bound table show up in lists, their labels, sort, enabled state, and master/sub relationships.

## Key Files
| File | Description |
|------|-------------|
| `ModuleFormCrInput.cs` | Create — `moduleId`, `bindTableName` (display), `bindTable` (real), `enCode`, `fullName`, `enabledMark`, `description`, `sortCode`, `fieldRule` (0:主表 / 1:副表), `childTableKey`. |
| `ModuleFormUpInput.cs` | Update payload (adds `id`). |
| `ModuleFormOutput.cs` | Detail output. |
| `ModuleFormListOutput.cs` | Grid projection for the management UI. |
| `ModuleColumnActionsBatchInput.cs` | Batch update — `moduleId`, `bindTableName`, `bindTable`, plus `formJson` (List<Form>) where each `Form` carries `enCode`, `fullName`, `bindTable`, `fieldRule`, `childTableKey`. Used when saving the entire column layout in one call. |

## For AI Agents

### Working in this directory
- Note: `ModuleColumnActionsBatchInput.cs` declares the class as `ModuleFormActionsBatchInput` (not matching its file name) and its inner `Form` class has a malformed XML doc comment (`</summary` missing `>`). Fix carefully — controllers reference the class name, not the file name.
- `fieldRule` (0=master, 1=sub-table) shares semantics with `ModuleDataAuthorize` DTOs.
- Namespace `Poxiao.Systems.Entitys.Dto.ModuleForm` (no `.System.` segment).
- `[SuppressSniffer]` is applied so Furion does not promote these to endpoints.
- Frontend pairing lives under `web/src/views/system/menu` / function-module designer; keep camelCase JSON names.

### Common patterns
- Cr/Up/List/Output suffix; batch input is a separate class with a `formJson` collection.
- Helper inner classes (`Form`) declared next to the input that uses them.

## Dependencies
### Internal
- Consumed by `modularity/system/Poxiao.Systems` services and the visual / module designer modules.

### External
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
