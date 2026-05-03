<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# logics

## Purpose
Cross-cutting application bootstrap and side-effect modules — initial app config hydration, global error handling pipeline, route-change pub/sub, and theme/CSS-var manipulation. These run once during app setup or react globally to runtime events.

## Key Files
| File | Description |
|------|-------------|
| `initAppConfig.ts` | Hydrates project config from `localStorage` + `projectSetting`, applies dark mode / theme color / gray / colorWeak before app mount. Also clears obsolete storage keys. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `error-handle/` | Global Vue / script / promise / resource error handler that pushes into `errorLog` store (see `error-handle/AGENTS.md`). |
| `mitt/` | Mitt-based event bus subscriber for route change broadcasts (see `mitt/AGENTS.md`). |
| `theme/` | CSS variable / dark / gray / colorWeak update helpers (see `theme/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `initAppConfig` is invoked from `src/setup/index.ts` (or equivalent App.vue setup) — order matters: must run before mounting `<RouterView>` because permission/theme depend on it.
- New globally-applied behaviour (interceptors, listeners) belongs here, not in `utils/`.

### Common patterns
- Modules touch Pinia stores via `useAppStore` / `useLocaleStore` / `useErrorLogStoreWithOut`.
- Persistence goes through `/@/utils/cache/persistent`.

## Dependencies
### Internal
- `/@/store/modules/*`, `/@/settings/projectSetting`, `/@/utils/cache/persistent`, `/@/enums/appEnum`.
### External
- `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
