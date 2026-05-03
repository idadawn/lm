<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Preview

## Purpose
Image preview component package. Wraps `ant-design-vue`'s `Image.PreviewGroup` for declarative gallery preview, and ships an imperative `createImgPreview` helper that mounts a fullscreen viewer with rotation/scale controls.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Public exports — `ImagePreview` component and `createImgPreview` functional API. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Implementation: `Preview.vue`, `Functional.vue`, `functional.ts`, `typing.ts` (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Two usage modes: declarative `<ImagePreview :imageList="..." />` for inline thumbnails, and imperative `createImgPreview({ imageList, index })` for modal preview.
- Imperative API mounts via `createVNode + render` directly to `document.body`; SSR-guarded by `isClient`.

### Common patterns
- `imageList` items may be `string` URLs or `ImageProps` objects (src/width/placeholder/preview).

## Dependencies
### Internal
- `/@/utils/is` (`isClient`, `isString`), `/@/hooks/web/useDesign`, `/@/utils/propTypes`.
### External
- `ant-design-vue` (`Image`, `Image.PreviewGroup`), `vue` (`createVNode`, `render`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
