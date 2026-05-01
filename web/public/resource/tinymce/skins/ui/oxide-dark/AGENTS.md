<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# oxide-dark

## Purpose
TinyMCE 5 dark skin variant — used when the app is in dark mode (`html[data-theme='dark']`). Mirror layout of `../oxide/` but without its own `fonts/` subdir (it shares glyphs with the light skin via the editor's font loader).

## Key Files
| File | Description |
|------|-------------|
| `skin.min.css` | Dark editor chrome stylesheet (~60KB). |
| `skin.mobile.min.css` | Mobile-mode dark chrome. |
| `skin.shadowdom.min.css` | Shadow-DOM scoped dark chrome. |
| `content.min.css` | In-iframe content styling (dark). |
| `content.inline.min.css` | Inline-mode content styling (dark). |
| `content.mobile.min.css` | Mobile content styling (dark). |

## For AI Agents

### Working in this directory
- Editor init: `skin: 'oxide-dark'`, `skin_url: '/resource/tinymce/skins/ui/oxide-dark'`, `content_css: '/resource/tinymce/skins/ui/oxide-dark/content.min.css'`.
- The TinyMCE wrapper component should switch skins based on `localStorage['__APP__DARK__MODE__']` / `htmlRoot.dataset.theme`.
- Vendor files; do not hand-edit.

## Dependencies
### External
- TinyMCE 5 oxide-dark skin distribution.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
