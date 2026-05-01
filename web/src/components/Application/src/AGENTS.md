<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Source files for the `Application` package: app-shell widgets and the provide/inject context hook used by layouts to share breakpoint and mobile-mode state.

## Key Files
| File | Description |
|------|-------------|
| `AppProvider.vue` | Top-level provider; tracks `isMobile` via `createBreakpointListen` and exposes context through `createAppProviderContext`. |
| `AppLogo.vue` | Project logo + title (collapsible). |
| `AppLocalePicker.vue` | Language switcher tied to locale store. |
| `AppDarkModeToggle.vue` | Light/dark theme toggle. |
| `useAppContext.ts` | `createAppProviderContext` / `useAppProviderContext` provide-inject pair. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `search/` | Global menu search modal (see `search/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Keep these components stateless or backed by Pinia (`useAppStore`) — they are mounted once at layout root.
- When extending the provider, update both `useAppContext.ts` typings and `AppProvider.vue`'s `setup` provide call.

### Common patterns
- `defineComponent({ name: 'AppXxx', setup(props, { slots }) { ... } })`.
- Class prefix from `/@/settings/designSetting` (`prefixCls`).

## Dependencies
### Internal
- `/@/hooks/event/useBreakpoint`, `/@/store/modules/app`, `/@/enums/menuEnum`.
### External
- `vue@3.3`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
