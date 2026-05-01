<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Tinymce

## Purpose
General-purpose TinyMCE rich-text editor. Provides a Vue wrapper around `tinymce@^5` with full plugin set (table, image, codesample, paste, fullscreen, …), a custom toolbar tuned for the LIMS UI, and an integrated image-upload sub-component that posts to the system upload endpoint.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `Tinymce` via `withInstall`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `Editor.vue`, `ImgUpload.vue`, `helper.ts`, `tinymce.ts` (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- This package is general-purpose; the print-template editor under `PrintDesign/PrintDesign/` is a separate, specialised wrapper — do not unify without auditing both call sites.
- Editor language follows the global app locale via `useLocale`; theme follows `useAppStore`.

### Common patterns
- Plug-in style API via `withInstall`.

## Dependencies
### Internal
- `/@/utils` (`withInstall`), `/@/locales/useLocale`, `/@/store/modules/app`, `/@/utils/uuid`.
### External
- `tinymce` v5 + plugins.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
