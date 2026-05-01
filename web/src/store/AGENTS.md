<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# store

## Purpose
Pinia root for the LIMS web app. Provides `setupStore(app)` (registered from `App.vue` setup) and re-exports the shared `store` instance. Per-domain stores live under `modules/` and are imported lazily as needed.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `createPinia()` instance + `setupStore(app)`; the singleton consumed by `defineStore({ store })` calls in modules. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `modules/` | Domain Pinia stores: app, user, permission, base, organize, multipleTab, locale, lock, errorLog, chart, generator (see `modules/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Bind `defineStore` to the exported `store` so the same Pinia instance is used outside Vue setup (`*WithOut` accessors).
- Avoid creating a second Pinia — that would lose persistence wiring done in modules.

### Common patterns
- Module entries follow `useXxxStore` / `useXxxStoreWithOut` pairs; the `WithOut` form is for guards/utilities that run before app mount.

## Dependencies
### External
- `pinia`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
