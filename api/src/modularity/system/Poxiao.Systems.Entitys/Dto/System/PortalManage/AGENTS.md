<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# PortalManage

## Purpose
DTOs for "门户管理" — assigning a configured portal home page (`portalId`) to a sub-system (`systemId`) on a given platform (`WEB` or `APP`). Drives the per-system landing page selection in the admin UI.

## Key Files
| File | Description |
|------|-------------|
| `PortalManageCrInput.cs` | Create — `description`, `systemId`, `portalId`, `sortCode`, `enabledMark` (1-可用 / 0-不可用), `platform` ("WEB"/"APP"). |
| `PortalManageUpInput.cs` | Update payload (adds `id`). |
| `PortalManageInfoOutput.cs` | Detail / edit-prefill output. |
| `PortalManageListOutput.cs` | Grid projection — adds friendly names (system name, portal name) for display. |
| `PortalManageListQueryInput.cs` | Grid query — paging + filters such as `systemId`, `enabledMark`, keyword. |

## For AI Agents

### Working in this directory
- `platform` is a string with two valid values (`WEB` / `APP`); validate at the service layer, not in the DTO.
- Namespace `Poxiao.Systems.Entitys.Dto.System.PortalManage` (note: includes `.System.` segment — inconsistent with most siblings).
- `[SuppressSniffer]` applied to keep these off the dynamic API surface.
- For mobile (`APP`) entries, the consuming uni-app frontend in `mobile/` looks up `portalId` via the API; keep the property name stable.

### Common patterns
- Cr/Up/Info/List/Query suffix.
- `enabledMark` consistently uses 1=enabled / 0=disabled across the System module.

## Dependencies
### Internal
- Consumed by `modularity/system/Poxiao.Systems` portal management service.

### External
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
