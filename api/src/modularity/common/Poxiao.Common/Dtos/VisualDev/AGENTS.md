<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# VisualDev

## Purpose
DTOs powering the 可视化开发 (low-code visual development) data-binding controls — used when a visual form/dialog binds to a data source for paged search and selection write-back.

## Key Files
| File | Description |
|------|-------------|
| `VisualDevDataFieldDataListInput.cs` | Paged search input (extends `PageInputBase`) — `relationField`、`id`、`propsValue`、`columnOptions`、`ids[]`、`paramList[]` of `DataInterfaceReqParameterInfo`. |
| `VisualDevModelDataCrInput.cs` | Create/edit input for visual-dev-bound model data. |
| `VisualDevModelDataUpInput.cs` | Update input. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Dtos.VisualDev`.
- Inherits `PageInputBase` from sibling `Filter/` — preserves the standard `currentPage`/`pageSize`/`sidx`/`sort`/`queryJson` contract.
- `paramList` reuses `DataInterfaceReqParameterInfo` from parent `Dtos/` — do not duplicate.
- `columnOptions` is a CSV string (legacy frontend contract); leave it as `string`, do not convert to `List<string>`.
- `[SuppressSniffer]` required.

### Common patterns
- Inputs split by lifecycle role (`Cr`/`Up`/`DataList`).
- Default-initialized collections (`ids = new List<string>()`, `paramList = new List<...>()`).

## Dependencies
### Internal
- `Poxiao.Infrastructure.Filter.PageInputBase`.
- `Poxiao.Infrastructure.Dtos.DataInterfaceReqParameterInfo`.
- Consumed by the VisualDev module controllers/services.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
