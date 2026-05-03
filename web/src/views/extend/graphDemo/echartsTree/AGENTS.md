<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsTree

## Purpose
ECharts 树图（Tree）演示页面，展示植物界分类（门 / 纲 / 目）的多层级树结构。是 `extend/graphDemo` 中数据量最大的示例（~34KB）。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，`reactive` 中嵌套大型 `data` 树（蓝藻门 / 裸藻门 / 甲藻门 …），通过 `/@/components/Chart` 渲染 `series.type: 'tree'`；命名 `extend-graphDemo-echartsTree`。 |

## For AI Agents

### Working in this directory
- 树形数据体积较大，避免在 hot path 中重新构造；如需修改建议拆出 JSON。
- 调整 `layout: 'orthogonal' | 'radial'`、`orient: 'LR'/'TB'` 切换布局方向。
- 不要为缩减体积移除中文分类，那是演示内容的核心。

### Common patterns
- `series[0].data: [data1]` 单根节点；`itemStyle`、`label.position: 'left|right'`、`emphasis.focus: 'descendant'`。

## Dependencies
### Internal
- `/@/components/Chart`
### External
- `echarts`、`vue` reactive
