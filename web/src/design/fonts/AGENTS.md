<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# fonts

## Purpose
Self-hosted iconfont sets used across the app. Each subdirectory is one iconfont collection (CSS class definitions + woff/woff2/ttf glyph files). Aggregator `index.less` imports all three so any `<i class="icon-xxx">` works app-wide.

## Key Files
| File | Description |
|------|-------------|
| `index.less` | Imports `ym/iconfont.css`, `ym-custom/iconfont.css`, `kpi/iconfont.css`. Single entry pulled in by `../index.less`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `ym/` | Primary iconfont (~28KB CSS, full ttf/woff/woff2 set) — generated from iconfont.cn (see `ym/AGENTS.md`). |
| `ym-custom/` | Project-customized icons (~100KB CSS — large set) (see `ym-custom/AGENTS.md`). |
| `kpi/` | Tiny KPI-feature-specific icon set (~0.7KB CSS, no glyph files in this dir — references upstream font face) (see `kpi/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Adding a new icon: regenerate the matching iconfont set on iconfont.cn and replace the entire subdir contents (CSS + woff/woff2/ttf). Do not hand-edit `iconfont.css`.
- Use case for three sets: `ym` is the upstream JNPF iconfont (do not modify); `ym-custom` is the project's own customizations; `kpi` is feature-specific (KPI module).
- Reference icons in components via `class="icon-<name>"` — collisions across sets are possible; check ym-custom first when adding.
- Do NOT import these CSS files individually elsewhere — `index.less` is the only entry.

### Common patterns
- Each subdir uses iconfont.cn output convention: `iconfont.css` defines `@font-face { font-family: 'iconfont'; src: url(./iconfont.woff2)... }` and per-icon class rules like `.icon-foo:before { content: '\eXXX' }`.

## Dependencies
### Internal
- Imported by `../index.less`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
