<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`createModel` 页面专用展示子组件：图表渲染容器（chartsModel）和归因分析（AttributionAnalysis）。`dimension/components/` 与本目录代码完全相同（同步副本），共同复用 `views/basic/home/components/optimalManagement` 下的图表配置 props。

## Key Files
| File | Description |
|------|-------------|
| `chartsModel.vue` | 接收 `chartsFlag/chartsData/gtData/ltData/random`，按 flag 渲染表格或多组 ChartsManageMent 图表。 |
| `AttributionAnalysis.vue` | 时间范围 + 分析维度多选 + 触发 `getIndexChartsDataList`，渲染 chartsModel。 |

## For AI Agents

### Working in this directory
- 与 `views/kpi/dimension/components/` 是镜像副本，修改时**两边都要同步**或抽到共享组件。
- `chartsModel` 内部直接引用 `views/basic/home/components/optimalManagement/index2.vue` 与 `optimalData/*.ts` 的 props，重构 home 模块时务必兼容。
- `chartsFlag === 'gudge'` 走 BasicTable 分支；其他 flag 走多卡片 ChartsManageMent 分支。

### Common patterns
- `:basicProps="item" :random="random"` 把数据 + 强制刷新 key 透传给图表组件
- props 集中在 `optimalManagement/optimalData/*.ts`

## Dependencies
### Internal
- `/@/views/basic/home/components/optimalManagement`, `/@/components/Table`, `/@/api/basic/charts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
