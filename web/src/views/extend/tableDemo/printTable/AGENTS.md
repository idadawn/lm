<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# printTable

## Purpose
演示「表格打印」：使用 `print-js` 打印 `BasicTable` 的当前数据集，搭配「打印」按钮。常用于检测报告/项目清单的纸质输出场景。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 通过 `printJS` 输出表格 DOM；列定义精简为打印友好字段。 |

## For AI Agents

### Working in this directory
- 打印前应保证当前已加载所有需要的数据（关闭分页或一次拉全）。
- 打印样式由 `print-js` 默认接管；需自定义样式时另行注入 CSS 字符串。

### Common patterns
- `import printJS from 'print-js'` + DOM 选择器调用

## Dependencies
### Internal
- `/@/components/Table`, `/@/api/extend/table`
### External
- `print-js`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
