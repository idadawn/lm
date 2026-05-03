<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricAnalysis

## Purpose
指标归因分析（attribution analysis）任务 DTO。供 `MetricAnalysisTaskService` 创建/更新分析任务、查询任务状态、汇总结果与维度归因明细。

## Key Files
| File | Description |
|------|-------------|
| `MetricAnalysisTaskCrInput.cs` | 创建任务入参：`metricId`、维度集合、`timeDimensions`、起止时间 |
| `MetricAnalysisTaskUpInput.cs` | 任务更新入参 |
| `MetricAnalysisTaskOutput.cs` | 任务概览：值、变化百分比、起止值、`taskId`、`AnalysisStatus`、`TrendType` |
| `MetricAnalysisDataInput.cs` | 单维度采样数据，喂给归因引擎 |
| `MetricAnalysisResultOutput.cs` | 归因结果：`base_period`/`compared_period` 及每维度 `attribution_list` |
| `MetricAnalysisSummaryOutput.cs` | 自然语言汇总文本（含 `task_status`/`summary_content`） |

## For AI Agents

### Working in this directory
- DTO 与 Python 归因服务约定的 JSON 字段保持小写下划线（如 `task_id`、`base_period`），通过 `[JsonProperty]` 显式映射。
- 任务状态使用 `Poxiao.Kpi.Core.Enums.AnalysisStatus`。

### Common patterns
- `MetricAnalysisTaskOutput` 含 `Trend = TrendType.Down/Up`、占位 `Value`/`Percentage` 字段，当前由服务硬编码示例数据返回。
- 维度数据先经 `IMetricDataService.GetChartDataAsync` 取得，再封装到 `MetricAnalysisDataInput.Data`。

## Dependencies
### Internal
- `../../../Poxiao.Kpi.Core/Enums`（`AnalysisStatus`、`TrendType`）
- `Services/MetricData`（取数）
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
