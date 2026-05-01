<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Vue 3 + TypeScript application source for the Laboratory Data Analysis SPA. Standard JNPF/Vben-style layout: feature-first under `views/`, shared components under `components/`, with clean separation of routing, store, hooks, locales, and styles. Path alias `/@/` resolves to this directory.

## Key Files
| File | Description |
|------|-------------|
| `main.ts` | Bootstraps the app: imports `animate.css`, WindiCSS virtual modules, `design/index.less`, registers SVG sprites, sets up Pinia (`setupStore`), `initAppConfigStore`, global components, i18n, router + guard, global directives, error handler. Provides a `mitt` emitter via `app.provide('emitter')`. Mounts `#app`. Also wires a `window.message` listener that pushes to `/kpi/indicatorDefine/chartsTree` (FastGPT integration). |
| `App.vue` | Root component — wraps `RouterView` in `ConfigProvider` (locale) and `AppProvider`; calls `useTitle()` and `useLocale()`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `api/` | Typed HTTP client modules per backend module. |
| `assets/` | App-bundled assets (icons, images, svg sprites) (see `assets/AGENTS.md`). |
| `components/` | Shared Vue components (Application, BasicForm, BasicTable, charts, editors, etc.). |
| `composables/` | Vue 3 composables — color styles, formula precision (see `composables/AGENTS.md`). |
| `design/` | Less stylesheets, fonts (iconfont/ym/kpi), theme + transition definitions (see `design/AGENTS.md`). |
| `directives/` | Custom Vue directives (permission, loading, click-outside, etc.). |
| `enums/` | Shared TS enums (page status, biz codes). |
| `hooks/` | Reusable hooks (`web/useTitle`, `core/usePermission`, table/form helpers). |
| `layouts/` | Top-level layout shells (default/blank/iframe). |
| `locales/` | i18n setup (`setupI18n`, `useLocale`) and language packs. |
| `logics/` | App-level orchestration (init config, error handling, theme). |
| `router/` | Vue Router setup, guards, dynamic route registration. |
| `settings/` | Project/component settings constants. |
| `store/` | Pinia stores (user, app, permission, dictionary). |
| `types/` | Global TypeScript ambient types. |
| `utils/` | Shared utilities (http axios, mitt, env, dateUtil, cipher). |
| `views/` | Feature pages (lab/kpi/system/workflow/etc.). |

## For AI Agents

### Working in this directory
- Always import via the `/@/` alias, never relative `../../..` chains. Example: `import { defHttp } from '/@/utils/http/axios';`.
- New global state → Pinia store under `store/`. New cross-cutting hook → `hooks/`. Single-feature composable → `composables/` for newer Vue 3 idiom hooks. Older project hooks live under `hooks/`.
- Component naming: PascalCase files (`UserList.vue`); variables camelCase; CSS classes kebab-case; **no inline styles** (per project CLAUDE.md).
- UI strings, errors, notifications must be Chinese-first (this is the project default language).
- Dark-mode style: target `html[data-theme='dark']` selectors; do not branch on JS state.

### Common patterns
- Bootstrap order in `main.ts`: store → app config → global comps → i18n (await) → router → router-guard → directives → error handler → mount.
- HTTP via `defHttp` (`utils/http/axios`); responses are unwrapped to the `data` field.
- Lazy chunked routes loaded by `router/` and registered after permission resolution.

## Dependencies
### Internal
- Hits backend `../api/` via the `/dev` proxy at runtime.
### External
- Vue 3.3, Vue Router 4, Pinia 2.1, Ant Design Vue 3.2, VueUse 10, vue-i18n 9, ECharts 5.4, Highcharts 11, AntV G6, LogicFlow, Monaco 0.38, TinyMCE 5, mathjs, mitt, mqtt, sortablejs, vue-grid-layout.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
