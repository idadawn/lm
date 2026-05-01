<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Service

## Purpose
实验室模块**所有业务服务实现**集中地。覆盖原始数据七步导入、中间数据计算/判定、公式解析与批量计算、外观特征 AI 匹配、磁性数据导入回填、月度质量报表、驾驶舱、单位换算、产品规格版本化等。配合 `Poxiao.Lab.Interfaces` / `Poxiao.Lab.Service` 中的契约 DI 注册。

## Key Files
| File | Description |
|------|-------------|
| `RawDataImportSessionService.cs` | 七步导入会话核心实现（约 145KB）：上传/解析/产品规格/重复/特性/复核/CompleteImport（写 RAW + INTERMEDIATE + 发布 `IntermediateData:CalcByBatch`）。 |
| `IntermediateDataService.cs` | 中间数据 CRUD + `GenerateIntermediateDataAsync` + 同步/批量公式计算桥接（约 100KB）。 |
| `IntermediateDataFormulaBatchCalculator.cs` | 批量计算引擎（约 100KB）：依赖图、拓扑序、`FormulaParser` 调用、范围公式 `[Start] TO [End]`、单位换算、精度链路、CALC/JUDGE 错误分类（DEPENDENCY/FORMULA/UNIT/SET_VALUE）写入 `LAB_INTERMEDIATE_DATA_CALC_LOG`。 |
| `IntermediateDataFormulaCalcHelper.cs` | 上下文构建：把实体可数值字段、`Length/Layers/Density`、`Detection1..22/Thickness1..22/LaminationDist1..22`、`DetectionColumns` 注入计算上下文。 |
| `IntermediateDataFormulaService.cs` | 公式 CRUD/Initialize/可用列/变量来源/校验。 |
| `IntermediateDataJudgmentLevelService.cs` | 判定等级 CRUD + `generate-judgment` 自动生成 JSON 规则框架（按优先级，把 IsDefault 等级名写入 `DefaultValue`）。 |
| `IntermediateDataExportService.cs` | 中间数据 Excel 导出（动态列、合并表头、按筛选条件）。 |
| `IntermediateDataColorService.cs` | 中间数据按值着色规则。 |
| `CalcTaskPublisher.cs` / `CalcProgressConsumer.cs` | RabbitMQ 发布 `lab.calc.task` 任务、消费 `lab.calc.progress` 进度，转发 WebSocket。 |
| `RawDataService.cs` / `RawDataValidationService.cs` | 原始数据查询与校验（含炉号正则与单元格格式检查）。 |
| `MagneticDataImportSessionService.cs` / `MagneticRawDataService.cs` | 磁性数据导入：解析→确认→完成→发布 `MAGNETIC_JUDGE` 任务回填。 |
| `AppearanceFeatureService.cs` / `AppearanceFeatureRuleMatcher.cs` / `AppearanceFeatureCategoryService.cs` / `AppearanceFeatureLevelService.cs` / `AppearanceAnalysisService.cs` / `FeatureLearningService.cs` | 外观特征 + AI 识别 + 规则学习 + 人工修正全链路。 |
| `MonthlyQualityReportService.cs` / `IMonthlyQualityReportService.cs` | 月度质量报表（约 55KB）+ 接口本体。 |
| `DashboardService.cs` / `IDashboardService.cs` | 驾驶舱实现 + **接口副本**（注意：与 `Poxiao.Lab.Service/IDashboardService.cs` 重复，需同步修改）。 |
| `ExcelImportTemplateService.cs` | 模板 CRUD + Excel 表头解析 + 模板验证（约 32KB）。 |
| `ProductSpecService.cs` / `ProductSpecVersionService.cs` / `ProductSpecAttributeService.cs` / `ProductSpecPublicAttributeService.cs` / `PublicDimensionService.cs` | 产品规格主表 + 版本快照 + 扩展属性表（取代旧 PropertyJson） + 公共维度。 |
| `UnitDefinitionService.cs` / `UnitCategoryService.cs` / `UnitConversionService.cs` | 单位维度/换算（基准切换会重算同维度其他单位）。 |
| `ReportConfigService.cs` | 报表列配置 CRUD（驱动月报动态列）。 |
| `UnitLogicTestController.cs` | 单位换算逻辑测试 Controller（**位置异常**，是 Controller 而非 Service，可能随重构搬走）。 |
| `temp_column_definitions.cs` | 临时列定义脚本（约 10KB）；`temp_*` 命名说明是过渡产物，应在重构后清理。 |

## For AI Agents

### Working in this directory
- 计算流水线核心是 `IntermediateDataFormulaBatchCalculator`：修改公式语义前必读它的依赖图构建、循环依赖分类（写 `DEPENDENCY` 错误并跳过）、错误分类（`FORMULA/UNIT/SET_VALUE`）以及精度优先级链。任何调整都要在 `IntermediateDataFormulaCalcHelper` 中保持上下文契约一致。
- `RawDataImportSessionService.CompleteImport` 是事务边界：解析模板配置→构造 `unitPrecisions`→读取 `validData` JSON→按 `ProductSpecId` 分组→`GenerateIntermediateDataAsync(batchId)`→事务写 RAW+INTERMEDIATE→发布 `IntermediateData:CalcByBatch` 事件。变更时勿打破"事务先提交，事件再发布"的顺序。
- `SimpleImport` 路径若需触发计算，必须复用 `CompleteImport` 的事件发布策略，否则该批次永远停留在 `PENDING`。
- DI 注册依赖约定式扫描；新增 `*Service.cs` 通常会被自动注册，但若使用了泛型/工厂或非约定命名，需在 `Poxiao.Lab.Web.Core` 启动模块中手动登记。
- `IDashboardService.cs` 在本目录与 `../../Poxiao.Lab.Service/IDashboardService.cs` 各有一份；同步修改避免编译歧义。
- 文件 `temp_column_definitions.cs` 与 `UnitLogicTestController.cs` 属过渡/调试代码——重构时优先评估能否删除。
- 多服务文件超过 50KB（Calculator/IntermediateDataService/RawDataImportSessionService/AppearanceFeatureService/MonthlyQualityReportService）。改动时倾向"提取私有 helper 类到独立文件"而非继续堆叠到大类中。

### Common patterns
- Controller 入口在 `Poxiao.Lab.Web.Core/Controller/`，本目录服务通过构造函数注入 SqlSugar `ISqlSugarClient`、`IEventBus`、`ILogger<T>`、其他 `IXxxService`。
- 事件驱动：`IntermediateData:CalcByBatch`、`MAGNETIC_JUDGE` 通过 `Poxiao.Lab/EventBus/` 订阅。
- `SqlSugar` 事务用 `db.Ado.UseTranAsync` 包裹多表写入；外部事件发布在事务提交后。
- 跨数据库兼容：禁用 SQL Server 专属 `(NOLOCK)`/T-SQL 函数，使用 SqlSugar 跨库 API。

## Dependencies
### Internal
- `../../Poxiao.Lab.Entity/`、`../../Poxiao.Lab.Interfaces/`、`../../Poxiao.Lab.Service/IDashboardService.cs`。
- `../EventBus/`、`../Helpers/`、`../Extensions/`、`../Interfaces/`（同模块辅助层）。
- `Poxiao.Common/Poxiao.Infrastructure`（基础设施 + SqlSugar 封装）。

### External
- SqlSugar、RabbitMQ.Client、Newtonsoft.Json、System.Text.Json、SkiaSharp（导出图表/图像处理），可能涉及 NPOI/MiniExcel（按 csproj 实际为准）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
