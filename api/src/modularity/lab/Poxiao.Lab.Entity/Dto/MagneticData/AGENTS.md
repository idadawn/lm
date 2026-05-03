<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MagneticData

## Purpose
磁性数据导入与查询 DTO。磁性数据走独立 Excel 表（按炉号 B/F/H/I/P 列结构），导入后会通过 `MAGNETIC_JUDGE` 任务回填到中间数据表的磁性字段。

## Key Files
| File | Description |
|------|-------------|
| `MagneticDataImportDto.cs` | `MagneticDataImportSessionInput`（FileName/FileData Base64）+ `MagneticDataImportItem`（`OriginalFurnaceNo`、去 K 后的 `FurnaceNo`、`IsScratched`、`PsLoss/SsPower/Hc/DetectionTime/RowIndex`）。还包含解析/确认/完成等多步 DTO。 |
| `MagneticRawDataListQuery.cs` | 列表查询输入（关键词/日期）。 |
| `MagneticRawDataListOutput.cs` | 列表输出（炉号/检测时间/Ss/Ps/Hc 等磁性指标）。 |

## For AI Agents

### Working in this directory
- 炉号字段语义：`OriginalFurnaceNo` 保留 K 标记原文用于报错；`FurnaceNo` 为去 K 后的标准化值，用于在 `LAB_INTERMEDIATE_DATA` 中通过 `FurnaceNoFormatted` 关联回填。
- `IsScratched` 与"K 标识"绑定：解析时由"是否带 K"决定；不要在 DTO 之外重新推断。
- 磁性导入完成后，`MagneticDataImportSessionService` 会发布 `TaskType=MAGNETIC_JUDGE` 的 `CalcTaskMessage`，序列化 `MagneticDataPayload` JSON——保持本目录字段与 `IntermediateData/CalcTaskMessage.cs` 中 `MagneticDataPayload` 的对应。

### Common patterns
- `RowIndex` 从 1 开始且对齐 Excel 行号，便于错误反馈给用户定位。

## Dependencies
### Internal
- `../../Entity/MagneticRawDataEntity.cs`、`MagneticDataImportSessionEntity.cs`。
- `../IntermediateData/CalcTaskMessage.cs`（`MagneticDataPayload`）。

### External
- 无（纯 POCO，FileData 为 Base64 字符串）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
