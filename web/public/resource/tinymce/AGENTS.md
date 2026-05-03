<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# tinymce

## Purpose
Self-hosted assets for the TinyMCE 5 rich-text editor wrapper used by article/template/notice views. Bundling these locally avoids fetching from the TinyMCE CDN at runtime (and works in offline-deploy environments). Loaded by the editor wrapper component at `src/components/Tinymce/`.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `langs/` | i18n language packs (`en.js`, `zh_CN.js`) (see `langs/AGENTS.md`). |
| `skins/` | UI skins (oxide light + oxide-dark) (see `skins/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Versioned to TinyMCE 5.10.x — must match the `tinymce@^5.10.7` npm dep so init code stays compatible.
- TinyMCE init must point `skin_url` at `/resource/tinymce/skins/ui/oxide` (or `oxide-dark`) and `language_url` at `/resource/tinymce/langs/zh_CN.js`. Do not relocate these subdirs.
- These files come from the upstream TinyMCE distribution — treat as vendor content; replace wholesale on version upgrades, do not hand-edit.

## Dependencies
### Internal
- Consumed by `src/components/Tinymce/` editor wrapper.
### External
- TinyMCE 5.10.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
