<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# theme

## Purpose
Runtime theme manipulation. Sets CSS variables for header / sider background, dark mode, color-weak, and gray (mourning) modes via DOM `documentElement` style updates. Also coordinates light/dark menu theme based on header bg luminance.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Re-exports + `changeTheme(themeColor)` entry that recomputes ant-design palette. |
| `dark.ts` | `updateDarkTheme(mode)` toggles `<html data-theme>` and `dark` class. |
| `updateBackground.ts` | `updateHeaderBgColor` / `updateSidebarBgColor` — derives hover/active variants via `lighten/darken`. |
| `updateColorWeak.ts` | Toggles a `body.color-weak` class with CSS filter. |
| `updateGrayMode.ts` | Toggles `body.gray-mode` for grayscale (e.g. national mourning days). |
| `util.ts` | `setCssVar(name, value)` helper. |

## For AI Agents

### Working in this directory
- Pair every CSS-var name introduced here with its declaration in `web/src/design/` global stylesheets.
- These functions are invoked from `logics/initAppConfig.ts` and the Setting drawer; keep idempotent.

### Common patterns
- Lighten/darken via `/@/utils/color`.
- Reads/writes `useAppStore` to keep persisted config in sync with DOM.

## Dependencies
### Internal
- `/@/utils/color`, `/@/store/modules/app`, `/@/enums/appEnum`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
