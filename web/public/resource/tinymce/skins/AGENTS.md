<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# skins

## Purpose
TinyMCE 5 UI skin assets (light + dark variants). Loaded via `skin_url` and `content_css` at editor init.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `ui/` | Holds the actual skin variants `oxide` (light) and `oxide-dark` (see `ui/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Standard TinyMCE skin layout: `skins/ui/<skin-name>/{skin.min.css,content.min.css,...}`. Do not flatten.
- Pair this with `../langs/` — both are needed for a fully self-hosted TinyMCE.
- Vendor content; replace on TinyMCE upgrades.

## Dependencies
### External
- TinyMCE 5.10 oxide skin distribution.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
