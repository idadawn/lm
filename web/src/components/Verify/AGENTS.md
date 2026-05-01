<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Verify

## Purpose
Human-verification widgets used during login / sensitive flows. Provides a slide-to-verify bar (`BasicDragVerify`) and a rotate-the-image puzzle (`RotateDragVerify`).

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall`-wrapped exports `BasicDragVerify`, `RotateDragVerify`, plus typings re-export. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Component implementations, prop schema, and typings (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Both components emit `success`, `change`, and `update:value` so they can be `v-model`'d as a boolean pass flag.
- `success` payload includes `time` (seconds, 1 decimal) — preserve this shape; analytics may consume it.
- I18n strings come from `component.verify.*`; keep new defaults in `props.ts` using `useI18n().t(...)`.

### Common patterns
- TSX `defineComponent` with `reactive` state for drag positions/timing.
- DOM event wiring via `useEventListener` and timeouts via `useTimeoutFn`.

## Dependencies
### Internal
- `/@/utils/index` (`withInstall`), `/@/utils/domUtils` (`hackCss`)
- `/@/hooks/core/useTimeout`, `/@/hooks/event/useEventListener`, `/@/hooks/web/useI18n`
- `/@/utils/helper/tsxHelper`

### External
- `vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
