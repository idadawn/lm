<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dto

## Purpose
DTOs for the Apps (应用门户) module — the lightweight payload shapes used by `Poxiao.Apps` services and controllers to expose application catalog, application tree, frequently-used app data, and per-user app/menu information to the frontend portal. All inputs/outputs use camelCase property names matching the Vue 3 frontend API contract.

## Key Files
| File | Description |
|------|-------------|
| `AppDataCrInput.cs` | Create input for "常用应用数据" (`objectType` / `objectId` / `objectData`) — records that an app has been pinned/used. |
| `AppDataListAllOutput.cs` | List output for all apps with isData flag indicating whether the current user has marked it as frequent; extends `TreeModel`. |
| `AppDataListOutput.cs` | Slim list output of frequently-used app entries. |
| `AppFlowListAllOutput.cs` | List output of available flow/workflow apps surfaced inside the App portal. |
| `AppMenuListOutput.cs` | Minimal menu node output for App-side menu rendering. |
| `AppTreeOutput.cs` | Hierarchical app tree output (`enCode`, `fullName`, `formType`, `category`, `iconBackground`, `visibleType`) — the canonical app catalog tree shape. |
| `AppUserInfoOutput.cs` | Current-user profile fields surfaced to the App home (`realName`, `organizeName`, `positionName`, `mobilePhone`, `headIcon`). |
| `AppUserOutput.cs` | List output of users for App-scoped pickers. |

## For AI Agents

### Working in this directory
- DTO property names are intentionally **camelCase** (e.g. `fullName`, `enCode`, `isData`) to match the JS/TS frontend — do **not** rename to PascalCase.
- Every public DTO type is decorated with `[SuppressSniffer]` from `Poxiao.DependencyInjection` so it is excluded from auto DI registration; keep this attribute on new DTOs.
- Tree-shaped outputs inherit `Poxiao.Infrastructure.Security.TreeModel`. Reuse it instead of redefining `id` / `parentId` / `children`.
- These DTOs are *entity-layer* (under `Poxiao.Apps.Entitys`) and must not reference service/controller projects.

### Common patterns
- XML doc comments on every property in Chinese (`/// <summary>名称.</summary>`).
- Nullable reference annotations (`string?`) for optional inputs/outputs.
- Output classes named `*Output`, input classes `*Input` (Cr=Create suffix observed: `AppDataCrInput`).

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- `Poxiao.Infrastructure.Security` — `TreeModel` base class.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
