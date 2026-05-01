<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Lab.Interfaces

## Purpose
实验室模块所有跨项目共享的服务契约（`I*Service` / `IFormulaParser`）。把接口与 DTO 一同放在轻量项目里，便于 Controller、Worker、其他模块以及测试项目仅引用契约而不依赖庞大的 `Poxiao.Lab` 实现。

## Key Files
| File | Description |
|------|-------------|
| `IRawDataImportSessionService.cs` | 七步导入会话契约：`Create/Get/UpdateStatus/UpdateStep/UploadAndParse/GetProductSpecMatches/UpdateProductSpecs/GetFeatureMatches/UpdateFeatures/GetReviewData/GetReviewDataPage/CompleteImport/CancelImport`，对应 `/api/lab/raw-data-import-session/*` 全部接口。 |
| `IIntermediateDataService.cs` | 中间数据服务，最关键的两个方法：`GenerateIntermediateDataAsync(rawData, productSpec, detectionColumns, layers, length, density, specVersion, batchId)`、`BatchCalculateFormulasByBatchIdAsync(batchId)`、`ApplyCalcFormulasForEntitiesAsync`、`Recalculate`、`ParseDetectionColumns`、`RepairSingleCoilWeight`，含 `ProductSpecOption` POCO。 |
| `IIntermediateDataFormulaService.cs` | 公式 CRUD + `GetAvailableColumnsAsync(includeHidden)` + `InitializeAsync` + `GetVariableSourcesAsync` + `ValidateFormulaAsync(FormulaValidationRequest)`，并含 `FormulaValidationRequest/Result/FormulaUpdateInput`。 |
| `IFormulaParser.cs` | `ExtractVariables(formula)` / `Calculate(formula, context)` / `ValidateExpression`，`context` 既支持 `Dictionary<string,object>` 也支持实体对象（计算引擎反射上下文）。 |
| `IRawDataImportSessionService.cs` | （见上）。 |
| `IRawDataService.cs` / `IRawDataValidationService.cs` | 原始数据查询/校验。 |
| `IAppearanceFeatureService.cs` / `IAppearanceFeatureCategoryService.cs` / `IAppearanceFeatureLevelService.cs` / `IAppearanceAnalysisService.cs` | 外观特征 + AI 分析契约。 |
| `IExcelImportTemplateService.cs` | Excel 模板 CRUD/校验/解析。 |
| `IMagneticDataImportSessionService.cs` / `IMagneticRawDataService.cs` | 磁性数据导入会话与列表查询。 |
| `IProductSpecService.cs` / `IProductSpecAttributeService.cs` / `IProductSpecPublicAttributeService.cs` / `IPublicDimensionService.cs` | 产品规格相关服务。 |
| `IUnitCategoryService.cs` / `IUnitConversionService.cs` / `IUnitDefinitionService.cs` | 单位维度/换算/定义。 |
| `Poxiao.Lab.Interfaces.csproj` | 项目文件（仅 ProjectReference 到 Entity 项目）。 |

## For AI Agents

### Working in this directory
- 仅放契约：**禁止** 在本目录引用 `SqlSugar/Poxiao.Lab/Service` 等实现层；保持依赖单向 `Service → Interfaces → Entity`。
- 新增方法时同步实现 `Poxiao.Lab/Service/*Service.cs`；DI 注册由 `Poxiao.Lab` 主项目按约定扫描完成。
- 注意：`IDashboardService` 与 `IMonthlyQualityReportService` **没有放在这里**，而是位于 `Poxiao.Lab.Service/IDashboardService.cs` 与 `Poxiao.Lab/Service/IMonthlyQualityReportService.cs`（历史结构，不要急于"统一"）。
- `IIntermediateDataService.GenerateIntermediateDataAsync` 的 `batchId` 是后续异步公式计算的关键 key——发布的 `IntermediateData:CalcByBatch` 事件、`CalcTaskMessage.BatchId` 都依赖它。

### Common patterns
- 所有方法返回 `Task` / `Task<T>`；POCO 辅助类（`ProductSpecOption/FormulaValidationRequest/...`）紧邻接口声明。

## Dependencies
### Internal
- `../Poxiao.Lab.Entity/`（DTO/Entity/Enum）。

### External
- 无（纯接口项目）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
