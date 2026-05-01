<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricAnalysisTask

## Purpose
归因分析任务持久化实体。记录某个指标在指定时间范围与维度组合下的归因任务及其状态。

## Key Files
| File | Description |
|------|-------------|
| `MetricAnalysisTaskEntity.cs` | 表 `metric_analysis_task` (`CUEntityBase`)：`metric_id`/`time_dimensions`/`dimensions`/`filters`(JSON)/`start_data`/`end_data`/`status`(`AnalysisStatus`，默认 `InProgress`) |

## For AI Agents

### Working in this directory
- 三个 JSON 字段（`time_dimensions`/`dimensions`/`filters`）都是字符串列，DTO 端使用 `Mapper` 反序列化。
- `Status` 默认值在实体中即设为 `InProgress`。

### Common patterns
- 任务 ID 由 `SnowflakeIdHelper.NextId()` 生成。

## Dependencies
### Internal
- `../../Enums/AnalysisStatus`
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
