<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Internal sub-components for the BasicPopup wrapper. Currently contains the popup header used by drawer/modal-style popups, exposing back-icon, title slot, primary/continue/cancel actions, and toolbar slots (`insertToolbar`, `centerToolbar`, `appendToolbar`).

## Key Files
| File | Description |
|------|-------------|
| `PopupHeader.vue` | `BasicPopupHeader` — fixed-height header with title (`BasicTitle` + helpMessage), back arrow, OK/Continue/Cancel buttons; emits `close`/`ok`/`continue` to parent popup. |

## For AI Agents

### Working in this directory
- Header consumes `headerProps` from `../props`; do not re-declare props locally.
- Style uses LESS with `@{namespace}-basic-popup-header` prefix and `useDesign` hook — preserve the prefix scheme on edits.
- Toolbar slots are positional: `insertToolbar` (before OK), `centerToolbar` (between OK and Cancel), `appendToolbar` (after Cancel).

### Common patterns
- Loading/disabled wiring across confirm + continue buttons (cross-disable while loading).
- Chinese UI strings come through props (`okText`, `continueText`, `cancelText`).

## Dependencies
### Internal
- `/@/components/Basic` (BasicTitle), `/@/hooks/web/useDesign`, `../props` (headerProps).
### External
- `@ant-design/icons-vue` (`ArrowLeftOutlined`), `ant-design-vue` (`a-button`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
