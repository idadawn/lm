<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# mitt

## Purpose
Mitt-event-bus producers/consumers for cross-component signals that don't fit the Pinia or router model. Currently only emits a `routeChange` event so listeners (breadcrumbs, page title, transition) can react without coupling to vue-router internals.

## Key Files
| File | Description |
|------|-------------|
| `routeChange.ts` | Wraps `mitt` with strongly-typed `setRouteChange` / `listenerRouteChange` — fired from a router `afterEach` guard. |

## For AI Agents

### Working in this directory
- Prefer this bus only for very lightweight one-shot notifications. For state, use Pinia.
- Listeners should call the returned `off` function on `onUnmounted` to avoid leaks.

### Common patterns
- Uses the shared `/@/utils/mitt` factory; namespace events with a stable string key.

## Dependencies
### Internal
- `/@/utils/mitt`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
