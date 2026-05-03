<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricInfo

## Purpose
指标定义 DTO 顶层目录。覆盖三类指标（Basic/Derive/Composite）的新增/更新/查询/详情 DTO，以及共用的过滤、维度、聚合等子模型。

## Key Files
| File | Description |
|------|-------------|
| `MetricInfoCrInput.cs` | 新建基础指标（数据模型、列、聚合、维度、过滤、时间维度、显示模式、存储频率、`MetricDataType`） |
| `MetricInfoUpInput.cs` | 更新基础指标 |
| `MetricInfoListQueryInput.cs` | 列表查询（关键字、分类、标签、启用状态、是否含已删除） |
| `MetricInfoListOutput.cs` | 列表项（含枚举字段字符串化） |
| `MetricInfoInfoOutput.cs` | 详情，JSON 字段反序列化为 `TableFieldOutput`/`MetricFilterDto` 等 |
| `MetricInfoAllOutput.cs` | 全量精简列表（用于选择器） |
| `MetricInfoDimensionsOutput.cs` | 指标共有维度的并集 |
| `MetricInfoDimQryCrInput.cs` | 维度查询入参 |
| `MetricFilterDto.cs` | 过滤条件 DTO（Where/范围/时间范围） |
| `MetricTimeDimensionDto.cs` | 时间维度（字段、粒度、展示选项） |
| `MetricAggInfoListOutput.cs` | 聚合方式列表（按字段类型推断） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Composite/` | 复合指标（公式表达式）专用 DTO (see `Composite/AGENTS.md`) |
| `Db/` | 数据源/Schema/字段查询 DTO (see `Db/AGENTS.md`) |
| `Derive/` | 派生指标（同环比、累计、移动、占比、排名）DTO (see `Derive/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 所有 `*CrInput`/`*UpInput` 与 `MetricInfoEntity` 之间的 JSON 字段（`dimensions`/`filters`/`format`/`timeDimensions`）必须在 `MetricInfoMapper` 中注册。
- 指标编码 `code` 与名称 `name` 唯一性由服务层 `CheckCodeAsync`/`CheckNameAsync` 保证。
- `DisplayMode` 默认值：基础 `General`、派生 `Auto`、复合按场景设置。

### Common patterns
- DTO 用强类型（`DbSchemaOutput`、`TableFieldOutput`、`List<MetricFilterDto>`），落库前由 Mapster `IRegister` 序列化为字符串字段。
- 标签 `MetricTag` 落库为逗号分隔的 ID 字符串。

## Dependencies
### Internal
- `../../Mapper/MetricInfoMapper.cs`
- `../../../Poxiao.Kpi.Core/Entities/MetricInfo`、`Enums`
- `Poxiao.Infrastructure`（`TableFieldOutput`、`DBAggType`、`DataModelFormat`）
### External
- Newtonsoft.Json, SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
