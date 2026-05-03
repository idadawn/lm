<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# img

## Purpose
Public branding images shipped with the SPA. Includes the loading-screen logo loaded by `index.html`, login-page split background, and PWA installation icons.

## Key Files
| File | Description |
|------|-------------|
| `logo.png` | Main app logo (~80KB), used by loading screen via `<img src="./resource/img/logo.png">` in `web/index.html`. |
| `logo-128.png` | 128px logo variant. |
| `logo1.png` | Alternate small logo. |
| `login-split-bg.png` | Login page hero/split background (~660KB). |
| `pwa-192x192.png` | PWA install icon (192×192). |
| `pwa-512x512.png` | PWA install icon (512×512), maskable. |

## For AI Agents

### Working in this directory
- Asset-only. Replacing `logo.png` changes the loading screen; preserve dimensions or update CSS `.app-loading-logo { width: 90px }` in `../../index.css`.
- PWA icons are referenced by the manifest generated via `vite-plugin-pwa`; renaming requires updating that plugin config.
- A legacy mirror exists at `web/resource/img/` — keep both in sync if a referencing path is unclear.
- Do not commit unoptimized large PNGs — run through imagemin (already wired into the build for `img/` paths).

## Dependencies
### Internal
- Loaded by `../../../index.html` (loading screen) and login page components in `src/views/`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
