<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# routes

## Purpose
Static route definitions registered into the router at boot — the always-present scaffold (login, redirect, 404, root, error log) plus the optional per-module bundles that ship pre-known views (currently `lab/` 化验室驾驶舱 / 指标维护 and `ai/` 自然语言查询入口).

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Aggregates `mainOutRoutes`, `PAGE_NOT_FOUND_ROUTE`, `REDIRECT_ROUTE`, `ERROR_LOG_ROUTE`, and `LoginRoute` into the exported `basicRoutes`. |
| `basic.ts` | Defines layout-less + 404 + redirect + error-log routes (used by router internals + dynamic 404 fallback in permissionGuard). |
| `mainOut.ts` | Routes rendered without the main layout (e.g. external-link previews). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `modules/` | Static feature route modules (lab, ai) merged into `basicRoutes` (see `modules/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Most feature routes come from the backend menu — only put a route here if it must exist before login or is the explicit static set for this fork.
- 404 route uses a catch-all `path: '/:path(.*)*'` and depends on `permissionGuard` adding it last after dynamic routes.
- `REDIRECT_ROUTE` is the canonical re-render trigger used by `useRedo`; do not rename `'RedirectTo'`.

### Common patterns
- Route titles use the `t('routes.xxx')` i18n helper; keys live in `web/src/locales/lang/*/routes.ts`.

## Dependencies
### Internal
- `/@/router/constant` (LAYOUT, EXCEPTION_COMPONENT), `/@/hooks/web/useI18n`, `/@/router/types`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
