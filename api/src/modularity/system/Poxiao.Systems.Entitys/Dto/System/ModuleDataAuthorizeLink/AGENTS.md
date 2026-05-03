<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ModuleDataAuthorizeLink

## Purpose
DTOs for "功能数据权限" data-source / table-link bindings. Connect a function module's authorization rules to specific external data-source connections (`linkId`) and physical tables (`linkTables`), distinguishing 列表权限 (list) / 数据权限 (row) / 表单权限 (form).

## Key Files
| File | Description |
|------|-------------|
| `ModuleDataAuthorizeLinkInfoOutput.cs` | Single-record output — `id`, `linkId` (data-source connection PK), `linkTables` (table name list), `dataType` ("1"=列表权限, "2"=数据权限, "3"=表单权限), `moduleId`. |
| `ModuleDataAuthorizeLinkTableOutput.cs` | Grid-shaped projection enumerating linked tables for the management UI. |

## For AI Agents

### Working in this directory
- `dataType` is a stringified integer ("1"/"2"/"3") rather than an enum; do not change to `int` without coordinating with the frontend grid filters.
- Namespace `Poxiao.Systems.Entitys.Dto.System.ModuleDataAuthorizeLink` (note: includes `.System.`) — if you copy a sibling DTO over, fix the namespace; the System module is inconsistent on this point.
- These two outputs are read-only — there is no Cr/Up DTO here; create/update flows reuse `ModuleDataAuthorizeScheme` payloads.
- `[SuppressSniffer]` is omitted on these classes (unlike most siblings); leave as-is unless the dynamic-API surface needs to change.

### Common patterns
- camelCase property names matching frontend.
- One class per file, no behaviour.

## Dependencies
### Internal
- Consumed by `modularity/system/Poxiao.Systems` controllers/services.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
