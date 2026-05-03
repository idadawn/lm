<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# subdev

## Purpose
二次开发 (secondary / customer-side development) module group. Reserved as the home for site-specific extensions to the Laboratory Data Analysis System — features built on top of the base modules without forking them. Currently scaffolded with the standard three-project split (impl + Entitys + Interfaces) but no source files yet — only `.csproj` placeholders.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.SubDev/` | Implementation project; depends on `Poxiao.Common.CodeGen` and `Poxiao.SubDev.Interfaces` (see `Poxiao.SubDev/AGENTS.md`). |
| `Poxiao.SubDev.Entitys/` | Entity + DTO project; depends only on `Poxiao.Infrastructure` (see `Poxiao.SubDev.Entitys/AGENTS.md`). |
| `Poxiao.SubDev.Interfaces/` | Service contracts; depends on `Poxiao.SubDev.Entitys` (see `Poxiao.SubDev.Interfaces/AGENTS.md`). |

## For AI Agents

### Working in this directory
- This module is intentionally **empty** today — the three projects exist as forward-declared targets so customer-specific features can be slotted in without touching the modular monolith's existing modules.
- When adding code, follow the *standard three-project layout* (matches `system`, `lab`, `workflow`, `app` modules): entities + DTOs in `*.Entitys`, contracts in `*.Interfaces`, services + dynamic-API controllers in `*` (impl).
- Entities here must follow `.cursorrules` — inherit `CLDEntityBase` and apply `[SugarColumn(ColumnName = "...")]` overrides for legacy field-name conventions.
- Do **not** add cross-module references upward (e.g., from `system` into `subdev`) — `subdev` is a leaf consumer, not a base.

## Dependencies
### Internal
- `Poxiao.Common.CodeGen` — leveraged by the impl project for code-generation helpers.
- `Poxiao.Infrastructure` — entity base classes and shared types.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
