<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementations of the two verify widgets plus their shared prop schema and typings.

## Key Files
| File | Description |
|------|-------------|
| `DragVerify.vue` | `BaseDargVerify` — slide-bar verification. Tracks mouse/touch on the action handle, computes `moveDistance`, marks `isPassing` when drag reaches the wrapper width, supports circular handle, custom slot icon, success state via `CheckOutlined`. |
| `ImgRotate.vue` | `ImgRotateDragVerify` — wraps `BasicDragVerify`, generates a random rotate offset (`minDegree`–`maxDegree`), spins the image by drag percentage, passes when within `diffDegree` tolerance. |
| `props.ts` | `basicProps` (value/text/successText/height/width/circle and four style overrides) and `rotateProps` extending it with `src`, `imgWidth`, `imgWrapStyle`, `minDegree`, `maxDegree`, `diffDegree`. |
| `typing.ts` | `DragVerifyActionType` (with `resume`), `PassingData`, `MoveData` interfaces. |

## For AI Agents

### Working in this directory
- `DragVerify` exposes `resume()` via `expose` — `ImgRotate` and consumers may call it after a failed attempt.
- Drag uses both `mousemove`/`mouseup` and `touchmove`/`touchend`; keep both branches when refactoring.
- The success window in `ImgRotate` is `Math.abs(currentRotate - randomRotate) <= diffDegree` — mirror this if adding new puzzle modes.

### Common patterns
- `reactive` state for drag/rotation timing; `computed` style getters for wrap/bar/action.
- I18n default text resolved at module-load via `useI18n()`.

## Dependencies
### Internal
- `/@/hooks/core/useTimeout`, `/@/hooks/event/useEventListener`, `/@/hooks/web/useI18n`
- `/@/utils/helper/tsxHelper`, `/@/utils/domUtils`

### External
- `vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
