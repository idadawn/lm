<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# routes

## Purpose
Koa-router wiring for the mock API server. Maps URL constants to canned data from `../data/`. Two registration styles: a JSON-driven simple list (`response.json` → `index.js`) and per-feature configurator modules.

## Key Files
| File | Description |
|------|-------------|
| `index.js` | Builds the koa-router instance. First iterates `response.json` (registers `method/url/delay/response`), then invokes feature configurators `common`, `createModel`, `chart`. |
| `url.js` | Central URL registry (`getLoginConfig`, `Login`, `CurrentUser`, `SysConfig`, `getNodes`, `getNodeElements`, `getWarningTableList`, `getChartsDataList`, `getLayout`, `getDashTreeList`, `getMetricSchema`, etc.). One source of truth for endpoint paths. |
| `common.js` | Registers GET/POST handlers for auth, dictionary, system, dashboard nodes, charts — each handler simply sets `ctx.body = common[<key>]`. |
| `createModel.js` | Indicator-tree endpoints (`getIndicatorTreeList`, `getIndicatorValueChainList`, `getAllIndicatorList`). |
| `chart.js` | Chart-specific routes pulling from `../data/chart.js`. |
| `response.json` | Tabular list of trivial `{ method, url, delay, response }` mocks consumed by `index.js`. |

## For AI Agents

### Working in this directory
- To add a new endpoint: (1) declare path in `url.js` as a key, (2) put data in `../data/<feature>.js` under the same key, (3) register `router.get|post(url.<key>, ...)` in the matching configurator. For trivial fixed responses, append a row to `response.json` instead.
- Handlers use `async ctx => { ctx.body = ... }`; do not return — Koa uses ctx mutation.
- `url.js` has duplicate keys (`getAllIndicatorList` is defined twice) — last definition wins; do not depend on key order.
- Dynamic params: koa-router `:id` style — see `getLayout: '/api/kpi/v1/metricdash/:id'`.

### Common patterns
- All URLs begin with `/api/` to match the path the SPA expects after the `/dev/mock` proxy strip.
- Response delay can be added via `helper.sleep(ms)` in the handler.

## Dependencies
### Internal
- `../helper.js` for `sleep`.
- `../data/*.js` for response payloads.
### External
- `koa-router`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
