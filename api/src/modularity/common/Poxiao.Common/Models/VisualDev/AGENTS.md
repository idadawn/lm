<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# VisualDev

## Purpose
Runtime models for the 可视化开发 (low-code) module's main-belt/sub-table query feature. Used when a visual-dev page composes a master grid against a primary table joined to optional sub-table search controls.

## Key Files
| File | Description |
|------|-------------|
| `MainBeltViceQueryModel.cs` | Master-with-sub query — `searchList[]` of `ListSearchParametersModel`、`sort`、`defaultSidx`. Also defines `ListSearchParametersModel` with `poxiaoKey` (control key)、`format` (date format)、`multiple`、`searchType`、`vModel` (control variable name). |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Models.VisualDev`.
- `poxiaoKey` is the unique stable id of the visual control; do not rename — it is part of the saved page-design JSON and breaking it invalidates existing dashboards.
- `searchType` is an int-coded operator that maps to `Enums/QueryType` — keep the mapping in sync if either side changes.
- `[SuppressSniffer]` required.

### Common patterns
- camelCase props.
- Two related models (master + item) live in the same file rather than splitting.

## Dependencies
### Internal
- Consumed by the VisualDev module services and code-gen runtime queries.
- Implicitly aligned with `Enums/QueryType.cs`.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
