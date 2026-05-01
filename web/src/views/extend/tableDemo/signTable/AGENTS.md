<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# signTable

## Purpose
演示「彩色项目标记」：每行末尾用一组圆点（红/橙/黄/绿/蓝/紫/灰）表示标记状态，点击操作列下拉可设置/取消标记。带前端分页（`handleTableChange` 接管 `pagination`），并演示自定义 `#toolbar` 重载按钮。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `signArray` 字段渲染 dot 列表；`updateSign` 提交标记；前端维护 `list/listQuery/total`。 |

## For AI Agents

### Working in this directory
- 颜色码硬编码（`#ff625c`、`#f9a646` 等）与 `options.value`（'1'-'7'）一一对应，新增颜色需同步两处。
- 该示例自管分页（`getPagination` 计算属性），与默认 `useTable` 的服务端分页路径不同。

### Common patterns
- `#bodyCell` 插槽 + `<a-dropdown>` 设置标记
- `state.options` 数组定义颜色/名称/value

## Dependencies
### Internal
- `/@/components/Table`, `/@/components/Form`, `../commonForm`, `/@/api/extend/table`
### External
- `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
