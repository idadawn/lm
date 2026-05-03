<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# mergeTable

## Purpose
演示「合并单元格」：通过列定义的 `customCell(record, rowIndex, column)` 返回 `{ rowSpan, colSpan }`，把同一项目阶段/负责人的连续行合并成一格。常用于报表汇总展示。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `customCellFun` 计算合并跨度，作用在 `projectPhase`、`principal` 等列。 |

## For AI Agents

### Working in this directory
- 合并依赖排序后的列表，前端在 `afterFetch` 或 `customCellFun` 内做相同字段的连续段判定。
- 与 `useSearchForm` 同用时，搜索过滤后需重新计算合并范围。

### Common patterns
- `customCell` 返回 `{ rowSpan: n, colSpan: 0 }` 实现纵向合并

## Dependencies
### Internal
- `/@/components/Table`, `../commonForm`, `/@/api/extend/table`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
