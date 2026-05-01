<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# public

## Purpose
Vite `publicDir` — files copied verbatim into the dist root, addressable from the browser via absolute paths (`/favicon.ico`, `/config.js`, `/resource/...`). Hosts the runtime config shim used to override env vars without rebuilding, plus app branding and editor/emoji assets.

## Key Files
| File | Description |
|------|-------------|
| `config.js` | Runtime config loaded by `index.html` **before** `main.ts`. Sets `window.__PRODUCTION__LM__CONF__` with `VITE_GLOB_APP_TITLE`, `VITE_GLOB_API_URL`, `VITE_GLOB_APP_SHORT_NAME`, `VITE_GLOB_API_URL_PREFIX`, `VITE_GLOB_WEBSOCKET_URL`. **Replace at deploy time** — no rebuild needed. |
| `index.css` | Loading-screen styles shown before Vue mounts (`.app-loading`, `.dot` spinner, dark-mode tweaks for `[data-theme='dark']`). |
| `favicon.ico` | App favicon. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `resource/` | Editor and UI runtime assets — emoji GIFs, branding images, TinyMCE skins/langs (see `resource/AGENTS.md`). |
| `img/` | Branding images (login backgrounds, logos, PWA icons). |
| `cdn/` | Vendored CDN-style copies of select libs for offline use. |

## For AI Agents

### Working in this directory
- Files here are **NOT** processed by Vite — no imports, no hashing, no minification. Only place truly static assets (or runtime config) here.
- `config.js` is intentionally not bundled so ops can `sed` / overwrite it on the deployed nginx pod without invalidating cached JS bundles. Do **not** import it from `src/`; read `window.__PRODUCTION__LM__CONF__` instead (typically wrapped in `src/utils/env.ts`).
- TinyMCE expects its skins/langs at `/resource/tinymce/...` — preserve that path layout.
- The duplicate `web/resource/` directory is a legacy mirror — prefer adding new resources here.

### Common patterns
- Absolute paths only (`/resource/...`); never relative since `index.html` is at root.

## Dependencies
### Internal
- Loaded by `../index.html`; consumed by TinyMCE init in `src/components/Tinymce/`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
