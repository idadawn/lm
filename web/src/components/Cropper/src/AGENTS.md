<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of the image cropper, the modal-driven avatar editor, and shared option types.

## Key Files
| File | Description |
|------|-------------|
| `Cropper.vue` | Wraps `new Cropper(imgEl, ...)` with default options (1:1 ratio, zoom/move/rotate enabled). Emits `ready`, `cropend({imgBase64, imgInfo})`, `cropendError`. Supports `circled` mode via `getRoundedCanvas`. Debounced (80ms) realtime preview. |
| `CopperModal.vue` | Modal wrapper around `Cropper.vue` plus zoom/rotate toolbar; calls `uploadApi` and emits `upload-success`. |
| `CropperAvatar.vue` | Avatar tile with hover mask + upload button; `v-model:value` is the image URL, opens `CopperModal` to edit. Exposes `openModal/closeModal`. |
| `typing.ts` | Cropper option/event types. |

## For AI Agents

### Working in this directory
- Default `cropperjs` options are declared at module scope — extend via the `options` prop, do not mutate `defaultOptions`.
- `Cropper` instance is destroyed `onUnmounted`; preserve this to avoid canvas leaks.
- LESS uses `useDesign('cropper-image')` and `useDesign('cropper-avatar')` namespaces; new selectors must use `@{prefix-cls}__*`.

### Common patterns
- Cropped output is always rendered to a `<canvas>` then read via `toBlob` → `FileReader.readAsDataURL`.

## Dependencies
### Internal
- `/@/components/Modal` (useModal), `/@/components/Icon`, `/@/components/Button` (ButtonProps), `/@/hooks/web/useDesign`, `/@/hooks/web/useMessage`, `/@/hooks/web/useI18n`.
### External
- `cropperjs`, `@vueuse/shared` (useDebounceFn), `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
