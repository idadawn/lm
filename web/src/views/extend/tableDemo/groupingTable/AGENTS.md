<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# groupingTable

## Purpose
演示「分组表格」：列表数据由后端返回 `getTableListAll` 平铺的父子记录（`record.isParent`），父行不显示操作列，子行显示常规 TableAction。常用于按客户/项目类型聚合的列表。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `v-if="!record.isParent"` 控制操作列；其余复用 commonForm。 |

## For AI Agents

### Working in this directory
- 父行通常不可编辑/删除，前端通过 `record.isParent` 判定；后端契约见 `/@/api/extend/table` 的 `getTableListAll`。
- `defineOptions({ name: 'extend-tableDemo-groupingTable' })`。

### Common patterns
- 单一接口返回平铺列表，前端按字段渲染分组样式
- 操作列条件渲染 `<TableAction v-if="!record.isParent" />`

## Dependencies
### Internal
- `/@/components/Table`, `../commonForm`, `/@/api/extend/table`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
