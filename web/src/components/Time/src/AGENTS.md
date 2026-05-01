<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of `Time`. A single SFC computes the formatted string from `value` and `mode` whenever the value changes or the periodic timer fires. Relative mode buckets elapsed time into seconds/minutes/hours/days and falls back to `formatToDate` for older dates.

## Key Files
| File | Description |
|------|-------------|
| `Time.vue` | `<span>` displaying `date` ref; props `value` (number/Date/string), `step` (seconds, default 60), `mode` ('date'\|'datetime'\|'relative'). |

## For AI Agents

### Working in this directory
- Time bucket constants are local (`ONE_SECONDS`, `ONE_MINUTES`, `ONE_HOUR`, `ONE_DAY`) — do not import; the component is intentionally self-contained.
- `useIntervalFn(setTime, step * 1000)` runs unconditionally; pause manually if mounted in a long-lived but invisible context to avoid wakeups.
- Watcher on `value` is `immediate: true` — first render is the same code path as the timer tick.

## Dependencies
### Internal
- `/@/hooks/web/useI18n`, `/@/utils/dateUtil` (`formatToDate`, `formatToDateTime`, `dateUtil`), `/@/utils/is`, `/@/utils/propTypes`.
### External
- `@vueuse/core` (`useIntervalFn`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
