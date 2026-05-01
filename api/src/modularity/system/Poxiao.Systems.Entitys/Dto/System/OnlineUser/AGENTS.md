<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# OnlineUser

## Purpose
DTO(s) for the "在线用户" admin feature — currently only the batch-kick (force-logout) input.

## Key Files
| File | Description |
|------|-------------|
| `BatchOnlineInput.cs` | `BatchOnlineInput { List<string> ids }` — list of online-session IDs to terminate in one call. |

## For AI Agents

### Working in this directory
- The "list online users" output is not a custom DTO here — the controller returns the cached/Redis session model directly. Keep input DTOs only in this directory.
- Namespace `Poxiao.Systems.Entitys.Dto.OnlineUser`. `[SuppressSniffer]` applied.
- Property name `ids` (lowerCamel) — matches frontend payload.

### Common patterns
- Minimal "ids list" inputs are common across System module batch-delete-style endpoints.

## Dependencies
### Internal
- Consumed by `modularity/system/Poxiao.Systems` online-user service / controller.

### External
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
