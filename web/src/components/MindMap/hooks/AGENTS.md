<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
MindMap 组件的 G6 渲染核心。包含画布初始化、自定义节点 (`node-with-line`)、工具栏 SVG、节点更新逻辑以及数据类型定义。

## Key Files
| File | Description |
|------|-------------|
| `useMindMap.ts` | 注册自定义节点（标题盒、指标值、metricGrade 标签、底部进度条、趋势 mini chart），绑定 hover/click/collapse/add/delete 事件，导出 `useMindMap`、`useMindMapResult`、`useMindMapCallback`。 |
| `CustomToolbar.tsx` | G6 ToolBar 内嵌的 SVG 按钮（zoomOut/zoomIn/realZoom/autoZoom/fullScreen）。 |
| `MindMapSourceType.ts` | `Node` / `Edge` / `MetricGrade` / `TrendData` / `SourceInterface` 类型；树形数据结构定义。 |
| `mockdata.json` | 本地开发用的指标树 mock 数据。 |
| `useMindMapMore.ts` | `indexMore.vue` 使用的多视图变体逻辑（保留备用）。 |

## For AI Agents

### Working in this directory
- `graph` 是模块级 reactive 单例，多个 MindMap 实例同时挂载会互相覆盖；新增组件实例化前先评估是否需要拆分为局部状态。
- 节点尺寸 `nodeConfig.size = [300, 120]`、`indent = size[0] * 1.5` 是布局基准，调整需同步 layout `getHeight`。
- 工具栏使用 `render(h(CustomToolbar), content)` 注入到 G6 ToolBar 容器，避免直接操作 DOM 字符串。

### Common patterns
- 自定义 marker（`add-item` / `remove-item` / `collapse-item`）通过 `target.get('name')` 区分点击事件。
- `metricGrade` 数组遍历时用 `is_show` 过滤，并按字符长度估算横向布局。

## Dependencies
### Internal
- 模块内 `MindMapSourceType`
### External
- `@antv/g6`、`@antv/chart-node-g6`、`@ant-design/icons-vue`、`lodash-es`、`ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
