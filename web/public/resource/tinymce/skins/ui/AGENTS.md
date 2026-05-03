<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ui

## Purpose
TinyMCE skin variants. Each subdirectory is a complete skin bundle (toolbar/menu CSS + content CSS) that can be selected via the editor's `skin` and `content_css` options.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `oxide/` | Default light skin — also contains a `fonts/` subdir for mobile glyphs (see `oxide/AGENTS.md`). |
| `oxide-dark/` | Dark-mode skin matching the app's `data-theme='dark'` (see `oxide-dark/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Editor init pattern: pick `oxide` or `oxide-dark` based on the global theme (`localStorage['__APP__DARK__MODE__']`).
- Both skins ship `skin.min.css`, `skin.mobile.min.css`, `skin.shadowdom.min.css`, `content.min.css`, `content.inline.min.css`, `content.mobile.min.css` — the editor selects the right one based on its mode (full/inline/mobile).
- These are vendor distributions; do not hand-edit.

## Dependencies
### External
- TinyMCE 5.10 oxide skin distribution.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
