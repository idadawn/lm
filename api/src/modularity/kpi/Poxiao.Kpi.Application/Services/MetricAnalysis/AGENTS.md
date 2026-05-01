<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricAnalysis

## Purpose
指标归因分析任务服务。维护 `MetricAnalysisTaskEntity`，串联多维度数据采样并对接外部归因引擎，返回任务状态、汇总文本、归因明细。

## Key Files
| File | Description |
|------|-------------|
| `IMetricAnalysisTaskService.cs` | 接口：`CreateAsync`/`UpdateAsync`/`GetStatusAsync`/`GetSummaryAsync`/`GetResultAsync` |
| `MetricAnalysisTaskService.cs` | 实现：注入 `ISqlSugarRepository<MetricAnalysisTaskEntity>` 与 `IMetricDataService`；当前 `GetStatus/GetSummary/GetResult` 返回硬编码示例 JSON，待对接真实引擎 |

## For AI Agents

### Working in this directory
- `CreateAsync` 必填 `TimeDimensions`，否则抛 `ErrorCode.K10021`。
- 创建任务时遍历每个维度调用 `_metricDataService.GetChartDataAsync` 取数；最终 entity 落库部分目前被注释，需替换为持久化与异步调度。
- `GetSummaryAsync`/`GetResultAsync` 当前返回内置 demo JSON（请在接入归因引擎时替换）。

### Common patterns
- `TaskId` 使用 `SnowflakeIdHelper.NextId().ToString()`。
- 维度筛选使用 `MetricFilterDto { Type = ByDateRang, MinValueChecked = MaxValueChecked = true }`。

## Dependencies
### Internal
- `../MetricData`、`../../../Poxiao.Kpi.Core/Entities/MetricAnalysisTask`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
