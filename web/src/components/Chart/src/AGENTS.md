<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of the `Chart` wrapper. Holds a `chartRef` div, instantiates `useECharts`, and re-applies `props.options` whenever they change.

## Key Files
| File | Description |
|------|-------------|
| `Chart.vue` | `<script setup>` with props `loading: boolean`, `width: string='100%'`, `height: string='300px'`, `options: object`; `watch(props.options, () => setOptions(props.options, false), { immediate: true, deep: true })`. |

## For AI Agents

### Working in this directory
- The second arg to `setOptions` (`false`) keeps merge mode — flip to `true` if you need a clean replacement.
- Resize handling lives in `useECharts`; do not add manual `addEventListener('resize', ...)` here.

### Common patterns
- `Ref<HTMLDivElement>` cast for `chartRef`.

## Dependencies
### Internal
- `/@/hooks/web/useECharts`.
### External
- `vue@3.3`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
