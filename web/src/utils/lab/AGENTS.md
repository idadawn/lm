<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# lab

## Purpose
Laboratory-domain utilities — currently a unit-conversion cache + lookup layer fronting `/@/api/lab/unit`. Provides reusable lookup of `UnitDefinition` records (id, category, symbol, scaleToBase, offset, precision) with module-level caching to avoid re-fetching on every form render.

## Key Files
| File | Description |
|------|-------------|
| `unit.ts` | `UnitDefinition` interface, `loadAllUnits()` cache primer, plus helpers that group/select units by category — used by metric / intermediate-data / formula forms. |

## For AI Agents

### Working in this directory
- Cache is module-level (`unitCache`, `allUnitsCache`). Reset on tenant/user change if necessary by re-importing or adding a clear function.
- `scaleToBase` + `offset` form a linear transform — for non-linear units (e.g. log-scale) extend the type instead of overloading.
- Stays generic: this folder should not host metric- or formula-specific logic — those belong under `views/lab/*` or `api/lab/`.

### Common patterns
- All exports are async functions returning `UnitDefinition[]` / `UnitDefinition | undefined`.

## Dependencies
### Internal
- `/@/api/lab/unit`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
