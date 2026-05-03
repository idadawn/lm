<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Source for the transition wrappers. `CreateTransition.tsx` exposes the two factories; `ExpandTransition.ts` produces JS lifecycle hooks (beforeEnter/enter/afterEnter/leave/…) that animate `height` (or `width`) from 0 to `scrollHeight`; `CollapseTransition.vue` is a ready-made SFC built on the same JS-hook pattern.

## Key Files
| File | Description |
|------|-------------|
| `CreateTransition.tsx` | `createSimpleTransition(name, origin?, mode?)` (CSS-only) and `createJavascriptTransition(name, hooks, mode?)` (lifecycle-driven). |
| `ExpandTransition.ts` | Generator returning `beforeEnter/enter/leave/afterLeave` hooks; supports horizontal mode (the boolean second arg). |
| `CollapseTransition.vue` | Standalone SFC; mutates `style.height`, `paddingTop`, `paddingBottom`, restores `oldOverflow` after enter — used by `SimpleMenu` etc. |

## For AI Agents

### Working in this directory
- `createSimpleTransition` sets `transformOrigin` in `onBeforeEnter` so consumers can change origin per-instance via the `origin` prop.
- The `group` prop on simple transitions switches to `<TransitionGroup>` — required for v-for animations.
- Don't read DOM in `enter` synchronously without forcing reflow first; the existing implementation relies on the natural reflow between `beforeEnter` writes and `enter` reads.

## Dependencies
### Internal
- `/@/utils/helper/tsxHelper`, `/@/utils/domUtils`.
### External
- `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
