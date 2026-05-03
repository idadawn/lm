<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# mobile

## Purpose
uni-app (Vue 3 + DCloud) cross-platform mobile client for the 检测室数据分析系统 (Laboratory Data Analysis System / LIMS). Targets Android / iOS / H5 / WeChat mini-program. Reuses the .NET backend's OAuth and `/api/lab/*` dashboard endpoints with a `Poxiao-Origin: app` header (and `origin=app` form field on login) so the server can apply app-specific token policies. A dedicated NLQ-agent SSE channel powers the chat tab and the KG reasoning chain.

## Key Files
| File | Description |
|------|-------------|
| `App.vue` | Root component — runs `checkUpdate()` on launch and routes to `pages/index/index` if a token is already in storage. |
| `main.js` | Standard uni-app Vue 3 entry (`createSSRApp`). |
| `manifest.json` | DCloud appid `__UNI__446F191`, Android package `cn.emergen.lm`, version `1.0.1` (versionCode 101), required permissions for INTERNET / external storage. |
| `pages.json` | Routes (login / index / chat / mine / profile / password / kg-demo) and 3-tab tabBar (首页 / 对话 / 我的). |
| `uni.scss` | Global SCSS variables shared across pages. |
| `README.md` | Module overview, backend endpoint mapping, HBuilderX/MuMu run instructions, auto-update API contract. |
| `RELEASE-v1.0.1.md` | Release notes for the current published build. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `api/` | Backend API client wrappers (see `api/AGENTS.md`) |
| `components/` | Reusable Vue components, currently the KG reasoning chain (see `components/AGENTS.md`) |
| `pages/` | Tab pages and detail pages routed by `pages.json` (see `pages/AGENTS.md`) |
| `scripts/` | PowerShell build/upload pipeline for HBuilderX cloud-pack + Pgyer (see `scripts/AGENTS.md`) |
| `types/` | TypeScript ambient declarations synced from `nlq-agent` (see `types/AGENTS.md`) |
| `utils/` | HTTP, storage, MD5, SSE, update, date helpers (see `utils/AGENTS.md`) |

(Excluded from deepinit: `.hbuilderx/` IDE state and `static/` binary assets.)

## For AI Agents

### Working in this directory
- All UI strings stay in 简体中文; only section headers, code identifiers, and log lines are English.
- Login must always send `origin: 'app'` and password as MD5 hash to match the Web client's contract.
- The HTTP layer hard-codes `Poxiao-Origin: app` — do not strip this header when adding new requests.
- Token storage keys are `lm_app_token` / `lm_app_user_info`; reuse `utils/storage.js` rather than calling `uni.setStorageSync` ad hoc.
- Auto-update routes through `utils/update.js`; switching between Pgyer and self-hosted backend is controlled by `UPDATE_SOURCE`.
- Do not introduce npm-only build steps that break HBuilderX — the project must remain runnable from the IDE.

### Common patterns
- `<script setup>` (Composition API) on newer pages; classic Options API on `kg-reasoning-chain` and `kg-demo` (uni-app component compatibility).
- Numeric sizes in `rpx` for cross-resolution scaling.
- API modules export named functions ending in `Api` returning the unified `{ code, data, msg }` envelope.
- Conditional compilation `// #ifdef APP-PLUS` guards plus runtime calls so H5/MP builds still compile.

## Dependencies
### Internal
- `nlq-agent/` — SSE protocol consumed by `utils/sse-client.js`; `types/reasoning-protocol.d.ts` is auto-synced from `nlq-agent/packages/shared-types`.
- `web/src/views/lab/monthly-dashboard/` — visual reference for the dashboard page.
- `api/` (.NET backend) — provides `/api/oauth/*`, `/api/lab/*`, `/api/permission/Users/Current/*`.

### External
- `uni-app` Vue 3 (`@dcloudio/uni-app`), HBuilderX 3.7+ for cloud build.
- DCloud `plus.runtime` / `plus.downloader` / `plus.runtime.install` for OTA updates.
- Pgyer (蒲公英) `apiv2/app/check` and upload API for distribution.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
