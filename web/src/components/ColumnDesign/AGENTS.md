<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ColumnDesign

## Purpose
JNPF low-code 列表设计器 (list/column designer). Lets a user pick which form fields appear as table columns, define filter rules, summary fields, super-query options, child-table styles, and pagination. Output is consumed by the runtime list renderer.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `BasicColumnDesign` (no `withInstall` wrapper here — direct named export). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Top-level designer component + sub-panels + helper config (see `src/AGENTS.md`). |
| `style/` | LESS styles for the designer layout (see `style/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Designer state mirrors `defaultColumnData` from `src/helper/config.ts` — keep new fields backward compatible.
- This package is tightly coupled to `/@/components/FormGenerator` (shared key lists like `useInputList`, `useDateList`).

### Common patterns
- jnpfKey-driven dispatch (search type 1=equal, 2=fuzzy, 3=range).

## Dependencies
### Internal
- `/@/components/FormGenerator/src/helper/config`.
### External
- `vue@3.3`, `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
