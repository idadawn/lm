<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# editorDashboard

## Purpose
ZChartEditor 的中央画布。承载 `chartItems`(图表/筛选器节点),通过自研 `DragResize` 实现拖拽缩放,并把每个节点投递给 `Chart.vue`(ECharts)或 `SelectFilter.vue`(过滤器)。支持网格吸附、画布缩放、删除、ToolBar 标题、X/Y 轴标题等。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 画布根组件,处理 drop/click/active 事件,挂载 ECharts/Filter 节点 |
| `Chart.vue` | 单图表渲染,基于 `useECharts` 监听 options 变化 |
| `DragResize.vue` | 通用拖拽缩放容器(替代 vue-grid-layout) |
| `SelectFilter.vue` | 维度过滤选择器节点 |
| `DateFilter.vue` | 日期区间过滤节点 |
| `ToolBar.vue` | 节点顶部标题工具栏 |
| `props.ts` | `chartItems / scale / editType / snapToGrid` 等 props |

## For AI Agents

### Working in this directory
- 节点类型由 `state.chartTypes` / `state.selectTypes` 数组控制;扩展节点类型须同步更新这两个集合并提供对应渲染分支。
- `drop` 事件区分 `outer`(画布空白)与 `inChart`(投放到已有节点),逻辑都在 `useEditor.addNode` 中。
- `parentScaleX/Y` 必须传 `scale.x`(同值),保持 DragResize 在缩放后坐标正确。

### Common patterns
- 通过 `defineExpose({ addNode })` 让父组件透传拖入事件。

## Dependencies
### Internal
- `../../hooks/useEditor`、`/@/store/modules/chart`、`/@/hooks/web/useECharts`
### External
- `@ant-design/icons-vue` (CloseOutlined),`ant-design-vue` (Spin)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
