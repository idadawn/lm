<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# factory

## Purpose
Vue async-component factory. Wraps `defineAsyncComponent` with sensible defaults — Spin loading slot, 30s timeout, and 3-attempt retry on `fetch` errors — used to lazy-load large feature pages without flicker.

## Key Files
| File | Description |
|------|-------------|
| `createAsyncComponent.tsx` | `createAsyncComponent(loader, options)` — options: `size` (small/default/large), `delay` (default 100ms), `timeout` (default 30000ms), `loading`, `retry`. |

## For AI Agents

### Working in this directory
- Use this for chunks where a Spin is acceptable; for tiny utility components prefer plain `defineAsyncComponent`.
- Retry only on `error.message.match(/fetch/)` — adjust the regex if you need broader retries.
- `loadingComponent` is rendered as JSX (file is `.tsx`); keep TSX semantics if extending.

### Common patterns
- Imports `Spin` from `ant-design-vue`; matches the global UI library for visual consistency.

## Dependencies
### Internal
- `/@/utils` (`noop`).
### External
- `vue`, `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
