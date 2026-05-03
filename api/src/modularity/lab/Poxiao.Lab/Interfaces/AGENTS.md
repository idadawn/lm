<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Interfaces

## Purpose
Module-internal service contracts that don't (yet) belong in the public `Poxiao.Lab.Interfaces` assembly. Currently holds the colour-configuration service used by the IntermediateData grid UI to persist per-cell highlight colours.

## Key Files
| File | Description |
|------|-------------|
| `IIntermediateDataColorService.cs` | CRUD contract for `IntermediateDataColorEntity`: `SaveColors`, `GetColors`, `DeleteColors`, `GetColorsByDataIds(intermediateDataIds, productSpecId)`, `SaveCellColor(intermediateDataId, fieldName, colorValue, productSpecId)`. |

## For AI Agents

### Working in this directory
- New module-internal interfaces (only consumed inside `Poxiao.Lab`) belong here; cross-module/public contracts go into `Poxiao.Lab.Interfaces` instead.
- The implementing class is `Service/IntermediateDataColorService.cs`.
- Colour values are stored per-cell `(intermediateDataId, fieldName) → colorValue`; queries are batched via `GetColorsByDataIds` to avoid N+1.

### Common patterns
- DTOs (`SaveIntermediateDataColorInput`, `GetIntermediateDataColorInput`, `DeleteIntermediateDataColorInput`, `IntermediateDataColorDto`) live in `Poxiao.Lab.Entity.Dto.IntermediateData`.

## Dependencies
### Internal
- `Poxiao.Lab.Entity.Dto.IntermediateData`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
