<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# tableTree

## Purpose
演示「树形数据表格」：`BasicTable` 通过 `isTreeTable: true` + `expandAll()` 渲染字典 `IndustryType` 的层级数据，无搜索栏、无分页，是字典/分类数据的最简陈列形式。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `useBaseStore().getDictionaryData('IndustryType')` 取数；`nextTick(expandAll)` 一次性展开。 |

## For AI Agents

### Working in this directory
- 数据来源为前端缓存的字典 store，不走 `api/extend/table`。
- 列简化为「名称 / 编码」，新增字段时确认字典原始结构是否包含。

### Common patterns
- `useTable({ isTreeTable: true, immediate: false, pagination: false })`

## Dependencies
### Internal
- `/@/components/Table`, `/@/store/modules/base`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
