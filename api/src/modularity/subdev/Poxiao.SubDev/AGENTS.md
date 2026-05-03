<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.SubDev

## Purpose
Implementation project for the 二次开发 (SubDev) module — services and dynamic-API controllers for customer-specific extensions. Currently a scaffold: only `Poxiao.SubDev.csproj` exists, no source files. Wired to consume `Poxiao.Common.CodeGen` (so generated SubDev features can reuse shared code-gen helpers) and the SubDev contracts in `Poxiao.SubDev.Interfaces`.

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.SubDev.csproj` | Empty project; references `Poxiao.Common.CodeGen` and `Poxiao.SubDev.Interfaces`; pulls StyleCop analyzers. |

## For AI Agents

### Working in this directory
- Empty implementation today — when adding features, place service classes (`IDynamicApiController, ITransient`) directly in this project root or under feature-named subfolders (matches conventions in `Poxiao.Systems`, `Poxiao.Lab`, etc.).
- Resolve all entity/DTO types from `Poxiao.SubDev.Entitys` (transitively via `Poxiao.SubDev.Interfaces`) — do not declare entities in this project.
- Keep code-generated artifacts here so `Poxiao.Common.CodeGen` helpers are in scope (it's the only customer-flavored module wired this way).
- This project is **not yet** referenced by `Poxiao.API.Entry`; when first content is added, also wire it into the application entry's project references.

## Dependencies
### Internal
- `Poxiao.Common.CodeGen` — code-generation helper utilities.
- `Poxiao.SubDev.Interfaces` — service contracts (which in turn pulls `Poxiao.SubDev.Entitys`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
