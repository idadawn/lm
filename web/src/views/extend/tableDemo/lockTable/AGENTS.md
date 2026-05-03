<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# lockTable

## Purpose
演示「列冻结」：通过列定义里的 `fixed: 'left'` 和 `fixed: 'right'` 让首两列项目名称/编码与操作列在水平滚动时常驻可见。其他逻辑与 commonTable 完全一致。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列定义首两列 `fixed: 'left'`，操作列固定右侧。 |

## For AI Agents

### Working in this directory
- 启用 `fixed` 时必须为列指定 `width`，否则 ant-design-vue 会渲染异常。
- 表头/表体行高需保持一致，避免冻结列与滚动列错位。

### Common patterns
- `{ title: '...', dataIndex: '...', width: 200, fixed: 'left' }`

## Dependencies
### Internal
- `/@/components/Table`, `../commonForm`, `/@/api/extend/table`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
