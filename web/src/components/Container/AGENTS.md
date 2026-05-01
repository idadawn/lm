<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Container

## Purpose
Layout container components: a `LazyContainer` for deferring expensive content until visible/triggered, a `ScrollContainer` wrapping a perfect-scrollbar-style scrollable region, and a `Collapse` family used by panels and sidebars.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Barrel exporting `LazyContainer`, `ScrollContainer`, `CollapseContainer` (typically via `withInstall`). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `LazyContainer.vue`, `ScrollContainer.vue`, `typing.ts`, and `collapse/` (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Use `LazyContainer` to gate echarts/large lists by visibility; do not place above-the-fold content inside it.
- `ScrollContainer` standardizes scrollbar styling — prefer it over raw `overflow:auto` for consistent UX.

### Common patterns
- `withInstall` barrel.

## Dependencies
### External
- `ant-design-vue` (`Collapse`, `Skeleton`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
