<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# componentDemo

## Purpose
Demo of authoring a small reusable component (`ZEditorForm`) and exporting it via `withInstall` — illustrates the public-API shape used by `/@/components/*`.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `ZEditor = withInstall(Component)` and re-exports `ZEditorFormProps`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Component implementation and props (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Pattern: `index.ts` only re-exports; implementation lives in `src/` — keep that split when copying for new components.
- Use `ExtractPropTypes` to derive the public props type from `props.ts`.
