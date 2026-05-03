<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Entity

## Purpose
实验室模块全部 SqlSugar 实体（`LAB_*` 表），覆盖原始数据、中间数据、产品规格、外观特征、判定等级、磁性数据、Excel 导入模板、报表配置、单位维度等。所有实体继承 `CLDEntityBase`，按 `.cursorrules` 字段命名规范使用 `[SugarColumn(ColumnName=...)]` 显式映射。

## Key Files
| File | Description |
|------|-------------|
| `RawDataEntity.cs` | `LAB_RAW_DATA` 原始数据；含 `[ExcelImportColumn]` 标注的导入字段（检测日期/炉号/带宽/单卷重量/检测列…）。 |
| `RawDataImportSessionEntity.cs` | `lab_raw_data_import_session` 七步导入会话；带 `SourceFileHash/Md5` 用于重复上传识别。 |
| `RawDataImportLogEntity.cs` | 导入历史日志。 |
| `IntermediateDataEntity.cs` | `LAB_INTERMEDIATE_DATA` 中间数据（约 38KB），属性大量打 `[IntermediateDataColumn]` 用于公式维护；含 `BatchId/CalcStatus/CalcErrorMessage/CalcStatusTime/JudgeStatus`。 |
| `IntermediateDataFormulaEntity.cs` | `LAB_INTERMEDIATE_DATA_FORMULA` 公式定义（CALC/JUDGE/NO）。 |
| `IntermediateDataFormulaCalcLogEntity.cs` | `LAB_INTERMEDIATE_DATA_CALC_LOG` 计算异常日志（批次/列/错误类型/详情）。 |
| `IntermediateDataJudgmentLevelEntity.cs` | 判定等级（FormulaId/Code/Name/Priority/IsDefault/Color/Condition JSON…）。 |
| `IntermediateDataColorEntity.cs` | 中间数据按值着色规则。 |
| `ProductSpecEntity.cs` | 产品规格主表。 |
| `ProductSpecAttributeEntity.cs` | 规格扩展属性表（替代旧 PropertyJson）。 |
| `ProductSpecPublicAttributeEntity.cs` | 公共规格属性。 |
| `ProductSpecVersionEntity.cs` | 规格版本快照（中间数据通过 `specVersion` 关联）。 |
| `PublicDimensionEntity.cs` / `PublicDimensionVersionEntity.cs` | 公共维度及其版本。 |
| `AppearanceFeatureEntity.cs` / `*CategoryEntity.cs` / `*CorrectionEntity.cs` | 外观特征及其分类、AI 误判修正记录。 |
| `SeverityLevelEntity.cs` | 外观特征严重等级。 |
| `MagneticRawDataEntity.cs` / `MagneticDataImportSessionEntity.cs` | 磁性数据原表与会话。 |
| `ExcelImportTemplateEntity.cs` | Excel 导入模板（持久化 `ExcelTemplateConfig` JSON 到 `F_TEMPLATE_CONFIG`）。 |
| `ReportConfigEntity.cs` | 报表列配置。 |
| `UnitCategoryEntity.cs` / `UnitDefinitionEntity.cs` | 单位维度与单位定义（基准/换算系数/精度）。 |

## For AI Agents

### Working in this directory
- **必读 `.cursorrules`**：`OEntityBase` 提供 `F_Id`、`F_TenantId`；`CLDEntityBase` 提供大写 `F_CREATORTIME/F_CREATORUSERID/F_ENABLEDMARK` 与混合大小写 `F_LastModifyTime/F_LastModifyUserId/F_DeleteMark/F_DeleteTime/F_DeleteUserId`。新增字段时若数据库列名不符合 PascalCase，需 `[SugarColumn(ColumnName="F_XXX")]` 显式映射；缺失基类字段需 `[SugarColumn(IsIgnore=true)]` 覆写。
- 所有实体必须 `[SugarTable("LAB_...")]` + `[Tenant(ClaimConst.TENANTID)]`，多租户隔离由基础设施保证。
- `IntermediateDataEntity` 的检测/厚度/叠片距离字段是固定 22 列（`Detection1..22/Thickness1..22/LaminationDist1..22`），公式上下文与范围公式（`Detection1 TO DetectionColumns`）依赖此约定。
- 添加可参与公式的列时，**同时**打 `[IntermediateDataColumn(...)]` 与 `[SugarColumn(...)]`；否则计算引擎反射不到。

### Common patterns
- 大量字段使用 `[SugarColumn(ColumnDescription="...")]` 写中文注释，作为 DDL 注释。
- 实体内通过 `#region` 分组（基础信息/检测数据/磁性数据/计算结果等）便于阅读。

## Dependencies
### Internal
- `../Attributes/`（特性挂载）、`Poxiao.Infrastructure.Contracts/Const/Security`。

### External
- SqlSugar、Newtonsoft.Json（部分字段持久化为 JSON）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
