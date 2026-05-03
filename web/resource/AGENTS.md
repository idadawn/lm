<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# resource

## Purpose
**Legacy** asset mirror, structurally identical to `public/resource/` (emoji/, img/, tinymce/). Predates the move to Vite's `publicDir` convention. Kept in the tree because some build/deploy paths still copy from here, but new assets should land in `public/resource/` instead.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `emoji/` | Numbered emoji GIFs (mirror of `public/resource/emoji/`). |
| `img/` | Branding images (mirror of `public/resource/img/`). |
| `tinymce/` | TinyMCE 5 skins + langs (mirror of `public/resource/tinymce/`). |

## For AI Agents

### Working in this directory
- **Do not add new assets here** — put them in `public/resource/` so Vite ships them at `/resource/...`. This dir is preserved for backward compatibility only.
- If updating an existing asset (e.g., logo, emoji), update both this dir and `public/resource/` to keep references resolving in both legacy and current paths.
- Files are not picked up by Vite's `publicDir` (which points to `public/`); content here only matters for downstream scripts that copy `web/resource` into the dist (check `build/script/postBuild.ts`).

## Dependencies
### Internal
- Mirror of `../public/resource/`. Possibly referenced by `../build/script/postBuild.ts` or legacy imports under `../src/`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
