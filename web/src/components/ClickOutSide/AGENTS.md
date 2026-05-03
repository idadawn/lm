<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ClickOutSide

## Purpose
Simple slot wrapper that emits a `clickOutside` event when a pointer-down occurs outside its rendered children. Used by modals/dropdowns/popovers (e.g., `AppSearchModal`) to close themselves on outside click.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `ClickOutSide = withInstall(clickOutSide)`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `ClickOutSide.vue` implementation (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Prefer this wrapper over a global `v-click-outside` directive when the trigger area is dynamic / slot-driven.

### Common patterns
- `withInstall` barrel.

## Dependencies
### Internal
- `/@/utils` (`withInstall`).
### External
- `vue@3.3`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
