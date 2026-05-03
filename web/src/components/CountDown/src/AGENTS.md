<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of the countdown widgets and their shared timer composable.

## Key Files
| File | Description |
|------|-------------|
| `CountButton.vue` | "Send verification code" button — props `count` (default 60), `value`, `beforeStartFunc`. Auto-resets when `value === undefined`; runs `beforeStartFunc` (with loading) before starting. |
| `CountdownInput.vue` | Input variant that pairs an `a-input` with the same countdown timer for inline verification flows. |
| `useCountdown.ts` | Composable: `setInterval`-based 1s ticker, exposes `start/stop/reset/restart/clear/currentCount/isStart`; auto-resets via `tryOnUnmounted`. |

## For AI Agents

### Working in this directory
- Timer cleanup is centralized — always reset through `stop()`/`reset()`; never call `clearInterval` directly on `timerId`.
- Button stays disabled (`isStart`) while ticking; do not bypass that state to "force resend".

### Common patterns
- `watchEffect(() => props.value === undefined && reset())` resets the button when the parent clears the bound model — preserve this convention.

## Dependencies
### Internal
- `/@/hooks/web/useI18n`, `/@/utils/is`.
### External
- `ant-design-vue` (Button), `@vueuse/core` (tryOnUnmounted).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
