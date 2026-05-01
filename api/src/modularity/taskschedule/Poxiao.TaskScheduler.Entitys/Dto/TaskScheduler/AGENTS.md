<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# TaskScheduler

## Purpose
定时任务（`TimeTaskEntity`）与执行日志（`TimeTaskLogEntity`）的输入/输出 DTO。

## Key Files
| File | Description |
|------|-------------|
| `TimeTaskCrInput.cs` | 创建任务（fullName/executeType/executeContent/enCode/sortCode/enabledMark） |
| `TimeTaskUpInput.cs` | 更新任务 |
| `TimeTaskInfoOutput.cs` | 详情（含执行内容 JSON 反序列化） |
| `TimeTaskListOutput.cs` | 列表行：runCount / nextRunTime / lastRunTime / 状态，附 `startTime/endTime`（Mapster 解析自 `executeContent`） |
| `TimeTaskTaskLogListOutput.cs` | 关联日志列表 |
| `TaskLogInput.cs` | 日志查询/筛选入参 |

## For AI Agents

### Working in this directory
- `executeContent` 是 JSON 串（`ContentModel`），表单需要传完整 JSON；前端编辑器直接产出。
- `enabledMark` 1=启用，0=暂停；与 `TaskAdjustmentEnum` 配合驱动调度状态变更。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
