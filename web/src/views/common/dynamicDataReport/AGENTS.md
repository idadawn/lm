<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dynamicDataReport

## Purpose
Iframe host for the external report engine. Reads the report id from `route.meta.relationId`, builds a URL of the form `${report}/preview.html?id=...&token=...&page=1&from=menu`, and renders it full-bleed inside the SPA chrome.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Single-component page — `defineOptions({ name: 'dynamicDataReport' })`, reads `useGlobSetting().report` for base URL and `getToken()` for auth, sets `state.url` in `onMounted` |

## For AI Agents

### Working in this directory
- The token is appended to the iframe URL as a query param. If you switch to header-based auth, the report engine must support either credentials in the iframe context or a same-origin proxy.
- `report` base URL comes from `web/.env.*` (`VITE_GLOB_REPORT_URL` or similar) via `useGlobSetting()`. Don't hard-code.
- The page exits early if no `relationId` is set on route meta — that's intentional; menu config errors should surface as a blank page rather than a 404 spam.

### Common patterns
- `useRoute()` is invoked inside `init()` rather than at top-level; safe in `onMounted` but be careful copying out — KeepAlive may already have a route reference.

## Dependencies
### Internal
- `/@/hooks/setting` (`useGlobSetting`), `/@/utils/auth` (`getToken`)
### External
- `vue`, `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
