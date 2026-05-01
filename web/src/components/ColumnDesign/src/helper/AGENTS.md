<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# helper

## Purpose
Configuration constants and pure helper functions for the JNPF 列表设计器. Defines default `columnData`, default function-body templates, jnpfKey-based search-type rules, multiple-select whitelist, and the no-upload key blacklist.

## Key Files
| File | Description |
|------|-------------|
| `config.ts` | `getSearchType(item)` (1=equal/2=fuzzy/3=range), `getSearchMultiple(jnpfKey)`, `defaultBtnFunc`, `defaultFuncsData` (afterOnload/rowStyle/cellStyle), and `defaultColumnData` (ruleList, searchList, hasSuperQuery, childTableStyle, showSummary, summaryField, columnList, type, defaultSidx, sort, hasPage, pageSize, ...). |

## For AI Agents

### Working in this directory
- Treat this as the schema source of truth — when adding a new designer feature, default it here first.
- jnpfKey lists (`useInputList`, `useDateList`, `noVModelList`) are imported from `FormGenerator/src/helper/config`; do not duplicate them.

### Common patterns
- Pure functions + plain object literals; no Vue dependencies.

## Dependencies
### Internal
- `/@/components/FormGenerator/src/helper/config`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
