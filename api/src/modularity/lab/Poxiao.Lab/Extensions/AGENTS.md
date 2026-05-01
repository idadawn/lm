<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
Service-layer extension methods on `ISqlSugarRepository<ProductSpecEntity>` for product-spec calculation properties. Most members here are marked `[Obsolete]` — the canonical store has migrated to the dedicated `LAB_PRODUCT_SPEC_ATTRIBUTE` table (`ProductSpecAttributeEntity` + `ProductSpecAttributeService.EnsureCoreAttributes`). Kept for backward compatibility with older callers.

## Key Files
| File | Description |
|------|-------------|
| `ProductSpecServiceExtensions.cs` | Repo extensions: `EnsureDefaultCalculationProperties`, `BatchUpdateCalculationPropertiesAsync`, `ValidateCalculationProperties` — all `[Obsolete]` no-ops kept for source compatibility. |

## For AI Agents

### Working in this directory
- Do **not** add new logic here — extend `ProductSpecAttributeService` (or `ProductSpecExtensions` in the Entity layer) instead.
- New product-spec attributes go into `LAB_PRODUCT_SPEC_ATTRIBUTE` rows (key/value/valueType), never into `PropertyJson`.
- If you must touch this file, preserve the `[Obsolete]` markers and the no-op bodies — call sites still link against the signatures.

### Common patterns
- All methods return `Task.CompletedTask` or do nothing — they exist solely so legacy `using Poxiao.Lab.Extensions;` files keep compiling.

## Dependencies
### Internal
- `Poxiao.Lab.Entity.ProductSpecEntity`.
- SqlSugar `ISqlSugarRepository<T>`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
