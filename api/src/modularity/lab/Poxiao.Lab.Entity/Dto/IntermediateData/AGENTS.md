<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# IntermediateData

## Purpose
中间数据 (`LAB_INTERMEDIATE_DATA`) 的 API 与 MQ 消息 DTO。承担列表查询、导出、单条更新、计算/判定任务消息（API ↔ Worker 之间通过 `lab.calc.task`/`lab.calc.progress` 队列）以及批量计算结果。

## Key Files
| File | Description |
|------|-------------|
| `IntermediateDataDto.cs` | `*ListQuery`（关键词/产品规格/批次/日期/`CalcStatus/JudgeStatus`/排序 JSON）+ `*ListOutput`、`*InfoOutput`、若干 `Update*Input`（性能/外观/基础信息/单卷重量），加上 `ProductSpecOption`。最庞杂的 DTO 文件。 |
| `IntermediateDataColorDto.cs` | 中间数据按值着色规则的 DTO。 |
| `CalcTaskMessage.cs` | API → Worker 计算任务消息：`BatchId/TenantId/UserId/SessionId/TotalCount/UnitPrecisionsJson/TaskType(CALC/JUDGE/MAGNETIC_JUDGE)/IntermediateDataId(per-item)/IntermediateDataIds/MagneticDataJson`，含 `MagneticDataPayload`。 |
| `CalcProgressMessage.cs` | Worker → API 进度回报：`Total/Completed/SuccessCount/FailedCount/Status(PROCESSING/COMPLETED/FAILED)/Message`，前端 WebSocket 消费。 |
| `FormulaCalculationResult.cs` | `Recalculate`/`BatchCalculateFormulasByBatchIdAsync` 返回的成功/失败统计。 |
| `IntermediateDataCalcLogDto.cs` | `LAB_INTERMEDIATE_DATA_CALC_LOG` 列表 DTO（批次/列名/公式类型/错误类型/详情）。 |
| `IntermediateDataExportInput.cs` | Excel 导出筛选 + 列选择参数。 |

## For AI Agents

### Working in this directory
- `CalcTaskMessage` 是 RabbitMQ 持久化消息：**追加字段时不能破坏 worker 端反序列化**；新字段必须给默认值或可空。
- `TaskType="MAGNETIC_JUDGE"` 时使用 `MagneticDataJson` + `MagneticDataPayload`，Worker 会按 `FurnaceNo` 局部更新中间数据磁性字段；不要改 `FurnaceNo` 的语义（`FurnaceNoFormatted`）。
- `IntermediateDataId` 单值表示 per-item 并发模式，`IntermediateDataIds` 表示批次模式；两者互斥。
- 排序传 `SortRules` 为 JSON 字符串（前端约定），服务端在 `IntermediateDataService.GetList` 中解析。

### Common patterns
- 所有进度/任务消息都带 `TenantId/UserId` 用于多租户与 WebSocket 推送。

## Dependencies
### Internal
- `../../Entity/IntermediateDataEntity.cs`；`../../Enums/IntermediateDataCalcStatus.cs`。
- 被 `Poxiao.Lab/Service/CalcTaskPublisher.cs`、`CalcProgressConsumer.cs`、`IntermediateDataFormulaBatchCalculator.cs`、`IntermediateDataExportService.cs` 消费。

### External
- Poxiao.Infrastructure.Filter (`PageInputBase`)。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
