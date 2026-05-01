<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# RawData

## Purpose
原始数据导入会话七步流程 (`/api/lab/raw-data-import-session/*`) 的全部 DTO，加上 `SimpleImport`（一键简化导入）契约。是把 Excel 解析后的数据落到 `LAB_RAW_DATA` 与 `LAB_INTERMEDIATE_DATA` 的入口数据载体。

## Key Files
| File | Description |
|------|-------------|
| `RawDataDto.cs` | 原始数据列表/详情 DTO（按炉号、生产/检测日期、产品规格关联）。 |
| `RawDataImportSessionDto.cs` | 七步流程关键输入输出：`RawDataImportSessionInput(FileName/ImportStrategy=append|overwrite/FileData/ForceUpload)` + `Step1` 上传解析输出（`PreviewData/TotalRows/ValidDataRows/PreviousReadRows/NoChanges/NoNewRows`）+ `RawDataProductSpecMatchOutput`（含 `MatchStatus=matched/unmatched/manual` 与 `DetectionValues`）+ 重复选择、特性匹配、复核分页、完成导入相关 DTO。 |
| `SimpleImportInput.cs` | `SimpleImport`：单步导入 + `SimpleImportOutput`（TotalRows/SuccessRows/FailRows/SkippedRows/Errors/DuplicateFurnaceNos）。简化路径不经过中间会话，需自行触发计算。 |

## For AI Agents

### Working in this directory
- 七步流程严格对应 `IRawDataImportSessionService`：`Create → UploadAndParse(step1) → GetProductSpecMatches/UpdateProductSpecs(step2) → UpdateDuplicateSelections → GetFeatureMatches/UpdateFeatures(step3) → GetReviewData/GetReviewDataPage(step4) → CompleteImport`。新增 DTO 需对齐方法签名。
- `ImportStrategy="overwrite"` 时仅更新带材重量/断头数/单卷重量/检测值；不要让其覆盖审计字段（`F_CREATORTIME` 等保留）。
- `ForceUpload=true` 用于绕过 `SourceFileMd5/Hash` 重复检测；只有在用户二次确认时才设置。
- `RawDataProductSpecMatchOutput.MatchStatus` 取值 `matched/unmatched/manual`（字符串），不要换成 enum，前端按字符串过滤。
- `SimpleImport` 路径如需触发计算，应与 `CompleteImport` 一致地发布 `IntermediateData:CalcByBatch` 事件（参考根模块 AGENTS.md 中的流水线说明）。

### Common patterns
- 所有上传 DTO 用 Base64 字符串传文件 (`FileData`)，避免依赖 multipart/form-data。
- 检测值用 `Dictionary<int, decimal?>(列号 → 值)` 表达，键 1..N，与产品规格 `DetectionColumns` 对齐。

## Dependencies
### Internal
- `../../Entity/RawDataEntity.cs`、`RawDataImportSessionEntity.cs`、`RawDataImportLogEntity.cs`。
- `../../Poxiao.Lab.Interfaces/IRawDataImportSessionService.cs`。

### External
- 无（纯 POCO）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
