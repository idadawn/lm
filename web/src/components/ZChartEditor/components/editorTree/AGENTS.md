<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# editorTree

## Purpose
ZChartEditor 左侧指标/维度面板。基于 a-tabs(指标/维度)展示扁平 `props.source[activeKey].children`,并允许用户拖拽列表项到中央 dashboard 触发节点新增。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 二级 Tab + 可拖拽列表;`dragstart` 写入 `dataTransfer('chartData')` |
| `props.ts` | `source` 数据源 prop 声明 |

## For AI Agents

### Working in this directory
- 拖拽数据通过 `dataTransfer.setData('chartData', JSON.stringify(item))` 传递;dashboard 端读取该 key,不要换名。
- 仅渲染 children;若需要分组/三级,需扩展 props.source 结构并保持向后兼容。

### Common patterns
- 命名 `defineOptions({ name: 'ZEditorTree' })`,与 ZEditor/editorTree 同名,通过路径区分。

## Dependencies
### External
- Ant Design Vue (`a-tabs`)、原生 HTML5 拖拽 API

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
