<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# charts

## Purpose
`@ant-design/charts` 包装层。Canvas 图表不支持 SSR，全部走 `next/dynamic` + `{ ssr: false }`。`index.tsx` 同时暴露：(1) 各图组件、(2) `ChartRenderer` —— 按 `ChartDescriptor.type` 派发到具体图组件的调度器（聊天 SSE `chart` 事件用它）。

## Key Files

| File | Description |
|------|-------------|
| `index.tsx` | dynamic 导出 `TrendLine` / `MetricGauge` / `GradePie` / `HorizontalBar` / `ScatterPlot` / `RadarChart` + `ChartRenderer` + 内联 `Shimmer` 占位 |
| `TrendLine.tsx` | `@ant-design/charts` Line 包装：smooth + point + grade threshold annotations + tooltip 格式化 |
| `MetricGauge.tsx` | 仪表盘图（单值，含等级阈值环） |
| `GradePie.tsx` | 等级分布饼图（`angleField`/`colorField`） |
| `HorizontalBar.tsx` | 横向柱：用于"不合格 Top5"等排名 |
| `ScatterPlot.tsx` | 散点：用于厚度相关性 |
| `RadarChart.tsx` | 雷达：班次综合评分对比 |

## For AI Agents

### Working In This Directory
- 新图必须遵守模板：(1) `Xxx.tsx` 用 `dynamic(() => import("@ant-design/charts").then(mod => mod.<Type>), { ssr: false, loading: () => <Shimmer .../> })`，(2) `index.tsx` 再 `dynamic` 暴露同名导出。
- 接受 `descriptor: ChartDescriptor` 作为唯一 prop，从 `descriptor.data/xField/yField/meta/annotations` 读字段；不要拓展 prop 接口。
- `ChartRenderer` 是 `switch (chartSpec.type)`，新图的 type 加在 `@nlq-agent/shared-types` 的 `ChartType` union 后再来这里 case。

### Testing Requirements
- vitest 难测 Canvas，仅在 `index.tsx` 校验 `ChartRenderer` 的 fallback 分支（"暂不支持图表类型: xxx"）。
- 主验证靠 e2e（`tests/e2e/`）和手工 `pnpm --filter web dev`。

### Common Patterns
- Annotations 绑定 `descriptor.meta?.gradeThresholds` —— 红绿目标线/警戒线虚线。
- Tooltip 用 `formatter: (datum) => ({ name, value: \`${val.toFixed(3)} ${unit}\` })`，单位从 `descriptor.meta.unit`。
- 内联 `Shimmer` 是 `<div className="animate-pulse bg-gray-200 ...">`；不抽公共组件（轻量，避免循环依赖）。

## Dependencies

### Internal
- `@nlq-agent/shared-types`：`ChartDescriptor`、`ChartType`、`GradeThreshold`、`ChartDataPoint`、`ChartAnnotation`。

### External
- `@ant-design/charts@^2.2`、`next/dynamic`、`react@^19`。
