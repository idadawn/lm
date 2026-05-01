<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# kpi

## Purpose
Tiny iconfont set scoped to the KPI / indicator-define feature. Holds only a CSS file; this set reuses glyphs from a shared font face declaration and only adds class-to-codepoint mappings for KPI-specific icons.

## Key Files
| File | Description |
|------|-------------|
| `iconfont.css` | ~0.7KB — `.icon-*` class definitions for the KPI feature. |

## For AI Agents

### Working in this directory
- This set has **no woff/woff2/ttf** of its own — it relies on a font-face declared in a sibling iconfont (likely `../ym-custom/`). When adding new icons to KPI, first verify the underlying glyph exists in the shared font; otherwise add to `../ym-custom/` instead.
- Aggregated through `../index.less`; no direct import elsewhere.
- Iconfont.cn-generated; replace wholesale on regeneration, do not hand-edit codepoints.

## Dependencies
### Internal
- Imported via `../index.less`; depends on font face from a sibling iconfont set.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
