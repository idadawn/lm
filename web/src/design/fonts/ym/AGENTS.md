<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ym

## Purpose
Upstream `ym` iconfont distribution — the primary icon set used across the app. Originates from iconfont.cn; provides the base catalogue of CRUD/UI/file-type icons available via `class="icon-<name>"`.

## Key Files
| File | Description |
|------|-------------|
| `iconfont.css` | ~28KB — `@font-face` declaration plus class-to-codepoint mappings for the ym set. |
| `iconfont.woff2` | Primary glyph format (~65KB), modern browsers. |
| `iconfont.woff` | Fallback glyph format (~80KB). |
| `iconfont.ttf` | Legacy fallback (~133KB). |

## For AI Agents

### Working in this directory
- Treat as **vendor**. To add or change icons, regenerate the entire set from iconfont.cn and replace all four files together — partial replacements lead to font-name collisions.
- The `@font-face { font-family: 'iconfont' }` declared here is shared by sibling sets (e.g. `../kpi/`) — do not rename the font family.
- Aggregated via `../index.less`; never import directly from a component.
- Project customizations should go to `../ym-custom/` to avoid divergence from the upstream source.

## Dependencies
### Internal
- Loaded via `../index.less`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
