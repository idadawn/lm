<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# warning

## Purpose
预警 (warning/alert) feature area — pages for configuring warning rules and viewing aggregated warning overviews. Supports the laboratory predictive-warning pipeline.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `maintenance/` | 预警规则 maintenance page (see `maintenance/AGENTS.md`). |
| `overview/` | 预警总览 dashboard (see `overview/AGENTS.md`). |

## For AI Agents

### Working in this directory
- These pages connect to the `extend/table` API (`/@/api/extend/table`) — note this **isn't** the dedicated warning API; the team has been reusing the generic table CRUD endpoints.
- When adding a new warning subview, mirror the `maintenance/`/`overview/` split.
