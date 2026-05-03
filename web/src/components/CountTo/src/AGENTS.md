<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Single-file animated counter component.

## Key Files
| File | Description |
|------|-------------|
| `CountTo.vue` | Renders `<span :style="{color}">{{ value }}</span>`. Props: `startVal/endVal/duration/autoplay/decimals/prefix/suffix/separator/decimal/color/useEasing/transition`. Emits `onStarted`, `onFinished`. Exposes `start()` and `reset()`. |

## For AI Agents

### Working in this directory
- Keep this file template-based (not TSX) — it is one of the few simple template SFCs in the components folder.
- `formatNumber` is internal; if you need formatting elsewhere extract to `/@/utils` rather than duplicating.

### Common patterns
- Auto-runs on mount when `autoplay`; subsequent prop changes also trigger `start()`.

## Dependencies
### Internal
- `/@/utils/is` (isNumber).
### External
- `@vueuse/core` (useTransition, TransitionPresets), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
