<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Chart

## Purpose
Thin wrapper around the project's `useECharts` hook, exposing a `<Chart>` component that renders an ECharts instance into a sized div. `options`, `width`, `height`, and `loading` are reactive props; option changes are deep-watched and re-applied via `setOptions`.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `Chart = withInstall(chart)`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `Chart.vue` implementation (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- For richer chart usage (resize, theme), use `useECharts` directly; this component is intentionally minimal.
- Avoid baking chart-type defaults into the component — pass `options` from the consumer.

### Common patterns
- `withInstall` barrel.

## Dependencies
### Internal
- `/@/hooks/web/useECharts`.
### External
- `echarts`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
