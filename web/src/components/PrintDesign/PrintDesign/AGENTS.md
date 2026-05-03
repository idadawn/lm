<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# PrintDesign

## Purpose
TinyMCE editor wrapper specialised for the print-template designer. Loads the full TinyMCE plugin set (table, image, pagebreak, codesample, fullscreen, etc.), applies a custom toolbar tuned for print layout, and exposes a stable `tinymceId` per instance via `buildBitUUID`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `<textarea>` host; initialises `tinymce` with merged `init` + `plugins` + `toolbar`; teardown on `onBeforeUnmount`/`onDeactivated`. |
| `plugins.ts` | Single-string plugin manifest passed to TinyMCE init (advlist, autosave, codesample, table, pagebreak, imagetools, etc.). |
| `toolbar.ts` | Toolbar string with print-relevant buttons (fontsize, lineheight, pagebreak, table, fullscreen, codesample, ltr/rtl). |

## For AI Agents

### Working in this directory
- `plugins.ts` and `toolbar.ts` are flat strings (TinyMCE format) — preserve spacing; bars and pipes are syntactic.
- Plugin imports in `index.vue` (`tinymce/plugins/...`) must match `plugins.ts` entries; mismatch yields silent runtime no-ops.
- Editor language is driven by `useLocale` and `useAppStore`.

### Common patterns
- Each editor instance gets a UUID DOM id to allow multiple coexisting editors.

## Dependencies
### Internal
- `/@/locales/useLocale`, `/@/store/modules/app`, `/@/utils/uuid` (`buildBitUUID`).
### External
- `tinymce` + theme/icons + ~30 plugins (advlist, table, image, fullscreen, codesample…).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
