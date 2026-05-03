<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Basic

## Purpose
Tiny presentational primitives reused across the admin UI: animated arrows, captions, titles with optional help, and a tooltip-driven help icon. These are the lowest-level building blocks consumed by larger components like `BasicTable`, forms, and panels.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Barrel exporting `BasicArrow`, `BasicTitle`, `BasicCaption`, `BasicHelp` (all `withInstall`). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `.vue` implementations (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- These are intentionally minimal — keep them slot-friendly and prop-light.
- Visual prefix uses `useDesign` so they inherit the global namespace.

### Common patterns
- `withInstall` barrel.

## Dependencies
### Internal
- `/@/utils` (`withInstall`), `/@/hooks/web/useDesign`.
### External
- `ant-design-vue` (`Tooltip`), `@ant-design/icons-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
