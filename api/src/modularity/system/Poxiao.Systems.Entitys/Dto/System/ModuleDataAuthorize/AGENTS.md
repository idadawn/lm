<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ModuleDataAuthorize

## Purpose
DTOs for the System module's "功能数据权限" (per-field row-level authorization rule) configuration — used to build dynamic data filters that restrict which rows / fields a role can read or write inside a function module.

## Key Files
| File | Description |
|------|-------------|
| `ModuleDataAuthorizeCrInput.cs` | Create payload — `moduleId`, `enCode` (field), `fullName` (label), `type`, `conditionSymbol`, `conditionText`, `description`, `fieldRule` (0:主表/1:副表), `bindTable`, `childTableKey`. |
| `ModuleDataAuthorizeUpInput.cs` | Update payload — same shape as create plus `id`. |
| `ModuleDataAuthorizeOutput.cs` | Detail output mirroring the entity. |
| `ModuleDataAuthorizeInfoOutput.cs` | Info-style projection for edit-form prefill. |
| `ModuleDataAuthorizeListOutput.cs` | List/grid projection — adds `conditionSymbolName` (display label) on top of the base condition fields. |

## For AI Agents

### Working in this directory
- All properties use camelCase (lowerCamel) JSON names by convention with the global `JsonSerializerOptions.PropertyNamingPolicy = null` in `Startup.cs` — keep property names lowerCamel exactly to match the frontend.
- Classes are decorated with `[SuppressSniffer]` (from `Poxiao.DependencyInjection`) so Furion's dynamic API discovery does not treat them as endpoints.
- `fieldRule` semantics (0=master table, 1=sub-table) are shared with `ModuleForm` DTOs; keep meanings consistent.
- These DTOs are paired with controllers/services in the parent `Poxiao.Systems` modularity project; only edit shapes here, not behaviour.

### Common patterns
- One DTO class per file, namespace `Poxiao.Systems.Entitys.Dto.ModuleDataAuthorize` (note: not `...Dto.System.ModuleDataAuthorize`).
- Cr/Up/Info/List/Output suffix convention.

## Dependencies
### Internal
- Consumed by `modularity/system/Poxiao.Systems` services and controllers.

### External
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
