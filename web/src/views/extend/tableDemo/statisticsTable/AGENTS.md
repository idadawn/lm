<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# statisticsTable

## Purpose
演示「表尾合计行」：使用 ant-design-vue 的 `<a-table-summary>` 在底部固定合计行，对费用类列做求和。其他流程沿用 commonForm 的 CRUD 模板。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `getColumnSum()` 计算每列合计；`#summary` 插槽渲染固定合计行。 |

## For AI Agents

### Working in this directory
- 合计仅汇总当前页数据；如需全量合计应在后端接口返回汇总并改为单独显示。
- `<a-table-summary fixed>` 需在 BasicTable 启用 `scroll` 时才生效。

### Common patterns
- `unref` 取响应式 list；`reduce` 求和
- 固定合计 `<a-table-summary fixed>`

## Dependencies
### Internal
- `/@/components/Table`, `../commonForm`, `/@/api/extend/table`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
