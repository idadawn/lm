<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Authority

## Purpose
Single-component package providing the `<Authority>` slot wrapper used to gate UI based on the user's role/permission codes. Internally calls `usePermission().hasColumnP(value)` to decide whether to render its slot content.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `Authority = withInstall(authority)`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `Authority.vue` implementation (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Use as `<Authority :value="'sys:user:add'"><a-button .../></Authority>`. Empty `value` renders unconditionally.
- Permission semantics depend on the active permission mode (role vs. backend code) — see `usePermission` and project permission settings.

### Common patterns
- Pure render-prop / slot pattern via `getSlot(slots)`.

## Dependencies
### Internal
- `/@/hooks/web/usePermission`, `/@/utils/helper/tsxHelper` (`getSlot`), `/@/utils` (`withInstall`).
### External
- `vue@3.3`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
