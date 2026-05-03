<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dynamicModel

## Purpose
Runtime host for visual-dev models. Loads a model configuration via `getConfigData(modelId, {type})` and dispatches to either the single-record `form/` view (when `webType === '1'`) or the list `list/` view. Supports preview mode via `?isPreview=true&id=...`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Dispatcher — reads `route.query.isPreview`, `route.query.id` (preview) or `route.meta.relationId` (production), `markRaw`s `Form`/`List`, redirects to `/404` on bad/missing config |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `form/` | Single-record entry form, with optional workflow gating (see `form/AGENTS.md`) |
| `list/` | Tabular list view for the same model |

## For AI Agents

### Working in this directory
- `useTabs().close()` is invoked when the config request fails — necessary because the bad route would otherwise stay in the tab bar. Keep the call before `router.replace('/404')`.
- `markRaw` on the lazily-imported components is required so Vue does not deeply react to component definitions.
- The `webType === '1'` magic string is the form/list discriminator. If you add new view types, extend the switch — do not branch on truthiness.

### Common patterns
- Pass `:config :modelId :isPreview` down to whichever view is mounted.

## Dependencies
### Internal
- `/@/api/onlineDev/visualDev` (`getConfigData`), `/@/hooks/web/{useMessage,useTabs}`
### External
- `vue`, `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
