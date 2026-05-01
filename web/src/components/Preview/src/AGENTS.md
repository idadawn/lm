<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation files for the image preview package. Contains the declarative gallery wrapper, the heavyweight functional/modal viewer used by `createImgPreview`, and shared TypeScript types.

## Key Files
| File | Description |
|------|-------------|
| `Preview.vue` | `ImagePreview` — wraps `Image.PreviewGroup`; renders thumbnails from `imageList` (string or `ImageProps`) with optional `placeholder` slot. |
| `Functional.vue` | Standalone fullscreen previewer with scale/rotate/pan, keyboard nav, prev/next, used by imperative API. |
| `functional.ts` | `createImgPreview(options)` — mounts `Functional.vue` to `document.body` via `createVNode + render`. |
| `typing.ts` | `Options`, `Props`, `PreviewActions` (resume/close/prev/next/setScale/setRotate), `ImageProps`, `ImageItem`. |

## For AI Agents

### Working in this directory
- `PreviewActions` is the exposed instance API for imperative use — keep names stable (downstream callers rely on them).
- `Options.imageList` is `string[]` for the imperative path; declarative path accepts richer `ImageItem`.
- Default `scaleStep` is 20; respect existing default when adjusting options merging.

### Common patterns
- LESS prefixed via `useDesign('image-preview')`; override styling on `.ant-image-preview-operations`.

## Dependencies
### Internal
- `/@/hooks/web/useDesign`, `/@/utils/is`, `/@/utils/propTypes`.
### External
- `ant-design-vue` (`Image`, `Image.PreviewGroup`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
