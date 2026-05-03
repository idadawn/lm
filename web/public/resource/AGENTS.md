<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# resource

## Purpose
Runtime asset bucket served at `/resource/...`. Contains pre-shipped binaries the SPA loads at runtime: emoji GIFs for chat/comment widgets, branding images, and a self-hosted TinyMCE 5 distribution (skins + language packs).

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `emoji/` | ~200 numbered `.gif` emoji sprites used by chat/IM components (see `emoji/AGENTS.md`). |
| `img/` | Branding images: `logo.png`, `logo-128.png`, `logo1.png`, `login-split-bg.png`, PWA icons (`pwa-192x192.png`, `pwa-512x512.png`) (see `img/AGENTS.md`). |
| `tinymce/` | Self-hosted TinyMCE 5 skins and language packs (see `tinymce/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Asset-only directory — do not put JS/TS sources here.
- All files are referenced by absolute path `/resource/...` from runtime code (TinyMCE init, emoji picker, login screens).
- A near-duplicate exists at `../../resource/` (legacy mirror) — when updating an asset consider whether the legacy copy also needs touching, but new assets should land here only.
- PWA icon names (`pwa-192x192.png`, `pwa-512x512.png`) are conventional for `vite-plugin-pwa`; keep names if PWA manifest is enabled.

## Dependencies
### Internal
- Referenced by TinyMCE wrapper components in `src/components/Tinymce/` and chat/IM emoji pickers.
### External
- TinyMCE 5.10.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
