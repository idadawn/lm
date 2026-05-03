<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Application

## Purpose
Application-level UI shell components mounted high in the layout tree: the `AppProvider` that injects breakpoint/mobile state via Vue provide/inject, plus standalone widgets for logo, locale switcher, dark-mode toggle, and a global menu search modal.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Barrel exporting `AppLogo`, `AppProvider`, `AppSearch`, `AppLocalePicker`, `AppDarkModeToggle` (all `withInstall`-wrapped) and re-exports `useAppProviderContext`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Component implementations + provider context hook (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Add new app-level shell widgets as `App<Name>.vue` under `src/`, then export from `index.ts` via `withInstall`.
- Provider context state (e.g., `isMobile`) flows through `useAppContext.ts` — read with `useAppProviderContext()`, do not inject directly.

### Common patterns
- `withInstall(component)` re-export pattern.
- Breakpoint listening via `createBreakpointListen` from `/@/hooks/event/useBreakpoint`.

## Dependencies
### Internal
- `/@/hooks/event/useBreakpoint`, `/@/store/modules/app`, `/@/settings/designSetting`, `/@/enums/menuEnum`, `/@/utils` (`withInstall`).
### External
- `vue@3.3`, `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
