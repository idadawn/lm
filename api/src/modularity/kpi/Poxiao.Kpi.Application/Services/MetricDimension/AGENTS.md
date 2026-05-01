<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricDimension

## Purpose
公共维度服务。把数据模型的某个字段抽象成全局维度供多个指标复用，并能取该维度的字段值集合作为筛选选项。

## Key Files
| File | Description |
|------|-------------|
| `IMetricDimensionService.cs` | CRUD + 选项 (`GetDimOptionsAsync`) 接口 |
| `MetricDimensionService.cs` | 实现：注入 `IDbService` 取实际字段值；CRUD 走 `MetricDimensionEntity` |

## For AI Agents

### Working in this directory
- `Column` 与 `DataModelId` 是 JSON 字段，已由 `MetricDimMapper` 处理；新增字段映射时同步在 mapper 注册。
- 列表分页 + 模糊匹配 `Keyword.Contains(it.Name)`。

### Common patterns
- 维度选项通常用于前端筛选下拉，调用 `IDbService` 真正读数据库。

## Dependencies
### Internal
- `../MetricInfo`（共用 `IDbService`）、`../../../Poxiao.Kpi.Core/Entities/MetricDimension`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
