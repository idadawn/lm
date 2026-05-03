<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
The single-file Vue 3 implementation of the virtual-scroll list.

## Key Files
| File | Description |
|------|-------------|
| `VirtualScroll.vue` | TSX `defineComponent('VirtualScroll')` — props: `height/maxHeight/maxWidth/minHeight/minWidth/width`, `bench`, required `itemHeight`, `items`. Computes first/last indices from `scrollTop`, listens for `scroll` via `useEventListener`, renders only `items[first..last]` inside a translateY spacer wrapper. |

## For AI Agents

### Working in this directory
- `convertToUnit(str, unit='px')` accepts numeric or string sizes; use it for any new size prop.
- `getFirstToRenderRef` / `getLastToRenderRef` clamp around `state.first/last` with `bench`; keep the `Math.max(0, ...)` and `Math.min(items.length, ...)` guards.
- Internal prefix class is `'virtual-scroll'`; styles are not scoped here, callers can theme via that class.

### Common patterns
- `reactive` `{ first, last, scrollTop }` plus `computed` derived ranges.
- `nextTick`/`onMounted` to sync layout after the wrapper element mounts.

## Dependencies
### Internal
- `/@/hooks/event/useEventListener`, `/@/utils/helper/tsxHelper` (`getSlot`)

### External
- `vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
