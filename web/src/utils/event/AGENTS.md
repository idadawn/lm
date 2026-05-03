<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# event

## Purpose
DOM event helpers for layout-reactive components. Wraps `ResizeObserver` (with polyfill) into add/removeResizeListener API and ships a `triggerWindowResize()` shortcut used after layout changes (sider collapse, settings drawer toggle).

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `addResizeListener(el, fn)` / `removeResizeListener(el, fn)` / `triggerWindowResize()`. Stores per-element listener arrays + ResizeObserver under `__resizeListeners__` / `__ro__`. |

## For AI Agents

### Working in this directory
- Always pair `addResizeListener` with `removeResizeListener(el, fn)` in `onUnmounted` — there's no automatic cleanup.
- SSR-safe via `typeof window === 'undefined'` short-circuit; do not remove that guard.
- Used by Table / Chart components to reflow on container size changes.

### Common patterns
- Custom events are dispatched via `document.createEvent('HTMLEvents')` — kept for legacy IE-era reasons; harmless on modern browsers.

## Dependencies
### External
- `resize-observer-polyfill`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
