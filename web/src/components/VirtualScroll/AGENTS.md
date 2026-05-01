<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# VirtualScroll

## Purpose
Lightweight virtual-scroll list. Renders only the visible window of a fixed-height item array, used to keep large lists (audit logs, big trees-as-lists, etc.) responsive.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Default export of `VScroll = withInstall(VirtualScroll.vue)`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Single-file `VirtualScroll.vue` implementation (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `itemHeight` is required and assumed fixed; the component does not measure variable rows.
- Render slot must produce DOM that respects `itemHeight` exactly, otherwise the spacer math drifts.
- `bench` extends rendered range above and below the viewport — increase it for fast-scroll smoothness, not for layout fixes.

### Common patterns
- `withInstall` wrapper so the component can be globally registered if desired.

## Dependencies
### Internal
- `/@/utils/index` (`withInstall`)

### External
- `vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
