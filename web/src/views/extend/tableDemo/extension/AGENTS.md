<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# extension

## Purpose
演示「行展开 + 嵌套子表」的 `BasicTable` 用法：父行通过 `expandedRowRender` 渲染另一个 `BasicTable` 显示子记录，按需懒加载子数据（`record.childTableLoading`、`onExpand`）。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 父表 `getIndustryList` + 子表 `getCityList`，行展开懒加载。 |

## For AI Agents

### Working in this directory
- 子表通过 `:data-source="record.list"` 接收懒加载结果；不要把分页加到子表上（父子共用 `pagination: false`）。
- 子表加载状态使用 `v-loading="record.childTableLoading"`，由父行 `onExpand` 在请求前后切换。

### Common patterns
- 父子双 `useTable` 注册器
- `onExpand(expanded, record)` 触发懒加载

## Dependencies
### Internal
- `/@/components/Table`, `/@/api/extend/table`, `/@/store/modules/base`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
