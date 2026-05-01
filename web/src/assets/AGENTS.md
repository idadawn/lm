<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# assets

## Purpose
App-bundled static assets imported by source code (and therefore hashed/optimized by Vite). Distinct from `web/public/` which is served as-is. Holds rasterized images, vector icons, and SVG sprite sources used by `vite-plugin-svg-icons`.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `icons/` | Icon image files (PNG/JPG) imported by components. |
| `images/` | UI images: backgrounds (e.g. `other-login-bg-dark.png` referenced from `../design/theme.less`), illustrations, dashboard backdrops. |
| `svg/` | SVG sources auto-registered by `vite-plugin-svg-icons` into a sprite, consumed via the `<SvgIcon name="..." />` component. |

## For AI Agents

### Working in this directory
- Files in `svg/` are auto-collected — file basename becomes the icon name (e.g. `svg/edit.svg` → `<SvgIcon name="edit" />`). Keep filenames kebab-case and unique across all svg subfolders.
- Import images via `/@/assets/images/...` (alias) — Vite hashes them and the URL becomes a string. Do not reference these paths from `public/` or runtime config.
- Prefer SVG over PNG for line icons; only put bitmaps here when the design requires raster.
- Design CSS in `../design/` references several files here via relative `../assets/images/...` — preserve those filenames or update the Less.

### Common patterns
- Three-way split: `icons/` (raster icons, rare), `images/` (UI artwork), `svg/` (sprite icons). Most new icons should go to `svg/`.

## Dependencies
### Internal
- Consumed by `../components/`, `../views/`, and `../design/theme.less` (theme image refs).
### External
- `vite-plugin-svg-icons@^2.0.1`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
