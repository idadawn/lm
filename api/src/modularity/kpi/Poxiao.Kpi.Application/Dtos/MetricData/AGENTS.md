<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricData

## Purpose
指标取数与图表数据 DTO。`MetricDataService` 利用 `IDbService` 把指标聚合结果转成单值与时间序列图表数据。

## Key Files
| File | Description |
|------|-------------|
| `MetricDataQryInput.cs` | 单指标取数入参（metricId、维度、过滤、排序、`displayOption`） |
| `MetricChartDataOutput.cs` | 图表数据：维度、x 轴、y 轴序列与基础信息 |
| `MetricDataOutput.cs` | 单值结果（`Data`/`MetricInfo` 元数据） |

## For AI Agents

### Working in this directory
- `MetricDataService` 根据 `MetricType.Basic/Derive/Composite` 走不同分支；DTO 字段需兼容三种场景。
- 时间维度走 `Granularity` + `DisplayOption`（`All`/`Latest`），缺省时回退到首个普通维度。

### Common patterns
- 服务在格式化阶段调用 `DealData(format, data)` 应用 `DataModelFormat`（保留小数、单位等）。
- 图表数据集合统一 `List<...>`，避免 null。

## Dependencies
### Internal
- `Services/MetricInfo`（取定义）、`Services/MetricInfo/IDbService`（取数）
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
