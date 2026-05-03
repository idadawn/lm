<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# oxide

## Purpose
TinyMCE 5 default light skin. Provides chrome (toolbar/menus) and content stylesheets for the iframe-mode, inline-mode, and mobile-mode editor variants.

## Key Files
| File | Description |
|------|-------------|
| `skin.min.css` | Main editor chrome stylesheet (~60KB) — toolbar, menus, dialogs in light theme. |
| `skin.mobile.min.css` | Mobile-mode chrome (~20KB). |
| `skin.shadowdom.min.css` | Shadow-DOM scoped chrome variant. |
| `content.min.css` | Stylesheet injected **inside** the editor iframe — formats body text in the editing area. |
| `content.inline.min.css` | Same, for inline-mode editor. |
| `content.mobile.min.css` | Same, for mobile mode. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `fonts/` | Glyph font for the mobile-mode UI (see `fonts/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Vendor files — replace as a unit when upgrading TinyMCE; do not edit individual `.min.css` files.
- Editor init: `skin: 'oxide'`, `skin_url: '/resource/tinymce/skins/ui/oxide'`, `content_css: '/resource/tinymce/skins/ui/oxide/content.min.css'`.
- Pairs with `../oxide-dark/` for theme switching.

## Dependencies
### External
- TinyMCE 5 oxide skin distribution.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
