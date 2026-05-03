<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Lab.Entity

## Purpose
实验室模块的实体/DTO/枚举/配置项目（`Poxiao.Lab.Entity.csproj`），被 `Poxiao.Lab.Interfaces`、`Poxiao.Lab` 服务实现以及 worker 端共享引用。它定义了 `RAW_DATA → INTERMEDIATE_DATA → 公式计算 → 判定` 整个流水线在 SqlSugar 数据库与 API/MQ 边界上的全部数据契约。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Lab.Entity.csproj` | 项目文件，引用 `Poxiao.Infrastructure`，依赖 `SharpZipLib`、`SkiaSharp`（图片/二维码/Excel）；显式排除 `Entity/Metric/**`。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | C# 特性：`ExcelImportColumn`、`IntermediateDataColumn`（公式可见性/单位/范围）。 (see `Attributes/AGENTS.md`) |
| `Config/` | 强类型配置：`LabOptions`（Formula 精度）、`ExcelTemplateConfig`。 (see `Config/AGENTS.md`) |
| `Dto/` | 按业务领域分子目录的 DTO 集合（输入/输出/查询）。 (see `Dto/AGENTS.md`) |
| `Entity/` | SqlSugar 实体（`LAB_*` 表），均继承 `CLDEntityBase`。 (see `Entity/AGENTS.md`) |
| `Enum/` | 业务枚举：`QualityStatusEnum`、`ExcelImportTemplateCode`。 (see `Enum/AGENTS.md`) |
| `Enums/` | 流水线状态枚举：`IntermediateDataCalcStatus`、`IntermediateDataFormulaType`。 (see `Enums/AGENTS.md`) |
| `Extensions/` | 产品规格扩展（多数已 `[Obsolete]`，向 `LAB_PRODUCT_SPEC_ATTRIBUTE` 表迁移）。 (see `Extensions/AGENTS.md`) |
| `Models/` | POCO 解析模型：`FurnaceNo` 炉号封装/正则解析。 (see `Models/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 此项目**不允许直接 IO 或 EF/SqlSugar 调用**：仅放置数据契约。业务逻辑写到 `Poxiao.Lab/Service` 中。
- 实体须继承 `CLDEntityBase` 并加 `[SugarTable("LAB_*")]` + `[Tenant(ClaimConst.TENANTID)]`；列名按 `.cursorrules` 规范用 `[SugarColumn(ColumnName=...)]` 显式映射（lab 表偏向大写 + 下划线，例如 `F_FURNACE_NO`）。
- 中间数据相关属性新增列时同步打上 `[IntermediateDataColumn]`，`Sort/DisplayName/Unit/IsCalculable` 决定其在公式维护界面与精度处理中的可见性。

### Common patterns
- DTO 普遍同时标注 `[JsonPropertyName]`（System.Text.Json）和 `[JsonProperty]`（Newtonsoft），双序列化器兼容。
- 入参/出参通过继承实体复用字段，再扩展显示字段（如 `*ListOutput : *Entity`）。

## Dependencies
### Internal
- `api/src/common/Poxiao.Common/Poxiao.Infrastructure.csproj`（`CLDEntityBase`、`PageInputBase`、`Tenant`）。

### External
- SqlSugar、Newtonsoft.Json、SharpZipLib、SkiaSharp、System.Drawing.Common。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
