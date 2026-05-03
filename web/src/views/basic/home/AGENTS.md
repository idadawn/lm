<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# home

## Purpose
Dashboard / 首页 entry. Renders the user's configured visual portal (`PortalLayout`) when one exists, or a custom-page iframe/component when `linkType=1`, falling back to a `Default.vue` mock dashboard when no data. Hosts the side `Setting` drawer that lets the user pick a portal.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Main entry — uses `usePortal()` state, renders `PortalLayout` / iframe / `Default`, opens `Setting` drawer |
| `secondIndex.vue` | Alternative second-level dashboard layout |
| `Default.vue` | Fallback dashboard (no portal data) — embeds `SiteAnalysis` |
| `OriginalDefault.vue` | Original demo dashboard (GrowCard + multiple chart components) |
| `Setting.vue` | Drawer to pick / refresh the portal `id` |
| `data.ts` | `growCardList` mock data (访问数/成交额/下载数/成交数) |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | All home charts and supporting widgets (see `components/AGENTS.md`) |
| `hooks/` | `usePortal` shared state and portal lifecycle (see `hooks/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Portal data flows through `usePortal()` (module-level `reactive` state — singleton). Avoid replacing with per-component state; siblings rely on the shared instance.
- `OriginalDefault.vue` retains demo charts (`GrowCard`, `SalesProductPie`, `VisitRadar` …) — kept as reference, do not delete unless removing routes.
- The setting button is fixed `position: fixed; right: 0; top: 300px` — z-index 100 to stay above grid layout.

### Common patterns
- `defineAsyncComponent + importViewsFile` to lazy-load custom portal pages by URL.
- Auto-refresh: per-element `setInterval` registered in `state.timerList`, cleared on `onUnmounted`.

## Dependencies
### Internal
- `/@/components/VisualPortal/Portal/Layout`, `/@/components/Container` (`ScrollContainer`), `/@/components/Drawer`, `/@/store/modules/user`, `/@/api/onlineDev/portal`, `/@/api/systemData/dataInterface`
### External
- `vue`, `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
