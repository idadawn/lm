<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CountTo

## Purpose
Animated number tweener — animates from `startVal` to `endVal` with optional easing, separator, decimals, prefix/suffix. Used on dashboards and stat cards.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Barrel re-export of the `CountTo` component. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Single SFC implementation (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Animation is driven by `@vueuse/core` `useTransition` with `TransitionPresets[transition]`; pass a valid preset name (`linear`, `easeInOutCubic`, etc.).
- Component re-creates the transition on `run()` when `startVal`/`endVal` change with `autoplay`.

### Common patterns
- Number formatting uses a regex digit-grouping loop with `separator`/`decimal`; honor user `decimals` (validator: ≥0).

## Dependencies
### External
- `@vueuse/core` (useTransition, TransitionPresets), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
