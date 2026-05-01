<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# System

## Purpose
DTOs for the 应用系统 (multi-application registry) feature — each row is a logical "system/app" the platform exposes (主应用、子应用) with code、name、icon、order、status. Backs the system switcher and module portal.

## Key Files
| File | Description |
|------|-------------|
| `SystemCrInput.cs` | Create/edit input — `id`、`fullName`、`enCode`、`icon`、`enabledMark`、`description`、`sortCode`、`propertyJson` extensions. |
| `SystemListOutput.cs` | List row — adds `isMain` flag (0/1 for 主应用 marker). |
| `SystemQuery.cs` | List query filter. |

## For AI Agents

### Working in this directory
- Note this is `Dto/System/System/` (folder name doubles), and the namespace is `Poxiao.Systems.Entitys.Dto.System.System` — keep both halves when copying patterns.
- `enCode` is the unique business code; `propertyJson` carries arbitrary extension payload — don't strongly-type it here.
- `sortCode` is `long?` to match other modules' ordering convention.
- `[SuppressSniffer]` on all classes.

### Common patterns
- camelCase props; nullable types (`int?`, `long?`) used for optional inputs.
- One DTO per CRUD role: Cr (create/edit), List (output), Query (filter).

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- Pairs with the `BaseSystem` entity and system-switcher service.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
