<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# AnalysisData

## Purpose
统计分析输出 DTO。承载 `AnalysisDataService` 的均值-极差控制图、正态分布与直方图结果，供前端绘制 SPC 图表。

## Key Files
| File | Description |
|------|-------------|
| `AnalysisControlChartOutput.cs` | 控制图：上/下/中管制限 (`uCL`/`cL`/`lCL`) 与 y 轴数据集 `axis` |
| `AnalysisXbarRbarOutput.cs` | Xbar-R 图：均值图 (`Average`) 与极差图 (`Range`)，每个含 `AnalysisControlChartOutput` |
| `AnalysisDataNormalListOutput.cs` | 正态分布曲线 + 直方图（`xAxis`/`yAxis` 与 `xAxisHistogram`/`yAxisHistogram`） |

## For AI Agents

### Working in this directory
- 仅放置只读的统计输出 DTO。计算逻辑保留在 `Services/AnalysisData/AnalysisDataService.cs`。
- 字段使用 `[JsonProperty(...)]` 显式 camelCase（如 `uCL`），保持与前端 ECharts/Highcharts 配置一致。

### Common patterns
- 列表初始化为 `new List<double>()` 防 null。
- 控制图限制字段命名遵循 SPC 标准 (UCL/CL/LCL)。

## Dependencies
### Internal
- `Poxiao.Kpi.Application` 命名空间（统一）
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
