<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dto

## Purpose
实验室模块所有 API 边界的 DTO 集合，按业务子域拆分到子目录，覆盖外观特征、判定等级、仪表盘、中间数据、公式维护、磁性数据、月度质量报表、产品规格、原始数据、报表配置、单位等。

## Key Files
| File | Description |
|------|-------------|
| `ExcelHeaderDto.cs` | Excel 解析结果的简单表头 DTO（列索引/列名）。 |
| `ExcelImportTemplateDto.cs` | Excel 导入模板 DTO + `ExcelParseHeadersInput`/`ExcelTemplateValidationInput`/`ExcelTemplateValidationResult`，支撑模板 CRUD 与解析校验链路。 |
| `ExcelImportTemplateInput.cs` | 创建/更新模板的 Cr/Up 输入。 |
| `SystemFieldDto.cs` | 系统字段定义（`field/label/dataType/sortCode/excelColumnNames/excelColumnIndex/unitId/required/decimalPlaces/defaultValue`），用于前端模板编辑。 |
| `SystemFieldResult.cs` | 系统字段查询结果包装。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `AppearanceFeature/` | 外观/缺陷特征及批量匹配、AI 分类、人工修正 DTO。 (see `AppearanceFeature/AGENTS.md`) |
| `AppearanceFeatureLevel/` | 外观特征等级 DTO。 (see `AppearanceFeatureLevel/AGENTS.md`) |
| `Dashboard/` | 驾驶舱 KPI/趋势/热力图/缺陷 Top 等 DTO。 (see `Dashboard/AGENTS.md`) |
| `IntermediateData/` | 中间数据列表/导出/计算任务/进度消息 DTO。 (see `IntermediateData/AGENTS.md`) |
| `IntermediateDataFormula/` | 公式维护 DTO（含可用列结果）。 (see `IntermediateDataFormula/AGENTS.md`) |
| `IntermediateDataJudgmentLevel/` | 判定等级 DTO。 (see `IntermediateDataJudgmentLevel/AGENTS.md`) |
| `MagneticData/` | 磁性数据导入/查询 DTO。 (see `MagneticData/AGENTS.md`) |
| `MonthlyQualityReport/` | 月度质量报表（汇总/明细/趋势/班组）DTO。 (see `MonthlyQualityReport/AGENTS.md`) |
| `ProductSpec/` | 产品规格 CRUD DTO（含扩展属性）。 (see `ProductSpec/AGENTS.md`) |
| `RawData/` | 原始数据/导入会话七步流程 DTO + 简化导入。 (see `RawData/AGENTS.md`) |
| `ReportConfig/` | 报表配置 DTO。 (see `ReportConfig/AGENTS.md`) |
| `Unit/` | 单位维度/定义/换算 DTO。 (see `Unit/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 大量 DTO 通过 `: <Entity>` 继承复用字段；新增显示字段（中文名等）时只在 `*Output` 子类上加，不要污染实体定义。
- 查询 DTO 应继承 `Poxiao.Infrastructure.Filter.PageInputBase`（已示例于 `*ListQuery`），分页字段由基类提供。
- DTO 同时使用 `System.Text.Json.JsonPropertyName` 与 `Newtonsoft.Json.JsonProperty`：保持双标注以兼容控制器和 RabbitMQ 消息序列化。

### Common patterns
- 输入命名：`*CrInput / *UpInput / *Input / *Query`；输出命名：`*ListOutput / *InfoOutput / *Result / *Output`。
- 复杂业务流程拆分为多个 DTO 文件（如 `RawData/` 七步导入会话）。

## Dependencies
### Internal
- 引用 `../Entity/*Entity.cs`、`../Enum/*`、`../Enums/*`、`Poxiao.Infrastructure.Contracts/Filter`。

### External
- Newtonsoft.Json、System.Text.Json、System.ComponentModel.DataAnnotations。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
