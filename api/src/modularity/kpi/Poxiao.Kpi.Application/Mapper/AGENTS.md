<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Mapper

## Purpose
Mapster `IRegister` 实现集中地。把 KPI 模块 DTO 与 SqlSugar 实体之间的 JSON 字段（dimensions/filters/format/timeDimensions/granularityStr/parentIds/metricTag）和 `DbSchemaOutput` 等强类型对象互转规则注册到 `TypeAdapterConfig`。

## Key Files
| File | Description |
|------|-------------|
| `MetricInfoMapper.cs` | 注册 `MetricInfoCrInput`/`MetricInfo4DeriveCrInput`/`MetricInfo4CompositeCrInput` ↔ `MetricInfoEntity`，以及 `MetricInfoEntity` → `*InfoOutput`/`MetricInfo4DeriveOutput`/`MetricInfo4CompositeOutput` 的 JSON 序列化与反序列化 |
| `MetricCategoryMapper.cs` | 注册 `DbLinkListOutput→DataModel4DbOutput`、`DataModel4DbOutput→DbSchemaOutput`、`DbTableInfo→DbSchemaOutput`，统一 `parentId="-1"` |
| `MetricDimMapper.cs` | 注册 `MetricDimensionCrInput`↔`MetricDimensionEntity`，处理 `column`/`dataModelId` JSON |

## For AI Agents

### Working in this directory
- 新增 DTO/实体时，若字段需要 JSON 化，必须在此处补充 `config.ForType<TIn, TOut>().Map(...)` 规则，否则 `Adapt<>()` 会丢字段。
- 所有 mapper 类必须 `: IRegister`，运行时由框架自动扫描注册。
- 注意 `IsNotEmptyOrNull()`/`ToJsonString()`/`ToObject<T>()` 等扩展来自 `Poxiao.Infrastructure.Extension`。

### Common patterns
- 写入：`src.X == null ? "" : src.X.ToJsonString()`；读取：`src.X.IsNotEmptyOrNull() ? src.X.ToObject<T>() : new T()`/`null`。
- 列表字段通常 `Join(',')` / `Split2List(',', true)` 两端转换。

## Dependencies
### Internal
- `../Dtos/**`、`../../Poxiao.Kpi.Core/Entities/**`
### External
- Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
