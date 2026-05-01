<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# prediction

## Purpose
预测 (Prediction) 模块的视图根目录。当前仅承载 `overview/` 总览页，作为预测分析功能的入口（与 `predictionManagement/` 形成"展示 vs 配置"的分工）。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `overview/` | 预测总览仪表盘（站点分析、雷达、仪表盘、品类占比、来源等多个 ECharts 图） (see `overview/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 与 `views/predictionManagement/prediction` 视觉一致但定位不同：本目录是"看板"，后者是"管理/配置"。
- 新增图表请放入 `overview/`，并在 `overview/index.vue` 通过 `md:flex` 网格组合。

## Dependencies
### Internal
- `/@/hooks/web/useECharts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
