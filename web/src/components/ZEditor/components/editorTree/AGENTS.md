<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# editorTree

## Purpose
ZEditor 左侧节点拖拽树。基于 `a-tree`(`show-line`)展示指标分类层级,叶节点(`level == 2`)可拖拽到 MindMap 画布触发 `dragendNode` 事件以新增价值链节点。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 自定义 #title slot,按 level 区分叶节点 draggable 行为 |
| `props.ts` | `source` 树形数据 prop 声明 |

## For AI Agents

### Working in this directory
- 拖拽通过 `dragend` 事件(而非 dragstart)向父发射 `dragendNode`,与 ZChartEditor 的 dragstart + dataTransfer 模式不同。
- `expandedKeys` 默认展开 `0-0-1`,如更换数据结构需同步更新或改为 `default-expand-all`。
- 仅支持 2 级展示;若需要 N 级拖拽,需扩展 level 判定。

### Common patterns
- `defineOptions({ name: 'ZEditorTree' })` 与 ZChartEditor 同名,通过 import 路径区分。

## Dependencies
### External
- Ant Design Vue (`a-tree`,`TreeProps['fieldNames']`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
