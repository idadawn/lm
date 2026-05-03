<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# StrengthMeter

## Purpose
Password strength meter component. Wraps `Input.Password` and renders a 0–4 score bar driven by `@zxcvbn-ts/core`. Used by user/account forms to give live feedback as the user types.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `StrengthMeter` via `withInstall`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Single-file implementation `StrengthMeter.vue` (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Component emits `change(value)` and `score-change(score)` — the score is the raw zxcvbn 0–4 result.
- Slot-passthrough pattern forwards every `$slots` to the underlying `InputPassword` so callers can use prefix/suffix slots.

### Common patterns
- `withInstall` for global registration (vben-admin convention).

## Dependencies
### Internal
- `/@/utils` (`withInstall`), `/@/hooks/web/useDesign`, `/@/utils/propTypes`.
### External
- `ant-design-vue` (`Input.Password`), `@zxcvbn-ts/core`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
