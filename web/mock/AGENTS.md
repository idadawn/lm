<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# mock

## Purpose
Standalone Koa-based mock API server for frontend-only development. Listens on **port 19003** and is reached by Vite via the `/dev/mock` proxy entry. Allows the SPA to run without the .NET backend by serving canned responses for `/api/oauth`, dashboards, KPI metrics, indicator trees, and chart data.

## Key Files
| File | Description |
|------|-------------|
| `mock.js` | Koa entrypoint — wires `koa-body` (multipart on), permissive CORS (`*`), `koa-static` for `static/` and a placeholder Cordova path, plus `routes/` router. Listens on 19003. |
| `helper.js` | One-liner `sleep(timeout)` Promise utility used to inject artificial response delay. |
| `package.json` | Run script `pnpm mock` (uses `nodemon` per `nodemon.json`). Uses Koa 2, koa-router, koa-body, koa-static, faker, fs-extra, mysql, ip. |
| `nodemon.json` | Nodemon watch config for hot-reload on route/data edits. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `routes/` | URL → handler glue (declarative `response.json` + per-feature configurators) (see `routes/AGENTS.md`). |
| `data/` | Canned JSON payloads (login, common, charts, indicator trees) (see `data/AGENTS.md`). |
| `static/` | Static files served at root (placeholder `index.html`) (see `static/AGENTS.md`). |

## For AI Agents

### Working in this directory
- This is a **separate npm project** with its own `package.json`/`node_modules` — install via `npm install` inside `web/mock`, not pnpm. CLAUDE.md command: `cd web/mock && npm install && npm run mock`.
- Add new endpoints by either (a) adding a row to `routes/response.json`, or (b) declaring the URL in `routes/url.js` and registering a handler in `routes/common.js` / `routes/createModel.js` / `routes/chart.js` that pulls from `data/*.js`.
- This server is independent from the .NET backend — never share types/code with `api/`.
- CORS is fully open (`*`) — only intended for local dev.

### Common patterns
- Response shape: `{ code: 200, msg: '操作成功', data: ..., extras: null, timestamp: <unix> }`.
- All URLs prefixed with `/api/...` to match real backend routes after proxy strip.

## Dependencies
### External
- Koa 2, koa-router, koa-body, koa-static, nodemon, faker, ip, fs-extra, mysql.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
