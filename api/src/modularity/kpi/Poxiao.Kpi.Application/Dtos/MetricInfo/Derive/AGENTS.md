<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Derive

## Purpose
派生指标 DTO。派生指标在基础指标之上做时间窗口/累计/同环比/排名/差值/占比等计算，对应 `MetricInfo4DeriveService`。

## Key Files
| File | Description |
|------|-------------|
| `MetricInfo4DeriveCrInput.cs` | 新建派生指标（dataModel/column/`deriveType`/`caGranularity`/`dateGranularity`/`granularityStr`/`aggType`/`rankingType`/`sortType`/format/dimensions/filters/timeDimensions/parentId/displayMode=Auto/frequency） |
| `MetricInfo4DeriveUpInput.cs` | 更新派生指标 |
| `MetricInfo4DeriveOutput.cs` | 详情，反序列化各 JSON 字段 |
| `CalculationGranularityModel.cs` | 计算区间模型（区间长度、单位等） |

## For AI Agents

### Working in this directory
- `DeriveType` 枚举值（PTD/POP/Cumulative/Moving/Difference/DifferenceRatio/TotalRatio/Ranking）决定哪些字段必填，DTO 不强制，由服务层校验。
- `CaGranularity`/`DateGranularity` 都是 `GranularityType?` 枚举（日/周/月/季/年/时/分/秒）。
- 默认 `Type = MetricType.Derive`、`DisplayMode = Auto`。

### Common patterns
- `granularityStr`（`CalculationGranularityModel`）落库前先 `ToJsonString()`，由 `MetricInfoMapper` 处理。
- 派生指标共享 `MetricInfoEntity` 表，靠 `Type=Derive` 区分。

## Dependencies
### Internal
- `Services/MetricInfo/MetricInfo4DeriveService.cs`
- `Mapper/MetricInfoMapper.cs`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
