<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ZChartEditor

## Purpose
自研图表看板编辑器组件包。提供"指标/维度"拖入画布 -> 自由拖拽布局 -> 实时预览 ECharts 图表的能力,服务于检测室数据分析的自定义图表大盘场景。通过 `withInstall(ZChartEditor)` 暴露顶层组件。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 入口,`withInstall` 包装 `src/index.vue` 并导出 `ZChartEditorProps` 类型 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 主组件容器 (左 tree / 中 dashboard / 右 elements 三栏布局) (see `src/AGENTS.md`) |
| `components/` | 编辑器子组件:tree、dashboard、elements (see `components/AGENTS.md`) |
| `hooks/` | `useEditor` 封装节点增删与画布交互 (see `hooks/AGENTS.md`) |
| `plugins/` | G6 画布插件:背景与网格 (see `plugins/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 区分 `params.type === 'edit'` 与预览态:预览态隐藏左右两侧面板。
- 缩放比例与布局通过 `chartStore`(`/@/store/modules/chart`)集中管理,不要在子组件里维护本地 scale 副本。
- 与同名的 `ZEditor` 区分:本组件聚焦图表大盘,`ZEditor` 用于指标价值链思维导图。

### Common patterns
- 三栏 layout + g6/echarts 双引擎;指标维度通过 `getMetricsDimensions` API 取得。

## Dependencies
### Internal
- `/@/store/modules/chart`、`/@/api/chart`
- `/@/utils` (`withInstall`)
### External
- Vue 3 + TS,`@antv/g6`,`echarts`(经 `useECharts`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
