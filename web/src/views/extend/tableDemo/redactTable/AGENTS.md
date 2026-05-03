<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# redactTable

## Purpose
演示「行内编辑」：点击表格行后，单元格切换为可编辑控件（`a-input`、`jnpf-select`、`jnpf-date-picker`），失焦或切换到其它行时调用 `updateTableRow` 保存。适合需要快速批量修改字段的场景。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `state.tabClickId` 跟踪当前编辑行；`@row-click="handleRowClick"` 切换编辑态。 |

## For AI Agents

### Working in this directory
- 显示态/编辑态使用 `v-if="state.tabClickId == record.id"` 条件渲染；保持 dayjs 格式化显示态时间字段。
- `updateTableRow` 应做并发保护，避免快速切换行时旧请求覆盖新数据。

### Common patterns
- 字典字段编辑用 `jnpf-select`，日期用 `jnpf-date-picker`
- 显示态格式：`dayjs(value).format('YYYY-MM-DD HH:mm')`

## Dependencies
### Internal
- `/@/components/Table`, `../commonForm`, `/@/api/extend/table`
### External
- `dayjs`, `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
