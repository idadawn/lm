<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Transition

## Purpose
Reusable Vue `<Transition>` wrappers used across the UI. Provides a fluent factory pair — `createSimpleTransition` for CSS-only fade/scale/slide/scroll variants and `createJavascriptTransition` for measured (height/width) expand transitions — plus a standalone `CollapseTransition` SFC for accordion-style content.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `CollapseTransition`, plus `Fade`, `Scale`, `SlideY/X`(±Reverse), `ScrollY/X`(±Reverse), `ScaleRotate`, `Expand`, `ExpandX` transitions. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Factories and the collapse SFC (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- New named transitions go in `index.ts` via the factory; matching CSS classes (`.{name}-enter-active` etc.) must exist in the global stylesheet.
- `ExpandTransition` and `ExpandXTransition` use JS hooks because height/width animation needs measurement; do not convert them to CSS-only.

### Common patterns
- All factories return a `defineComponent` exposing `mode`, `origin`, `group` props as appropriate.

## Dependencies
### Internal
- `/@/utils/helper/tsxHelper` (`getSlot`), `/@/utils/domUtils` (`addClass`, `removeClass`).
### External
- `vue` (`Transition`, `TransitionGroup`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
