<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Header and footer sub-components composed inside `BasicDrawer`. Kept separate so detail mode and standard mode can re-skin the title bar without rewriting the drawer body.

## Key Files
| File | Description |
|------|-------------|
| `DrawerHeader.vue` | Renders either `BasicTitle` (standard) or a detail-mode bar with `ArrowLeftOutlined` back button + `titleToolbar` slot. Props: `isDetail`, `showDetailBack`, `title`. Emits `close`. |
| `DrawerFooter.vue` | Footer button row: continue/cancel/ok buttons gated by `showContinueBtn/showCancelBtn/showOkBtn` plus `insertFooter`/`centerFooter`/`appendFooter`/`footer` named slots. Computes `lineHeight: calc(${height} - 1px)` for the absolute footer bar. Emits `close/ok/continue`. |

## For AI Agents

### Working in this directory
- These components are template-based SFCs (not TSX); they pull props from `../props.ts` (`footerProps`) — extend `props.ts` rather than redeclaring.
- Footer styling is absolute-positioned at the drawer bottom (height defaults to 60px). Maintain the LESS classes `@{namespace}-basic-drawer-footer` and `@{namespace}-basic-drawer-header`.

### Common patterns
- Custom `footer` slot fully overrides built-in buttons; ensure new slot keys are documented in `typing.ts`.

## Dependencies
### Internal
- `/@/components/Basic` (BasicTitle), `/@/hooks/web/useDesign`, `/@/utils/propTypes`.
### External
- `@ant-design/icons-vue` (ArrowLeftOutlined), `ant-design-vue`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
