<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# langs

## Purpose
TinyMCE 5 i18n language packs. Loaded at editor init time via `language_url`, populates the toolbar/menu strings.

## Key Files
| File | Description |
|------|-------------|
| `en.js` | English language pack (~16KB), upstream TinyMCE distribution. |
| `zh_CN.js` | 简体中文 language pack (~17KB) — default for the LIMS UI which is Chinese-first. |

## For AI Agents

### Working in this directory
- Vendor files — replace on TinyMCE version bumps; avoid hand-editing translations (translations come from the official TinyMCE language site).
- The editor init in `src/components/Tinymce/` must set `language: 'zh_CN'` (or `'en'`) and `language_url: '/resource/tinymce/langs/zh_CN.js'`. Do not rename files.

## Dependencies
### External
- TinyMCE 5 (the language file is a `tinymce.addI18n('<lang>', {...})` call shape).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
