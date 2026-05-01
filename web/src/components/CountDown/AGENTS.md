<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CountDown

## Purpose
Countdown UI primitives — primarily the SMS/email "send code" button (`CountButton`) and a numeric input variant (`CountdownInput`). Used by login, security, and verification flows.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Barrel: `CountdownInput` and `CountButton`, both wrapped via `withInstall` for plugin-style registration. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Components and the `useCountdown` composable (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Both components share `useCountdown(count)`; reuse it instead of writing new timers.
- Default count is 60s; `CountButton` accepts `beforeStartFunc: () => Promise<boolean>` so callers can validate (e.g. phone format) before triggering the timer.

### Common patterns
- i18n keys `component.countdown.normalText` / `component.countdown.sendText` drive button label.

## Dependencies
### Internal
- `/@/utils` (withInstall), `/@/hooks/web/useI18n`.
### External
- `ant-design-vue` (Button), `@vueuse/core`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
