<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Time

## Purpose
Self-updating time/date display component. Renders the input timestamp formatted as either an absolute date, datetime, or a relative phrase ("3 分钟前", "刚刚"); refreshes itself on a `useIntervalFn` tick controlled by the `step` prop.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `Time` via `withInstall`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `Time.vue` SFC (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `value` accepts `number | Date | string`; `mode` is `'date' | 'datetime' | 'relative'` (default `relative`).
- Default refresh interval is 60 seconds; lower with `step` for live clocks but consider re-render cost.
- I18n keys for relative phrases live in the translation tree consumed by `useI18n`.

### Common patterns
- Plug-in style API via `withInstall`.

## Dependencies
### Internal
- `/@/utils` (`withInstall`), `/@/hooks/web/useI18n`, `/@/utils/dateUtil`, `/@/utils/is`, `/@/utils/propTypes`.
### External
- `@vueuse/core` (`useIntervalFn`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
