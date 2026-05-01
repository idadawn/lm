<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
Composables used by the home dashboard. Currently a single `usePortal` that owns module-level `reactive` state for the active portal (id, layout, autorefresh timers, custom URL/component) and exposes lifecycle helpers used by `home/index.vue` and `Setting.vue`.

## Key Files
| File | Description |
|------|-------------|
| `usePortal.ts` | Singleton state + `initData` (loads `getAuthPortal`), `filterList` (drops items missing `'pc'` visibility), `initAutoRefresh` (per-element setInterval), `clearAutoRefresh`, `layoutUpdatedEvent` (persists layout via `UpdateCustomPortal`) |

## For AI Agents

### Working in this directory
- The `state` object is declared at module top level so all consumers share the same portal instance. If you add another portal page, do NOT import this — clone with a new local `reactive` instead, otherwise dashboards will collide.
- Auto-refresh timers are stored on `state.timerList` and cleared in `onUnmounted` of the consumer. Always pair `initData` with `clearAutoRefresh`.
- `filterList` mutates the array via `splice` while iterating — uses backwards index (`i--`). Don't refactor to map/filter without preserving correct removal semantics.

### Common patterns
- `defineAsyncComponent + importViewsFile(formUrl)` to load a custom portal Vue file by string path.
- Chart auto-refresh only triggers for `dataType === 'dynamic'` and recognised `jnpfKey` values (`barChart`/`lineChart`/`pieChart`/`radarChart`/`mapChart`).

## Dependencies
### Internal
- `/@/api/onlineDev/portal` (`getAuthPortal`, `UpdateCustomPortal`), `/@/api/systemData/dataInterface` (`getDataInterfaceRes`), `/@/utils` (`importViewsFile`)
### External
- `vue` (`reactive`, `defineAsyncComponent`, `markRaw`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
