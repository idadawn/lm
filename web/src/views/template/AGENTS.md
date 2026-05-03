<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# template

## Purpose
Page-template scaffolds — boilerplate Vue pages used as starting points when generating new feature views (left/center page-content layout, demo of `ZEditor`, etc.).

## Key Files
| File | Description |
|------|-------------|
| `base.vue` | Minimal three-zone (`page-content-wrapper-{left,center,search-box}`) skeleton, useful as a copy-paste starter. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `baseDemo/` | Demo of dashboard-edit page wiring with `usePopup` (see `baseDemo/AGENTS.md`). |
| `componentDemo/` | Demo registering a custom `ZEditor` component (see `componentDemo/AGENTS.md`). |

## For AI Agents

### Working in this directory
- These pages are **examples**, not feature pages. When scaffolding new views, copy out of this directory rather than importing from it.
- Do not wire `template/*` into the production sidebar/router.
