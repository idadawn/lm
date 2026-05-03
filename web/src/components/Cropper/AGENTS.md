<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Cropper

## Purpose
Image cropping based on `cropperjs`. Provides `CropperImage` (raw cropper canvas) and `CropperAvatar` (modal-driven avatar editor with upload). Used by user profile, organization avatars, and any place needing cropped image upload.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Barrel: `CropperImage` and `CropperAvatar` via `withInstall`; re-exports `src/typing`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Cropper SFCs, modal wrapper, types (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `CropperAvatar` requires `uploadApi: ({ file: Blob, name: string }) => Promise<void>` — never bypass it; raw uploads must use the project's upload helper.
- Circle mode draws a rounded canvas with `globalCompositeOperation = 'destination-in'`; preserve when extending.

### Common patterns
- Emits payloads include `imgBase64` (FileReader DataURL) and `imgInfo` from `cropper.getData()`.

## Dependencies
### Internal
- `/@/utils` (withInstall), `/@/components/Modal`, `/@/components/Icon`, `/@/hooks/web/useDesign`, `/@/hooks/web/useMessage`, `/@/hooks/web/useI18n`.
### External
- `cropperjs` + `cropperjs/dist/cropper.css`, `@vueuse/shared`, `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
