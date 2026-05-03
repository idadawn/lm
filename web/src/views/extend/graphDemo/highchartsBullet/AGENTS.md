<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# highchartsBullet

## Purpose
Highcharts 子弹图（Bullet）演示页面，使用 `highcharts/modules/bullet` 注册子弹图类型，并行渲染三个紧凑子弹图。属于 `extend/graphDemo` 第三方组件示例。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，使用 `inverted: true` 横向布局，三组 `options1/options2/options3` 演示不同 KPI；含免责声明 alert。 |

## For AI Agents

### Working in this directory
- 必须先 `highchartsBullet(Highcharts)` 注册模块。
- 三个 reactive options 共用同一组样式，调整时记得同步保持视觉一致性。
- `chart.type: 'bullet'` + `plotOptions.series.pointPadding/borderWidth/color/targetOptions` 是关键配置。

### Common patterns
- `inverted: true` 让条形横向；高度通过 `class="h-130px"/h-80px` 紧凑显示。

## Dependencies
### Internal
- `page-content-wrapper` 全局样式
### External
- `highcharts`、`highcharts-vue`、`highcharts/modules/bullet`
