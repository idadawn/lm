<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# static

## Purpose
Static file root for the mock server. `mock.js` mounts this directory via `koa-static`, so anything dropped here is served at the URL root of `http://localhost:19003`. Currently holds only a placeholder landing page used to verify the server is up.

## Key Files
| File | Description |
|------|-------------|
| `index.html` | Minimal HTML (`mock server!`) returned at `GET /` — sanity-check page. |

## For AI Agents

### Working in this directory
- Use this for ad-hoc fixtures (uploads, sample images, downloadable test files) needed by mocked endpoints. Reference them with absolute paths like `/sample.png`.
- Keep contents small — this is dev-only; do not commit production-sized binary fixtures.
- Sibling Cordova static path is also mounted in `mock.js` (`../src-cordova/platforms/ios/platform_www`) but that directory is not part of this repo by default.

## Dependencies
### Internal
- Mounted by `../mock.js` via `koa-static`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
