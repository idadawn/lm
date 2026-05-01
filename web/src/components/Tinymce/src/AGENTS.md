<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of the generic `Tinymce` editor. `Editor.vue` mounts and tears down a TinyMCE instance per Vue component instance (UUID id), `ImgUpload.vue` provides the inline image upload control hooked to TinyMCE's image plugin, `helper.ts` wires DOM listener events to Vue emits, and `tinymce.ts` declares the plugin/toolbar configuration.

## Key Files
| File | Description |
|------|-------------|
| `Editor.vue` | TinyMCE wrapper — props: `value` (v-model), `modelValue`, `toolbar`, `plugins`, `height`, `width`, `disabled`; emits `change`, `update:modelValue`, all TinyMCE events. |
| `ImgUpload.vue` | Custom image upload modal/button used by the `image` plugin's `images_upload_handler`. |
| `helper.ts` | `bindHandlers(initEvent, listeners, editor)` — whitelists ~50 valid TinyMCE event keys (`onActivate`, `onChange`, `onPaste`…) and delegates to the editor's event bus. |
| `tinymce.ts` | `plugins` array (`advlist anchor autolink … wordcount`) and `toolbar` rows (fontsize, lineheight, alignment, codesample…). |

## For AI Agents

### Working in this directory
- When adding a new TinyMCE event passthrough, also add the camelCase key to the `validEvents` array in `helper.ts`, otherwise it is silently filtered.
- `tinymce.ts` `plugins` is an array of one space-separated string by TinyMCE convention — keep that shape.
- Each editor uses `buildBitUUID` for its DOM id so multiple instances coexist.

## Dependencies
### Internal
- `/@/locales/useLocale`, `/@/store/modules/app`, `/@/utils/uuid`.
### External
- `tinymce` v5 + ~30 plugin imports.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
