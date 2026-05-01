<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DepSelect

## Purpose
Multi-select dropdown for laboratory metric dimensions (`field`/`fieldName` schema). Adds an automatic "全部" option that, when picked, expands to all rows. Used by chart configuration, target/metric pickers in 检测室分析模块.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Default-exports `./src/index.vue`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | The Ant Design Vue select wrapper (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Field schema is fixed: items must expose `field` (id) and `fieldName` (label). For id/name shaped data use the sibling `DepSelectTag` component instead.
- The "all" sentinel uses `field: 'all'`, `dataType: 'all'` — preserve when changing the selection logic.

### Common patterns
- Emits `depSelectEmits` (id array) and `depSelectItemEmits` (raw row array).

## Dependencies
### External
- `ant-design-vue` (a-select), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
