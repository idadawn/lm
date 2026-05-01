<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ym-custom

## Purpose
Project-specific iconfont customizations layered on top of `../ym/`. Holds the larger of the two icon sets (~100KB of class definitions). New project-only icons (lab/kpi/workflow specific) go here, leaving the upstream `ym` set untouched for cleaner upgrades.

## Key Files
| File | Description |
|------|-------------|
| `iconfont.css` | ~101KB — full project-customized icon class definitions. |
| `iconfont.woff2` | Custom glyph font (~122KB), modern browsers. |
| `iconfont.woff` | Fallback (~158KB). |
| `iconfont.ttf` | Legacy fallback (~311KB). |

## For AI Agents

### Working in this directory
- Vendor-style file-set: regenerate from iconfont.cn (project's own icon repository) and replace all four files together — never hand-edit codepoints.
- Aggregated via `../index.less` — order matters: `ym-custom` follows `ym`, so a same-named class in this set wins.
- When adding a new icon, prefer this set over `../ym/` to keep the upstream baseline reusable.
- Watch for size — this is already large; remove unused icons during regeneration.

## Dependencies
### Internal
- Loaded via `../index.less`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
